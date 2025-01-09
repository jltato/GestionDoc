using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SUAP_PortalOficios.Data;
using SUAP_PortalOficios.Data.Repository.Interfaces;
using SUAP_PortalOficios.Hubs;
using SUAP_PortalOficios.Models;
using System.IO.Compression;
using System.Security.Claims;
using iText.Kernel.Pdf;
using static SUAP_PortalOficios.Views.Shared.Components.BusquedaInterno;
using iText.Kernel.Utils;
using SUAP_PortalOficios.Extensions;


namespace SUAP_PortalOficios.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]/{indice?}")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly MyDbContextSuap _contextSuap;
        private readonly UserManager<MyUser> _userManager;
        private readonly string _basePath;
        private readonly ILogger<DocumentController> _logger;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly IUserPermissionsService _userPermissionsService;
        public DocumentController(MyDbContext context, 
            MyDbContextSuap contextSuap, 
            UserManager<MyUser> userManager, 
            IConfiguration configuration, 
            ILogger<DocumentController> logger,
            IHubContext<UpdateHub> hubContext,
            IUserPermissionsService userPermissionsService)
        {
            _context = context;
            _contextSuap = contextSuap;
            _userManager = userManager;
            _basePath = configuration["FileStorage:BasePath"];
            _logger = logger;
            _hubContext = hubContext;
            _userPermissionsService = userPermissionsService;
        }


        /// <summary>
        /// POST: api/Create ///
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="file"></param>
        /// <returns> StatusCode (200,Guardado) </returns>
        /// 
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(int indice, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.ContentType != "application/pdf")
                return BadRequest("Invalid File. Only PDF files are allowed.");
             
            try
            {
                int oficioId;
                var up = _userPermissionsService.GetPermissionsAsync(User);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var section = up.Result.Sections.First();
                var establecimiento = up.Result.Scopes.ToList();
                var document = new DocumentPdf();
                if (userId != null)
                {
                    if (indice == 0)
                    {
                        var oficio = new Oficios
                        {
                            IdEstado = 1,
                            UserId = userId,
                            IdEstabACargo = establecimiento.First()
                        };

                        _context.Oficios.Add(oficio);
                        await _context.SaveChangesAsync();

                        oficioId = oficio.IdOficio;
                        document.FileName = file.FileName;
                        // Notificar a todos los clientes conectados que la tabla ha sido actualizada
                        var userList = await UsersByOficioAsync(oficioId);

                        await _hubContext.Clients.Users(userList).SendAsync("ReceiveTableUpdate");
                        userList.Add(userId);
                        await _hubContext.Clients.Users(userList).SendAsync("ReceiveCantidadesUpdate");
                        _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Creo el oficio Id:{oficio.IdOficio}");
                    }
                    else
                    {
                        section = section == 1 ? 3 : section;
                        oficioId = indice;
                        document.FileName = (await _context.Sections
                                            .Where(a => a.SectionId == section)
                                            .Select(s => s.Name)
                                            .FirstOrDefaultAsync() ?? "Documento PDF").ToString();
                    }
                   
                    document.FileSize = file.Length.ToString();
                    document.fechaCarga = DateTime.Now;
                    document.EliminadoLogico = false;
                    document.OficioId = oficioId;
                    

                    _context.DocumentPdf.Add(document);                   

                    await _context.SaveChangesAsync();

                    int documentId = document.DocId;
                    var path = $"{DateTime.Now:yyyyMMdd_HHmmss}_{documentId}";

                    var result = await SaveFileAsync(file, path);

                    if (result)
                    {
                        document.src = path;
                        _context.DocumentPdf.Update(document);
                        await _context.SaveChangesAsync();

                        var userList = await UsersByOficioAsync(oficioId);
                        await _hubContext.Clients.Users(userList).SendAsync("ReceivePdfUpdate");
                        _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Agrego el documento {document.FileName} al oficio Id:{oficioId}");

                        return StatusCode(200, "guadado");

                    }
                    else
                    {
                        return StatusCode(500, "internal server error");
                    }
                }
                else
                {
                    return BadRequest("No User.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Error creando un documento" + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }


        /// <summary>
        /// GET: api/Document/GetDocumentPdf
        /// </summary>
        /// <param name="IdOficio"></param>
        /// <param name="id"></param>
        /// <returns>File(fileBytes, "application/pdf")</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetDocumentPdf(int IdOficio, int id)
        {
            try
            {
                var documentos = await _context.DocumentPdf
                            .Where(a => a.OficioId == IdOficio && a.EliminadoLogico == false)
                            .ToListAsync();
                var cantidadArchivos = documentos.Count();
                if (id < 0 || id >= cantidadArchivos)
                {
                    return StatusCode(400, "Índice de archivo fuera de rango");
                }

                var document = documentos[id];

                if (document != null)
                {
                    // Añadir la cantidad de archivos en un header HTTP
                    Response.Headers.Append("X-Total-Documents", cantidadArchivos.ToString());
                    Response.Headers.Append("Nombre", document.FileName?.ToString());
                    var filePath = Path.Combine(_basePath, document.src ); // Ruta del archivo en el servidor

                    if (System.IO.File.Exists(filePath)) // Verificar si el archivo existe
                    {
                        var fileBytes = await DecompressFileToByteArrayAsync(filePath);

                        
                        return File(fileBytes, "application/pdf");
                    }
                    else
                    {
                        // Si no se encuentra el archivo                       
                        return NotFound("El archivo no se encontró en el servidor.");
                    }
                }
                else
                {
                    _logger.LogError("No se encontró el archivo en la BBDD");
                    return StatusCode(500, "No se encuentra el archivo");
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error buscando un documento" + ex.Message);
                return StatusCode(500, "Internal server error");
            }            
        }

        /// <summary>
        /// ////DELETE: api/Document/DeleteDocument
        /// </summary>
        /// <param name="IdOficio"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Asesor, Coordinador")]
        [ValidateAntiForgeryToken]
        [HttpDelete]
        public async Task<IActionResult> DeleteDocument(int IdOficio, int id)
        {
            try
            {
                var documentos = await _context.DocumentPdf
                            .Where(a => a.OficioId == IdOficio && a.EliminadoLogico == false)
                            .ToListAsync();
                var cantidadArchivos = documentos.Count();
                if (id < 1 || id >= cantidadArchivos)
                {
                    return StatusCode(400, "Índice de archivo fuera de rango");
                }

                var document = documentos[id];

                if (document != null)
                {
                    document.EliminadoLogico = true;
                    _context.DocumentPdf.Update(document);
                    await _context.SaveChangesAsync();
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Eliminó el documento {document.FileName} asociado al oficio Id:{IdOficio}");
                    return StatusCode(200, "Archivo eliminado con exito");
                }

                return StatusCode(500, "Server Error");

            }
            catch (Exception ex)
            {
                _logger.LogError("Error borrando un documento" + ex.Message);
                Console.WriteLine(ex.Message, ex);
                return StatusCode(500, "Se produjo una excepción");
            }
        }

        // GET: api/Document/GetDropDownData
        [HttpGet]
        public async Task<IActionResult> GetDropDownData()
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            var plazos = await _context.Plazo
                .Select(p => new DropDownOption { Id = p.IdPlazo, Nombre = p.PlazoName })
                .ToListAsync();

            var tipos = await _context.TipoOficios
                .Select(t => new DropDownOption { Id = t.IdTipoOficio, Nombre = t.TipoOficioNombre })
                .ToListAsync();

            var medios = await _context.MedioIng
                .Select(m => new DropDownOption { Id = m.IdMedio, Nombre = m.MedioName })
                .ToListAsync();

            var tribunales = await _contextSuap.Tribunals
                .Where(t => t.EstadoTribu == 1)
                .Select(tr => new DropDownOption { Id = tr.IdTribunal, Nombre = tr.Descripcion })
                .ToListAsync();

            var Establecimientos = await _context.Scopes
                .Select(s => new DropDownOption { Id = s.ScopeId, Nombre = s.ScopeName })
                .ToListAsync();

            if (plazos == null || tipos == null || medios == null || tribunales == null || Establecimientos == null )
            {
                return NotFound();
            }

            var result = new
            {
                Plazos = plazos,
                Tipos = tipos,
                Medios = medios,
                Tribunales = tribunales,
                Establecimientos = Establecimientos
            };

            return Ok(result);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public async Task<IActionResult> GetScopes(bool BusquedaExtendida)
        {
           
            List<DropDownOption> scopes;
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (BusquedaExtendida)
            {
                scopes = await _context.Scopes
                            .Where(up => up.ScopeId != 1)
                            .Select(s => new DropDownOption
                            {
                                Id = s.ScopeId,
                                Nombre = s.ScopeName
                            })
                           .ToListAsync();
            }
            else
            {
                scopes = await _context.UserPermissions
                           .Include(up => up.Scope)
                           .Where(up => up.UserId == userId)
                            .Select(s => new DropDownOption
                            {
                                Id = s.ScopeId,
                                Nombre = s.Scope.ScopeName
                            })
                           .ToListAsync();
            }
            if (scopes == null)
            {
                return NotFound();
            }
            return Ok(scopes);
        }

        [ValidateAntiForgeryToken] 
        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            List<Sections> sections;

            sections = await _context.Sections
                            .Where(up => up.SectionId != 1 && up.SectionId !=2)    
                            .OrderBy(up => up.Name)
                           .ToListAsync();
            
            if (sections == null)
            {
                return NotFound();
            }
            return Ok(sections);
        }
  
        public async Task<IActionResult> Download(int indice)
        {
            try
            {
                var oficio = await _context.Oficios
                                    .Include(o => o.MedioIng)
                                    .Include(o => o.interno_X_Oficios)
                                    .Where(o => o.IdOficio == indice)
                                    .FirstOrDefaultAsync();

                var Legajo = oficio?.interno_X_Oficios?.Select(io => io.Legajo).FirstOrDefault().ToString() ?? "";
                var Tribunal = await _contextSuap.Tribunals.FindAsync(oficio?.IdTribunal);
                var NombreTrib = "";
                if (Tribunal != null)
                {
                    NombreTrib = Tribunal.Nombre.ToString();
                }
                var documents = await _context.DocumentPdf
                                    .Where(d => d.OficioId == indice && d.EliminadoLogico == false)
                                    .ToListAsync();

                // Crear el archivo ZIP en memoria
                using (var zipStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (var document in documents)
                        {
                            // Crear la ruta completa del archivo
                            var filePath = Path.Combine(_basePath, document.src);

                            // Verificar si el archivo existe en el servidor
                            if (System.IO.File.Exists(filePath))
                            {
                                // Descomprimir el archivo GZip
                                byte[] decompressedFile = await DecompressFileToByteArrayAsync(filePath);

                                // Crear un archivo dentro del ZIP con el nombre original y extensión .pdf
                                var fileName = document.FileName + ".pdf"; 
                                var zipEntry = zip.CreateEntry(fileName);

                                // Escribir el contenido descomprimido al archivo en el ZIP
                                using (var zipEntryStream = zipEntry.Open())
                                {
                                    await zipEntryStream.WriteAsync(decompressedFile, 0, decompressedFile.Length);
                                }
                            }
                        }
                    }

                    // Convertir el archivo ZIP en un arreglo de bytes
                    var zipBytes = zipStream.ToArray();

                    // Devolver el archivo ZIP para descarga
                    return File(zipBytes, "application/zip", $"{oficio.MedioIng?.MedioName.ToString()}-{NombreTrib}-{Legajo}.zip");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error descargando un documento" + ex.Message);
                return NotFound();
            }

        }

        private async Task<bool> SaveFileAsync(IFormFile file, string path)
        {
            try
            {
                // Definir la ruta de almacenamiento del archivo comprimido
                var compressedFilePath = Path.Combine(_basePath, path);

                // Comprimir y guardar el archivo
                using (var outputStream = new FileStream(compressedFilePath, FileMode.Create))
                {
                    using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        using (var inputStream = file.OpenReadStream())
                        {
                            await inputStream.CopyToAsync(compressionStream);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error guardando un documento" + ex.Message);
                return false;
            }
        }

        private async Task<byte[]> DecompressFileToByteArrayAsync(string compressedFilePath)
        {
            try
            {               
                if (!System.IO.File.Exists(compressedFilePath))
                {
                    throw new FileNotFoundException($"El archivo no se encontró: {compressedFilePath}");
                }

                // Crear un MemoryStream para almacenar el contenido descomprimido
                using (var outputStream = new MemoryStream())
                {
                    // Abrir el archivo comprimido para lectura
                    using (var compressedFileStream = new FileStream(compressedFilePath, FileMode.Open))
                    {
                        // Usar GZipStream para descomprimir el archivo
                        using (var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                        {
                            // Copiar el contenido descomprimido al MemoryStream
                            await decompressionStream.CopyToAsync(outputStream);
                        }
                    }

                    // Devolver el contenido descomprimido como un arreglo de bytes
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al descomprimir el archivo {compressedFilePath}: {ex.Message}");
                throw; // Relanzar la excepción para que pueda ser manejada externamente si es necesario
            }
        }

        private async Task<List<string>> UsersByOficioAsync(int oficioId)
        {
            try
            {
                var thisUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var listUsers = await _context.Users
                    .FromSqlRaw(@"
                SELECT U.Id 
                FROM UserPermissions UP
                INNER JOIN AspNetUsers U ON U.Id = UP.UserId
                WHERE 
                    (
                        EXISTS (
                            SELECT 1 
                            FROM Oficios_x_Area OXA 
                            WHERE 
                                OXA.OficiosId = 125 
                                AND OXA.SectionId = UP.SectionId 
                                AND OXA.ScopeId = UP.ScopeId
                        )
                    )
                    OR 
                    (
                        EXISTS (
                            SELECT 1 
                            FROM Oficios O 
                            WHERE O.IdEstabACargo = UP.ScopeId AND O.IdOficio = {0}
                        )
                    )", oficioId)
                    .Select(u => u.Id)
                    .ToListAsync();

                var listUsersFiltered = listUsers
                    .Where(userName => userName != null && userName != thisUser)
                    .Cast<string>()
                    .ToList();

                return listUsersFiltered;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return null!;
            }
        }

        [HttpGet]        
        public async Task<IActionResult> CombineAndPrint(int Id)
        {
            if (Id <= 0)
                return BadRequest("ID inválido.");

            var oficios = await _context.DocumentPdf
                .Where(o => o.OficioId == Id && o.EliminadoLogico != true)
                .Select(o => o.src)
                .ToListAsync();

            if (!oficios.Any())
                return NotFound("No se encontraron documentos para combinar.");

            var pdfPaths = oficios
                .Select(o => Path.Combine(_basePath, o))
                .Where(System.IO.File.Exists)
                .ToList();

            if (!pdfPaths.Any())
                return NotFound("No se encontraron archivos existentes en el servidor.");

            try
            {
                var pdfByteArrays = new List<byte[]>();

                foreach (var pdfPath in pdfPaths)
                {
                    var pdfBytes = await DecompressFileToByteArrayAsync(pdfPath);
                    pdfByteArrays.Add(pdfBytes);
                }

                // Llamar a la función Combine
                var combinedPdfBytes = Combine(pdfByteArrays);

                // Verificar que no esté vacío
                if (combinedPdfBytes.Length == 0)
                {
                    return StatusCode(500, "El archivo PDF combinado está vacío.");
                }

                // Convertir a Base64 para enviarlo como respuesta JSON (si es necesario)
                var base64Pdf = Convert.ToBase64String(combinedPdfBytes);

                // O devolver directamente como un archivo PDF descargable
                return Content(base64Pdf, "text/plain");


            }
            catch (Exception ex)
            {
                // Loguear el error para diagnóstico
                _logger.LogError(ex.Message, "Error al combinar documentos PDF");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }

        }

        public byte[] Combine(IEnumerable<byte[]> pdfs)
        {
            using (var writerMemoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(writerMemoryStream))
                {
                    using (var mergedDocument = new PdfDocument(writer))
                    {
                        var merger = new PdfMerger(mergedDocument);

                        foreach (var pdfBytes in pdfs)
                        {
                            using (var copyFromMemoryStream = new MemoryStream(pdfBytes))
                            {
                                using (var reader = new PdfReader(copyFromMemoryStream))
                                {
                                    using (var copyFromDocument = new PdfDocument(reader))
                                    {
                                        merger.Merge(copyFromDocument, 1, copyFromDocument.GetNumberOfPages());
                                    }
                                }
                            }
                        }
                    }
                }

                return writerMemoryStream.ToArray();
            }
        }

    }
}
