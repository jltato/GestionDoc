using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using SUAP_PortalOficios.Areas.Identity.Pages.Account;
using SUAP_PortalOficios.Data;
using SUAP_PortalOficios.Data.Repository.Implementations;
using SUAP_PortalOficios.Data.Repository.Interfaces;
using SUAP_PortalOficios.Extensions;
using SUAP_PortalOficios.Hubs;
using SUAP_PortalOficios.Migrations;
using SUAP_PortalOficios.Models;
using SUAP_PortalOficios.Models.DTOs;
using Superpower;
using System.Security.Claims;
using System.Threading.Tasks.Dataflow;
using static SUAP_PortalOficios.Views.Shared.Components.FormOficio;


namespace SUAP_PortalOficios.Controllers
{
    public class OficiosController : Controller
    {
        private readonly MyDbContext _context;
        private readonly MyDbContextSuap _contextSuap;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly UserManager<MyUser> _userManager;
        private readonly ILogger<OficiosController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IInternoRepository _InternoRepository;
        private readonly IUserPermissionsService _userPermissionsService;
        public OficiosController(MyDbContext context,
            MyDbContextSuap contextSuap,
            IHubContext<UpdateHub> hubContext,
            UserManager<MyUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<OficiosController> logger,
            IInternoRepository InternoRepository,
            IUserPermissionsService userPermissionsService)
        {
            _context = context;
            _contextSuap = contextSuap;
            _hubContext = hubContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _InternoRepository = InternoRepository;
            _userPermissionsService = userPermissionsService;
        }

        // GET: Oficios/UploadPdf
        [Authorize(Roles = "Coordinador")]
        public ActionResult UploadPdf()
        {
            return View();
        }
        // GET: Oficios
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IndexAsync(string id)
        {
            if (id == null || id == "")
            {
                ViewBag.Estados = "Pendientes";
                ViewBag.State = "Pendientes";
                return View();
            }
            string estado = id.Replace("-", " ");
            var sections = await _context.Sections
                            .Select(a => new { a.SectionId, a.Name })
                            .ToListAsync();

            ViewBag.Sections = sections;
            ViewBag.Estados = estado;
            ViewBag.State = id;
            return View();
        }

        // GET: Oficios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            try
            {
                // InternoRepository internoRepository = new InternoRepository(_contextSuap, _context, _userPermissionsService);
                //var user = await _userManager.GetUserAsync(User);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var up = await _userPermissionsService.GetPermissionsAsync(User);

                // busco el oficio y le incluyo todos los campos
                var oficios = await _context.Oficios
                    .Include(o => o.MyUser)
                    .Include(o => o.Estado)
                    .Include(o => o.TipoOficio)
                    .Include(o => o.MedioIng)
                    .Include(o => o.Plazo)
                    .Include(o => o.Scope)
                    .Include(o => o.interno_X_Oficios)
                    .FirstOrDefaultAsync(m => m.IdOficio == id);

                if (oficios == null)
                {
                    return NotFound();
                }
                else
                {
                    //Busco las areas asociadas al oficio si es que existen
                    var oficiosAreas = await _context.Oficios_x_Area
                                     .Include(o => o.Estado)
                                     .Include(o => o.Sections)
                                     .Where(oa => oa.OficiosId == id)
                                     .ToListAsync();

                    //Actualizo el ofiocio_x_area de "nuevo" a  "pendiente"
                    var paraModificar = oficiosAreas.Where(oa => up.Sections.Contains(oa.SectionId) && oa.EstadoId == 3).ToList();

                    if (paraModificar.Any())
                    {
                        foreach (var paraMod in paraModificar)
                        {
                            if (paraMod != null && paraMod.EstadoId == 3)
                            {
                                paraMod.EstadoId = 4;
                                _context.Oficios_x_Area.Update(paraMod);
                            }
                        }
                        await _context.SaveChangesAsync();
                        var listUsers = await UsersByOficioAsync(id, "Usuarios");
                        listUsers.Add(userId);
                        await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                        await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    }
                    // busco el interno asociado al oficio si es que existe
                    InternoResultadoDTO? interno = null;
                    var ixo = oficios.interno_X_Oficios?.FirstOrDefault();

                    if (ixo != null)
                    {
                        var legajo = ixo.Legajo;
                        interno = await _InternoRepository.BuscarInternoAsync(legajo);
                        //interno = await internoRepository.BuscarInternoAsync(ixo.Legajo, User, true);
                    }
                    //busco el tribunal asociado al oficio si es que existe
                    Tribunal? tribunal = new Tribunal();
                    var tribu = await _contextSuap.Tribunals.FirstOrDefaultAsync(t => t.IdTribunal == oficios.IdTribunal);
                    if (tribu != null)
                    {
                        tribunal = tribu;
                    }

                    // actualizo los permisos para borrar segun el rol
                    var userRoles = up.Roles.ToList();
                    bool permission = false;
                    if (userRoles.Contains("Administrador") || userRoles.Contains("Coordinador") || userRoles.Contains("Asesor"))
                    {
                        if (oficios.IdEstado < 7)
                        {
                            permission = true;
                        }
                    }


                    // Permiso para agregar archivos segun el rol y el estado del oficio
                    bool hasPermission = false;
                    switch (userRoles)
                    {
                        case var roles when roles.Contains("Administrador"):
                            hasPermission = true;
                            break;

                        case var roles when roles.Contains("Coordinador"):
                            hasPermission = oficios.IdEstado < 7;
                            break;

                        case var roles when roles.Contains("Asesor"):
                            hasPermission = oficios.IdEstado < 6;
                            break;

                        case var roles when roles.Contains("Usuario"):
                            hasPermission = oficios.IdEstado < 5;
                            break;

                        default:
                            hasPermission = false;
                            break;
                    }

                    ViewBag.AddPermission = hasPermission;
                    ViewBag.DeletePermission = permission;
                    ViewBag.Areas = oficiosAreas;
                    ViewBag.Tribunal = tribunal;
                    ViewBag.Interno = interno;
                    return View(oficios);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener los Detalles del oficio " + ex.Message);
                return BadRequest();
            }
        }

        // GET: Oficios/Observaciones/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Observaciones(int? id)
        {
            var observations = await _context.Observation
                              .Include(o => o.user)
                              .Where(o => o.IdOficio == id)
                              .ToListAsync();

            if (observations == null || observations.Count < 1)
            {
                return NotFound();
            }
            return Ok(observations);
        }

        // POST: Oficios/Observaciones/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostObservaciones([FromBody] Observation observation)
        {
            try
            {
                if (observation != null)
                {
                    _context.Observation.Add(observation);
                    await _context.SaveChangesAsync();

                    var OficioId = observation.IdOficio;

                    var listUsers = await UsersByOficioAsync(OficioId, "");
                    var aCargo = await UsersByOficioAsync(OficioId, "COORDINADOR, ASESOR");
                    listUsers.AddRange(aCargo);
                    listUsers.Add(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
                    // Notificar a los clientes para actualizar observaciones
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveMessage", "Mensaje para varios usuarios específicos");
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al enviar Observacion " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        //POST: oficios/DatosTable
        [Authorize]
        [HttpPost]
        public async Task<IActionResult?> DatosTable()
        {
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(User);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = up.Roles.ToList(); // Roles del usuario

                // Obtener parámetros de la solicitud
                var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
                var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "0");
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault() ?? "";
                var orderDirection = Request.Form["order[0][dir]"] == "asc" ? "OrderBy" : "OrderByDescending";
                var draw = int.Parse(Request.Form["draw"].FirstOrDefault() ?? "0");

                // Obtener parametros personalizados
                var section = Request.Form["section"].FirstOrDefault() ?? "";
                var state = Request.Form["state"].FirstOrDefault() ?? ""; // Estado del oficio (Nuevo, Pendiente, Archivado, etc.)
                var estado = ""; // variable para manejar los estados dentro de la consulta sql

                var estab = string.Join(",", up.Scopes.ToList());
                var sections = string.Join(",", up.Sections.ToList());

                switch (state)
                {
                    case "Nuevos":
                        estado = "3";
                        break;
                    case "Pendientes":
                        if (userRoles.Contains("Coordinador") || userRoles.Contains("Asesor"))
                        {
                            estado = "3,4";
                        }
                        else
                        {
                            estado = "4";
                        }
                        break;
                    case "Para-Control":
                        estado = "5";
                        break;
                    case "Para-Enviar":
                        estado = "6";
                        break;
                    case "Archivados":
                        if (userRoles.Contains("Coordinador"))
                        {
                            estado = "7";
                        }
                        else if (userRoles.Contains("Asesor"))
                        {
                            estado = "6,7";
                        }
                        else
                        {
                            estado = "5,6,7";
                        }
                        orderColumnIndex = "1";
                        break;
                    default:
                        estado = "4";
                        break;
                }

                var sql = "";

                if (userRoles.Contains("Coordinador") || userRoles.Contains("Asesor") || userRoles.Contains("Administrador"))
                {
                    sql = @" 
                            SELECT 
                                o.IdOficio, 
                                MAX(P.PlazoName) AS Plazo, 
                                MAX(M.MedioName) AS Medio, 
                                MAX(o.FechaIngreso) AS FechaIngreso, 
                                MAX(t.descripcion) AS Tribunal, 
                                MAX(ti.TipoOficioNombre) AS TipoOficioNombre, 
                                MAX(I.legajo) AS Legajo, 
                                MAX(I.apellido) AS Apellido, 
                                MAX(I.nombre) AS Nombre,
                                MAX(s.ScopeName) AS EstabACargo
                            FROM 
                                Oficios O
                            LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                            LEFT JOIN Scopes s ON s.ScopeId = O.IdEstabACargo
                            LEFT JOIN TipoOficios TI ON TI.IdTipoOficio = O.IdTipoOficio
                            LEFT JOIN [SUGI].[DBO].tribunal T ON T.idTribunal = O.IdTribunal
                            LEFT JOIN Plazo P ON P.IdPlazo = O.IdPlazo
                            LEFT JOIN MedioIng M ON M.IdMedio = O.IdMedio
                            LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                            LEFT JOIN [SUGI].[DBO].interno I ON I.legajo = IXO.Legajo
                            LEFT JOIN Oficios_x_Area oa ON oa.OficiosId = o.IdOficio
                            WHERE                                 
                                O.EliminadoLogico = 0   
                                AND O.IdEstabACargo in(" + estab + @")
                                AND O.IdEstado in (" + estado + @") ";
                    if (section != "1")
                    {
                        sql += " AND oa.SectionId =" + section + " ";
                    }

                    sql += @"    
                                 GROUP BY 
                                    o.IdOficio
                                ORDER BY
                                    CASE WHEN MAX(o.IdPlazo) IS NULL THEN 1 ELSE 0 END, 
                                    MAX(o.IdPlazo) ASC,
                                    MAX(o.FechaIngreso) ASC; ";
                }
                else
                {
                    sql += @" 
                            SELECT DISTINCT
                                o.IdOficio, 
                                P.PlazoName AS Plazo, 
                                M.MedioName AS Medio, 
                                o.FechaIngreso, 
                                t.descripcion AS Tribunal, 
                                ti.TipoOficioNombre, 
                                I.legajo, 
                                I.apellido, 
                                I.nombre, 
                                o.IdPlazo,
                                s.ScopeName as EstabACargo,
		                        CASE WHEN o.IdPlazo IS NULL THEN 1 ELSE 0 END AS PlazoOrden
                            FROM 
                                Oficios O
                            LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                            LEFT JOIN Scopes s on s.ScopeId = O.IdEstabACargo
                            LEFT JOIN TipoOficios TI ON TI.IdTipoOficio = O.IdTipoOficio
                            LEFT JOIN [SUGI].[DBO].tribunal T ON T.idTribunal = O.IdTribunal
                            INNER JOIN Oficios_x_Area OA ON OA.OficiosId = O.IdOficio
                            LEFT JOIN Plazo P ON P.IdPlazo = O.IdPlazo
                            LEFT JOIN MedioIng M ON M.IdMedio = O.IdMedio
                            LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                            LEFT JOIN [SUGI].[DBO].interno I ON I.legajo = IXO.Legajo
                            WHERE 
                                O.EliminadoLogico = 0  
                                AND OA.EstadoId in (" + estado + @") 
                                AND OA.SectionId in (" + sections + @")
		                        AND oa.ScopeId in (" + estab + @")
                            ORDER BY
                                CASE WHEN o.IdPlazo IS NULL THEN 1 ELSE 0 END, 
                                o.IdPlazo ASC,
                                o.FechaIngreso ASC ";
                }
                var userIdParam = new SqlParameter("@UserId", userId);

                var allData = await _context.Database.SqlQueryRaw<pendientesDTO>(sql, userIdParam).ToListAsync();

                // Contar registros totales (sin paginación)
                var totalRecords = allData.Count();

                // Aplicar filtrado en memoria
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower(); // Convertir a minúsculas para una búsqueda sin distinción de mayúsculas/minúsculas

                    allData = allData.Where(i =>
                        i.IdOficio.ToString().Contains(searchValue) ||
                        (i.Plazo != null && i.Plazo.ToLower().Contains(searchValue)) ||
                        (i.FechaIngreso.ToString("dd/MM/yyyy HH:mm").Contains(searchValue)) ||
                        (i.Legajo.ToString() != null && i.Legajo.ToString().Contains(searchValue)) ||
                        (i.Nombre != null && i.Nombre.ToLower().Contains(searchValue)) ||
                        (i.Apellido != null && i.Apellido.ToLower().Contains(searchValue)) ||
                        (i.TipoOficioNombre != null && i.TipoOficioNombre.ToLower().Contains(searchValue)) ||
                        (i.Tribunal != null && i.Tribunal.ToLower().Contains(searchValue))
                    ).ToList();
                }

                // Contar registros filtrados
                var totalRecordsFiltered = allData.Count();

                //// Aplicar ordenación en memoria
                switch (orderColumnIndex)
                {
                    case "0":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.IdOficio).ToList()
                            : allData.OrderByDescending(i => i.IdOficio).ToList();
                        break;
                    case "1":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.FechaIngreso).ToList()
                            : allData.OrderByDescending(i => i.FechaIngreso).ToList();
                        break;

                    case "2":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Legajo).ToList()
                            : allData.OrderByDescending(i => i.Legajo).ToList();
                        break;
                    case "3":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Apellido).ToList()
                            : allData.OrderByDescending(i => i.Apellido).ToList();
                        break;
                    case "4":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Nombre).ToList()
                            : allData.OrderByDescending(i => i.Nombre).ToList();
                        break;
                    case "5":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Plazo).ToList()
                            : allData.OrderByDescending(i => i.Plazo).ToList();
                        break;
                    case "6":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Tribunal).ToList()
                            : allData.OrderByDescending(i => i.Tribunal).ToList();
                        break;
                    case "7":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.EstabACargo).ToList()
                            : allData.OrderByDescending(i => i.EstabACargo).ToList();
                        break;
                    default:

                        break;
                }

                if (length == -1)
                {
                    length = totalRecordsFiltered;
                }

                // Paginación
                var paginatedResult = allData
                    .Skip(start)
                    .Take(length)
                    .ToList();

                var json = Json(new {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecordsFiltered,
                    data = paginatedResult
                });

                return (json);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;
            }
        }

        // GET: Oficios/Create
        [Authorize(Roles = "Coordinador")]
        [HttpGet]
        public async Task<IActionResult> Create(int Id)
        {
            var up = await _userPermissionsService.GetPermissionsAsync(User);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = up.Roles.ToList();
            bool permission = false;
            if (userRoles.Count > 0)
            {
                foreach (var role in userRoles)
                {
                    if (role == "Coordinador" || role == "Administrador")
                    {
                        permission = true;
                    }
                }
            }
            else
            {
                Console.WriteLine("Usuario no encontrado.");
            }

            ViewBag.UserId = userId;
            var scopeId = await _context.UserPermissions
                            .Include(up => up.Scope)
                            .Where(up => up.UserId == userId)
                            .Select(up => new {
                                up.ScopeId,
                                up.Scope.ScopeName
                            })
                            .ToListAsync();
            var tribunales = await _contextSuap.Tribunals
                            .Where(up => up.EstadoTribu == 1)
                            .ToListAsync();

            ViewBag.Oficio = await _context.Oficios
                             .Include(o => o.MyUser)
                             .Where(o => o.IdOficio == Id)
                             .FirstOrDefaultAsync();
            ViewBag.IdTipoOficio = new SelectList(_context.TipoOficios, "IdTipoOficio", "TipoOficioNombre");
            ViewBag.UserId = userId;
            ViewBag.Derivado = new SelectList(scopeId, "ScopeId", "ScopeName");
            ViewBag.Tribunales = new SelectList(tribunales, "IdTribunal", "Descripcion");
            ViewBag.DeletePermission = permission;
            return View();
        }

        //POST: /Oficios/GuardarOficio
        [Authorize(Roles = "Coordinador")]
        [HttpPost]
        public async Task<IActionResult> GuardarOficio([FromBody] FormCargaDTO formCarga)
        {
            if (ModelState.IsValid)
            {
                if (formCarga.RegistroList == null || formCarga.RegistroList.Count == 0)
                {
                    return BadRequest("La lista de registros no puede estar vacía.");
                }
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userManager.GetUserAsync(User);
                    List<RegistroDTO> listado = formCarga.RegistroList;
                    Oficios oficio = formCarga.Oficios;
                    oficio.IdEstabACargo = listado[0].DerivacionId;
                    oficio.MyUser = user;
                    oficio.UserId = userId;
                    oficio.Modificado = DateTime.Now;
                    _context.Update(oficio);

                    var documento = await _context.DocumentPdf.Where(d => d.OficioId == oficio.IdOficio).FirstOrDefaultAsync();

                    // Notificar a todos los clientes conectados que la tabla ha sido actualizada
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "ASESOR");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Nuevo Oficio", "Tienes un nuevo oficio para derivar");
                    var coordinadores = await UsersByOficioAsync(oficio.IdOficio, "COORDINADOR");
                    listUsers.AddRange(coordinadores);
                    listUsers.Add(userId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");

                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $" A derivado correctamente el oficio ID:{oficio.IdOficio} Al establecimiento {listado[0].Derivado}");

                    // Verificar si el primer registro tiene legajo y agregar Interno_x_Oficio
                    if (listado[0].Legajo > 0)
                    {
                        Interno_x_Oficio interno = new()
                        {
                            OficiosId = oficio.IdOficio,
                            Legajo = listado[0].Legajo ?? 0,
                        };
                        _context.Add(interno);
                    }

                    // Procesar registros adicionales
                    for (var item = 1; item < listado.Count(); item++)
                    {
                        var EstabACargo = listado[item].Derivado;
                        Oficios NewOficio = new Oficios
                        {
                            IdTipoOficio = oficio.IdTipoOficio,
                            IdTribunal = oficio.IdTribunal,
                            IdEstado = oficio.IdEstado,
                            UserId = userId,
                            Modificado = DateTime.Now,
                            IdPlazo = oficio.IdPlazo,
                            IdMedio = oficio.IdMedio,
                            FechaIngreso = oficio.FechaIngreso,
                            IdEstabACargo = listado[item].DerivacionId,
                            SAC = oficio.SAC,
                        };

                        _context.Add(NewOficio);
                        await _context.SaveChangesAsync(); // Necesario aquí para obtener el ID generado por la base de datos
                        DocumentPdf NewDocument = new DocumentPdf
                        {
                            src = documento.src,
                            FileName = documento.FileName,
                            OficioId = NewOficio.IdOficio,
                            fechaCarga = documento.fechaCarga,
                            FileSize = documento.FileSize
                        };
                        NewDocument.OficioId = NewOficio.IdOficio;
                        _context.DocumentPdf.Add(NewDocument);


                        if (listado[item].Legajo > 0)
                        {
                            Interno_x_Oficio newinterno = new()
                            {
                                OficiosId = NewOficio.IdOficio,
                                Legajo = listado[item].Legajo ?? 0,
                            };
                            _context.Add(newinterno);
                        }
                        // Notificar a todos los clientes conectados que la tabla ha sido actualizada
                        var listUsers2 = await UsersByOficioAsync(NewOficio.IdOficio, "ASESOR");
                        await _hubContext.Clients.Users(listUsers2).SendAsync("ReceiveNotify", "Nuevo Oficio", "Tienes un nuevo oficio para derivar");
                        var coordinadores2 = await UsersByOficioAsync(NewOficio.IdOficio, "COORDINADOR");
                        listUsers2.AddRange(coordinadores2);
                        listUsers2.Add(userId);
                        await _hubContext.Clients.Users(listUsers2).SendAsync("ReceiveTableUpdate");
                        await _hubContext.Clients.Users(listUsers2).SendAsync("ReceiveCantidadesUpdate");
                        _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $" A derivado correctamente el oficio ID:{NewOficio.IdOficio} Al establecimiento {EstabACargo}");
                    }

                    await _context.SaveChangesAsync(); // Guardar todos los cambios juntos
                    await transaction.CommitAsync(); // Confirmar la transacción

                    return RedirectToAction(nameof(Borradores));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Revertir la transacción en caso de error
                    _logger.LogError("Error al guardar Oficio nuevo " + ex.Message);
                    return BadRequest(ex);
                }
            }
            return BadRequest(ModelState);
        }

        //GET: oficios/deleted
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var up = await _userPermissionsService.GetPermissionsAsync(User);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = up.Roles.ToList();

            var sql = @"
                        SELECT 
                         o.IdOficio, 
                         o.FechaIngreso AS 'FechaIng', 
                         p.PlazoName as 'Plazo', 
                         I.legajo as 'Legajo', 
                         I.apellido as 'Apellido', 
                         i.nombre as 'Nombre', 
                         T.TipoOficioNombre as 'Tipo', 
                         tr.descripcion as 'Tribunal', 
                         S.ScopeName AS 'EstabACargo',
                         E.EstadoNombre AS 'Estado', 
	                     o.Modificado AS 'Eliminado'
                     FROM Oficios O
                         LEFT JOIN Plazo p on p.IdPlazo = o.IdPlazo
                         LEFT JOIN TipoOficios T ON T.IdTipoOficio = O.IdTipoOficio
                         LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                         LEFT JOIN [SUGI].[dbo].interno I ON I.legajo = IXO.Legajo
                         LEFT JOIN [SUGI].[dbo].tribunal TR ON TR.idTribunal = O.IdTribunal
                         LEFT JOIN Scopes S ON S.ScopeId = O.IdEstabACargo
                         LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                     WHERE o.EliminadoLogico = 1 
                     and O.IdEstado <> 1
                     and o.Modificado >= DATEADD(DAY, -30, CAST(GETDATE() AS DATE))
                     and IdEstabACargo in (
                         SELECT s.ScopeId from Scopes s
                         INNER JOIN UserPermissions up on up.ScopeId = s.ScopeId
                         WHERE up.UserId = @UserId)		
                     ORDER BY 
                         o.Modificado DESC 
                        ";
            var userIdParam = new SqlParameter("@UserId", userId);

            var allData = await _context.Database.SqlQueryRaw<OficiosBorradosDTO>(sql, userIdParam).ToListAsync();

            return View(allData);
        }

        // GET: Oficios/Delete/5
        [Authorize(Roles = "Coordinador")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Capturar la URL de la página anterior (Referer)
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;

            var oficios = await _context.Oficios
                .Include(o => o.Estado)
                .Include(o => o.MyUser)
                .Include(o => o.TipoOficio)
                .Include(o => o.MedioIng)
                .Include(o => o.Scope)
                .Include(o => o.interno_X_Oficios)
                .FirstOrDefaultAsync(m => m.IdOficio == id);

            if (oficios == null)
            {
                return NotFound();
            }
            if (oficios.IdEstado == 7)
            {
                return Redirect("/Identity/Account/AccessDenied");
            }

            var legajo = oficios?.interno_X_Oficios?
                .Select(o => o.Legajo)
                .FirstOrDefault();
            if (legajo > 0)
            {
                var interno = await _contextSuap.Internos
                .Select(i => new InternoResultadoDTO
                {
                    Legajo = i.Legajo,
                    Apellido = i.Apellido,
                    Nombre = i.Nombre,
                })
                .FirstOrDefaultAsync(i => i.Legajo == legajo);

                ViewBag.interno = interno;
            }
            else
            {
                ViewBag.interno = null;
            }


            return View(oficios);
        }

        // POST: Oficios/Delete/5
        [Authorize(Roles = "Coordinador")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            { var user = await _userManager.GetUserAsync(User);
                var oficios = await _context.Oficios.FindAsync(id);
                if (oficios != null)
                {
                    oficios.EliminadoLogico = true;

                    oficios.MyUser = user;
                    oficios.Modificado = DateTime.Now;
                    _context.Oficios.Update(oficios);
                    await _context.SaveChangesAsync();
                    var listUsers = await UsersByOficioAsync(id, "Todos");

                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    listUsers.Add(user?.Id ?? "");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(user.Nombre, $"Ha borrado exitosamente el oficio ID: {oficios.IdOficio}");
                }

                ViewBag.Response = "El oficio se ha sido eliminado";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al intentar borrar un oficio" + ex.Message);
                return BadRequest();
            }

        }

        //POST: oficios/resotre/5
        [HttpPost]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var oficios = await _context.Oficios.FindAsync(id);
                if (oficios != null)
                {
                    oficios.EliminadoLogico = false;
                    oficios.UserId = userId;
                    oficios.Modificado = DateTime.Now;
                    _context.Oficios.Update(oficios);
                    await _context.SaveChangesAsync();
                    var listUsers = await UsersByOficioAsync(id, "Todos");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Atención", "Se ha restaurado un oficio borrdo previamente");
                    listUsers.Add(userId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Ha Restaurado el oficio ID: {oficios.IdOficio}");
                }

                return RedirectToAction(nameof(IndexAsync), new { id = "Pendientes" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al restaurar oficio" + ex.Message);
                return BadRequest();
            }
        }

        private async Task<bool> OficiosExists(int id)
        {
            return await _context.Oficios.AnyAsync(e => e.IdOficio == id);
        }

        // GET: Oficios/DetalleOficio/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DetalleOficio(int id)
        {
            var oficio = await _context.Oficios
                        .Include(o => o.oficios_X_Areas)
                            .ThenInclude(oa => oa.Sections)
                        .Include(o => o.oficios_X_Areas)
                            .ThenInclude(oa => oa.Estado)
                        .Include(o => o.Scope)
                        .Include(o => o.TipoOficio)
                        .Include(o => o.Plazo)
                        .Include(o => o.MedioIng)
                        .Where(o => o.IdOficio == id)
                        .FirstOrDefaultAsync();

            if (oficio == null)
            {
                return NotFound();
            }

            var tribunal = await _contextSuap.Tribunals
                            .Where(t => t.IdTribunal == oficio.IdTribunal)
                            .FirstOrDefaultAsync();

            if (tribunal != null)
            {
                oficio.Tribunal = tribunal;
            }
            return Ok(oficio);
        }

        // Get: Oficios/borradores
        [Authorize(Roles = "Coordinador")]
        [HttpGet]
        public async Task<IActionResult> Borradores()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sql = @"
                        SELECT o.IdOficio, o.FechaIngreso, dp.FileName, dp.FileSize
                        FROM Oficios O
                        INNER JOIN DocumentPdf DP ON DP.OficioId = O.IdOficio
                        where o.EliminadoLogico = 0 and IdEstabACargo in (
                        select s.ScopeId from Scopes s
                        inner join UserPermissions up on up.ScopeId = s.ScopeId
                        where up.UserId = @UserId)
                        and o.IdEstado = 1
                        and dp.EliminadoLogico = 0
                        ORDER BY o.FechaIngreso DESC
                        ";
            var userIdParam = new SqlParameter("@UserId", userId);

            var allData = await _context.Database.SqlQueryRaw<Borrador>(sql, userIdParam).ToListAsync();

            return View(allData);
        }

        [HttpGet]
        [Authorize(Roles = "Coordinador, Asesor")]
        public async Task<IActionResult> SinDerivar()
        {
            return View();
        }

        //POST: oficios/DatosSinDerivar
        [Authorize]
        [HttpPost]
        public async Task<IActionResult?> DatosSinDerivar()
        {
            try
            {
                // Obtener parámetros de la solicitud
                var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";
                var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "0");
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault() ?? "";
                var orderDirection = Request.Form["order[0][dir]"] == "asc" ? "OrderBy" : "OrderByDescending";
                var draw = int.Parse(Request.Form["draw"].FirstOrDefault() ?? "0");


                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sql = @"
                        SELECT 
                            o.IdOficio, 
                            o.FechaIngreso AS 'FechaIng', 
                            p.PlazoName as 'Plazo', 
                            I.legajo as 'Legajo', 
                            I.apellido as 'Apellido', 
                            i.nombre as 'Nombre', 
                            T.TipoOficioNombre as 'Tipo', 
                            tr.descripcion as 'Tribunal', 
                            S.ScopeName AS 'EstabACargo',
                            S.ScopeId,
                            E.EstadoNombre AS 'Estado',
                            O.SAC
                        FROM Oficios O
                            LEFT JOIN Plazo p on p.IdPlazo = o.IdPlazo
                            LEFT JOIN TipoOficios T ON T.IdTipoOficio = O.IdTipoOficio
                            LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                            LEFT JOIN [SUGI].[dbo].interno I ON I.legajo = IXO.Legajo
                            LEFT JOIN [SUGI].[dbo].tribunal TR ON TR.idTribunal = O.IdTribunal
                            LEFT JOIN Scopes S ON S.ScopeId = O.IdEstabACargo
                            LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                        WHERE o.EliminadoLogico = 0 and IdEstabACargo in (
                            SELECT s.ScopeId from Scopes s
                            INNER JOIN UserPermissions up on up.ScopeId = s.ScopeId
                            WHERE up.UserId = @UserId)
                            AND o.IdEstado = 2
                        ORDER BY 
                            CASE WHEN o.IdPlazo IS NULL THEN 1 ELSE 0 END, 
                            o.IdPlazo ASC, 
                            o.FechaIngreso ASC 
                        ";

                var userIdParam = new SqlParameter("@UserId", userId);

                var allData = await _context.Database.SqlQueryRaw<OficiosPendientesDTO>(sql, userIdParam).ToListAsync();


                // Contar registros totales (sin paginación)
                var totalRecords = allData.Count();


                // Aplicar filtrado en memoria
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower(); // Convertir a minúsculas para una búsqueda sin distinción de mayúsculas/minúsculas

                    allData = allData.Where(i =>
                        i.IdOficio.ToString().Contains(searchValue) ||
                        (i.Plazo != null && i.Plazo.ToLower().Contains(searchValue)) ||
                        (i.FechaIng.ToString("dd/MM/yyyy HH:mm").Contains(searchValue)) ||
                        (i.Legajo.ToString() != null && i.Legajo.ToString().Contains(searchValue)) ||
                        (i.Nombre != null && i.Nombre.ToLower().Contains(searchValue)) ||
                        (i.Apellido != null && i.Apellido.ToLower().Contains(searchValue)) ||
                        (i.Tipo != null && i.Tipo.ToLower().Contains(searchValue)) ||
                        (i.Tribunal != null && i.Tribunal.ToLower().Contains(searchValue)) ||
                        (i.SAC != null && i.SAC.ToLower().Contains(searchValue)) ||
                        (i.EstabACargo != null && i.EstabACargo.ToLower().Contains(searchValue))
                    ).ToList();
                }

                // Contar registros filtrados
                var totalRecordsFiltered = allData.Count();

                //// Aplicar ordenación en memoria
                switch (orderColumnIndex)
                {
                    case "0":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.IdOficio).ToList()
                            : allData.OrderByDescending(i => i.IdOficio).ToList();
                        break;
                    case "1":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.FechaIng).ToList()
                            : allData.OrderByDescending(i => i.FechaIng).ToList();
                        break;
                    case "2":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Plazo).ToList()
                            : allData.OrderByDescending(i => i.Plazo).ToList();
                        break;
                    case "3":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Legajo).ToList()
                            : allData.OrderByDescending(i => i.Legajo).ToList();
                        break;
                    case "4":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Apellido).ToList()
                            : allData.OrderByDescending(i => i.Apellido).ToList();
                        break;
                    case "5":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Nombre).ToList()
                            : allData.OrderByDescending(i => i.Nombre).ToList();
                        break;
                    case "6":
                        allData = orderDirection == "OrderBy"
                           ? allData.OrderBy(i => i.Tipo).ToList()
                           : allData.OrderByDescending(i => i.Tipo).ToList();
                        break;
                    case "7":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.Tribunal).ToList()
                            : allData.OrderByDescending(i => i.Tribunal).ToList();
                        break;
                    case "8":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.SAC).ToList()
                            : allData.OrderByDescending(i => i.SAC).ToList();
                        break;
                    case "9":
                        allData = orderDirection == "OrderBy"
                            ? allData.OrderBy(i => i.EstabACargo).ToList()
                            : allData.OrderByDescending(i => i.EstabACargo).ToList();
                        break;
                    default:
                        //allData = allData.OrderBy(e => e.Plazo)
                        //.ToList();
                        break;
                }

                if (length == -1)
                {
                    length = totalRecordsFiltered;
                }

                // Paginación
                var paginatedResult = allData
                    .Skip(start)
                    .Take(length)
                    .ToList();

                var json = Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecordsFiltered,
                    data = paginatedResult
                });

                return (json);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Coordinador, Asesor")]
        public async Task<IActionResult> Derivar(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            // Capturar la URL de la página anterior (Referer)
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            var oficio = await _context.Oficios
                                   .Include(o => o.MyUser)
                                   .Where(o => o.IdOficio == Id)
                                   .FirstOrDefaultAsync();
            if (oficio.IdEstado < 7)
            {
                var up = await _userPermissionsService.GetPermissionsAsync(User);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = up.Roles.ToList();
                bool permission = true;

                var legajo = await _context.Interno_x_Oficio.Where(o => o.OficiosId == Id).Select(i => i.Legajo).FirstOrDefaultAsync();
                InternoResultadoDTO? interno = await _InternoRepository.BuscarInternoAsync(legajo);

                List<Sections> sections = await _context.Oficios_x_Area
                                         .Where(o => o.OficiosId == Id)
                                         .Select(o => o.Sections)
                                         .ToListAsync();

                ViewBag.Interno = interno;
                ViewBag.Oficio = oficio;
                ViewBag.UserId = userId;
                ViewBag.DeletePermission = permission;
                ViewBag.Sections = sections;
                ViewBag.RefererUrl = refererUrl;
                return View();
            }
            else
            {
                return Redirect("/Identity/Account/AccessDenied");
            }
        }

        //Post: /oficios/GuardarDeriva
        [Authorize(Roles = "Asesor, Coordinador")]
        [HttpPost]
        public async Task<IActionResult> GuardarDeriva([FromBody] FormCargaSecundariaDTO formCarga)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    List<Sections> listado = formCarga.RegistroList;
                    Oficios oficio = formCarga.Oficios;
                    oficio.MyUser = user;
                    oficio.Modificado = DateTime.Now;
                    _context.Oficios.Update(oficio);


                    // pocesar internos x oficio
                    if (oficio.interno_X_Oficios != null && oficio.interno_X_Oficios.Any())
                    {
                        // Obtener el único interno_x_oficio de la lista
                        var interno = oficio.interno_X_Oficios.First();

                        // Buscar si ya existe algún registro en la base de datos asociado al IdOficio
                        var existingIxo = await _context.Interno_x_Oficio
                            .Where(ixo => ixo.OficiosId == oficio.IdOficio)
                            .FirstOrDefaultAsync();

                        if (existingIxo == null)
                        {
                            // No hay registros existentes, agregar el nuevo
                            _context.Interno_x_Oficio.Add(interno);
                        }
                        else
                        {
                            if (existingIxo.Legajo != interno.Legajo)
                            {
                                // Si el Legajo es diferente, eliminar el existente y agregar el nuevo
                                _context.Interno_x_Oficio.Remove(existingIxo);
                                _context.Interno_x_Oficio.Add(interno);
                            }
                        }
                    }
                    // Procesar registros 
                    List<Oficios_x_Area> oficiosxArea = await _context.Oficios_x_Area
                                                        .Include(oa => oa.Sections)
                                                        .Where(oa => oa.OficiosId == oficio.IdOficio)
                                                        .ToListAsync();
                    var registrosAEliminar = oficiosxArea
                                        .Where(oa => !listado.Any(item =>
                                            item.SectionId == oa.SectionId &&
                                            oficio.IdEstabACargo == oa.ScopeId))
                                        .ToList();

                    // Eliminar los registros que ya no están en el listado
                    if (registrosAEliminar.Any())
                    {
                        _context.Oficios_x_Area.RemoveRange(registrosAEliminar);
                    }

                    // Filtrar los nuevos registros para agregar
                    var nuevosRegistros = listado
                        .Where(item => !oficiosxArea.Any(oa =>
                            oa.OficiosId == oficio.IdOficio &&
                            oa.SectionId == item.SectionId &&
                            oa.ScopeId == oficio.IdEstabACargo))
                        .Select(item => new Oficios_x_Area
                        {
                            OficiosId = oficio.IdOficio,
                            SectionId = item.SectionId,
                            ScopeId = oficio.IdEstabACargo,
                            EstadoId = 3,
                            FechaDerivado = DateTime.Now,
                            MyUser = user
                        })
                        .ToList();

                    // Agregar los nuevos registros
                    if (nuevosRegistros.Any())
                    {
                        _context.Oficios_x_Area
                            .AddRange(nuevosRegistros);
                    }

                    var nuevosNames = listado.Select(n => n.Name).ToList();
                    var sectionsSrt = String.Join(", ", nuevosNames);
                    var nombreUsuario = User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo";

                    _logger.Modificacion(nombreUsuario, $"Ha derivado el oficio {oficio.IdOficio} a las Areas {sectionsSrt}");
                    await _context.SaveChangesAsync(); // Guardar todos los cambios juntos
                    await transaction.CommitAsync(); // Confirmar la transacción

                    // Notificar a todos los clientes conectados que la tabla ha sido actualizada
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "Usuarios");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Nuevo Oficio", "Tienes un nuevo oficio pendiente");
                    listUsers = await UsersByOficioAsync(oficio.IdOficio, "Todos");
                    listUsers.Add(user.Id);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");

                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Revertir la transacción en caso de error
                    _logger.LogError("Error al derivar un oficio a las areas" + ex.Message);
                    return BadRequest(ex);
                }

            }
            return View(formCarga);
        }

        //GET: oficios/GetCantidades
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCantidades()
        {
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(User);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = up.Roles.ToList();
                var sql = "";
                if (userRoles.Contains("Administrador") || userRoles.Contains("Coordinador"))
                {
                    sql = @"
                            SELECT 
                                e.EstadoNombre AS Estado,
                                COUNT(*) AS Cantidad
                            FROM oficios o
                                INNER JOIN Estado e ON e.IdEstado = o.IdEstado
                                INNER JOIN UserPermissions up ON up.ScopeId = o.IdEstabACargo 
                                    AND up.UserId = @UserId
                            WHERE o.EliminadoLogico = 0 
                            GROUP BY e.EstadoNombre
                        ";
                }
                else if (userRoles.Contains("Asesor"))
                {
                    sql = @"
                            SELECT 
                                e.EstadoNombre AS Estado,
                                COUNT(*) AS Cantidad
                            FROM oficios o
                                INNER JOIN Estado e ON e.IdEstado = o.IdEstado
                                 INNER JOIN UserPermissions up ON up.ScopeId = o.IdEstabACargo 
                                    AND up.UserId = @UserId
                            WHERE o.EliminadoLogico = 0 
                            GROUP BY e.EstadoNombre
                        ";
                }
                else
                {
                    sql = @"
                            SELECT
                                e.EstadoNombre AS Estado,
                                COUNT(*) AS Cantidad
                            FROM oficios o
                                INNER JOIN Oficios_x_Area oxa ON oxa.OficiosId = o.IdOficio
                                INNER JOIN Estado e ON e.IdEstado = oxa.EstadoId   
                                INNER JOIN UserPermissions up ON up.UserId = @UserId 
                                    AND up.SectionId = oxa.SectionId
                                    AND up.ScopeId = oxa.ScopeId  
                            WHERE o.EliminadoLogico = 0 
                            GROUP BY e.EstadoNombre;
                            ";
                }
                var userIdParam = new SqlParameter("@UserId", userId);

                List<EstadoCantidadDTO> cantidades = await _context.Database.SqlQueryRaw<EstadoCantidadDTO>(sql, userIdParam).ToListAsync();

                return Ok(cantidades);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener cantidades para el LeftBar " + ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetOficiosPendientes(int legajo, int oficioId)
        {
            try
            {
                var sql = @"
                        SELECT CASE 
                            WHEN EXISTS (
                                SELECT 1 
                                FROM Oficios O 
                                LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                                WHERE O.EliminadoLogico = 0 
                                AND O.IdOficio <> " + oficioId + @"
                                AND O.IdEstado < 6
                                AND O.IdEstado > 1
                                AND IXO.Legajo = @legajo
                            ) 
                            THEN 1 
                            ELSE 0 
                        END AS Value";

                var legajoParam = new SqlParameter("@legajo", legajo);

                var result = await _context.Database.SqlQueryRaw<int>(sql, legajoParam).FirstAsync();

                bool existe = result == 1;

                return Ok(existe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PorLegajo(int legajo)
        {
            if (legajo > 0)
            {
                try
                {
                    var query = @"
                            SELECT 
                                o.IdOficio, 
                                o.FechaIngreso AS 'FechaIng', 
                                p.PlazoName as 'Plazo', 
                                I.legajo as 'Legajo', 
                                I.apellido as 'Apellido', 
                                i.nombre as 'Nombre', 
                                T.TipoOficioNombre as 'Tipo', 
                                tr.descripcion as 'Tribunal', 
                                S.ScopeName AS 'EstabACargo',
                                S.ScopeId,
                                E.EstadoNombre AS 'Estado',
                                o.SAC
                            FROM Oficios O
                                LEFT JOIN Plazo p on p.IdPlazo = o.IdPlazo
                                LEFT JOIN TipoOficios T ON T.IdTipoOficio = O.IdTipoOficio
                                LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                                LEFT JOIN [SUGI].[dbo].interno I ON I.legajo = IXO.Legajo
                                LEFT JOIN [SUGI].[dbo].tribunal TR ON TR.idTribunal = O.IdTribunal
                                LEFT JOIN Scopes S ON S.ScopeId = O.IdEstabACargo
                                LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                            WHERE o.EliminadoLogico = 0 
	                            AND IXO.Legajo = @legajo
                            ORDER BY 
                                CASE WHEN o.IdPlazo IS NULL THEN 1 ELSE 0 END, 
                                o.IdPlazo ASC, 
                                o.FechaIngreso ASC ";

                    var userIdParam = new SqlParameter("@legajo", legajo);

                    var allData = await _context.Database.SqlQueryRaw<OficiosPendientesDTO>(query, userIdParam).ToListAsync();

                    return View(allData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al buscar oficios por legajo " + ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("No hay legajo");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PorSac(string sac)
        {
            if (sac != "")
            {
                try
                {
                    var query = @"
                            SELECT 
                                o.IdOficio, 
                                o.FechaIngreso AS 'FechaIng', 
                                p.PlazoName as 'Plazo', 
                                I.legajo as 'Legajo', 
                                I.apellido as 'Apellido', 
                                i.nombre as 'Nombre', 
                                T.TipoOficioNombre as 'Tipo', 
                                tr.descripcion as 'Tribunal', 
                                S.ScopeName AS 'EstabACargo',
                                S.ScopeId,
                                E.EstadoNombre AS 'Estado',
                                o.SAC
                            FROM Oficios O
                                LEFT JOIN Plazo p on p.IdPlazo = o.IdPlazo
                                LEFT JOIN TipoOficios T ON T.IdTipoOficio = O.IdTipoOficio
                                LEFT JOIN Interno_x_Oficio IXO ON IXO.OficiosId = O.IdOficio
                                LEFT JOIN [SUGI].[dbo].interno I ON I.legajo = IXO.Legajo
                                LEFT JOIN [SUGI].[dbo].tribunal TR ON TR.idTribunal = O.IdTribunal
                                LEFT JOIN Scopes S ON S.ScopeId = O.IdEstabACargo
                                LEFT JOIN Estado E ON E.IdEstado = O.IdEstado
                            WHERE o.EliminadoLogico = 0 
	                            AND O.SAC = @sac
                            ORDER BY 
                                CASE WHEN o.IdPlazo IS NULL THEN 1 ELSE 0 END, 
                                o.IdPlazo ASC, 
                                o.FechaIngreso ASC ";

                    var userIdParam = new SqlParameter("@sac", sac);

                    var allData = await _context.Database.SqlQueryRaw<OficiosPendientesDTO>(query, userIdParam).ToListAsync();

                    return View(allData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al buscar oficios por sac " + ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("No hay sac");
        }

        /// <summary>
        /// Devuelve una lista de usuarios asociados al numero de oficio y el rol, solo un rol por consulta
        /// </summary>
        /// <param name="oficioId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task<List<string>> UsersByOficioAsync(int oficioId, string role)
            {
                try
                {
                var sqlQuery = "";
                if (role == "Usuarios")
                {
                    sqlQuery = @"SELECT DISTINCT up.UserId
                                FROM UserPermissions UP
                                INNER JOIN Oficios_x_Area oa 
                                    ON oa.SectionId = up.SectionId AND oa.ScopeId = up.ScopeId
                                WHERE oa.OficiosId = " + oficioId;
                }
                else if(role == "Todos")
                {
                    sqlQuery = @"SELECT DISTINCT up.UserId
                                FROM UserPermissions UP
                                LEFT JOIN Oficios_x_Area oa 
                                    ON oa.SectionId = up.SectionId AND oa.ScopeId = up.ScopeId AND oa.OficiosId = " + oficioId + @"
                                LEFT JOIN Oficios o 
                                    ON o.IdEstabACargo = up.ScopeId AND o.IdOficio = " + oficioId + @"
                                LEFT JOIN AspNetUserRoles anur 
                                    ON anur.UserId = up.UserId
                                LEFT JOIN AspNetRoles anr 
                                    ON anr.Id = anur.RoleId AND anr.NormalizedName IN ('COORDINADOR', 'ASESOR')
                                WHERE oa.OficiosId IS NOT NULL 
                                   OR (o.IdOficio IS NOT NULL AND anr.Id IS NOT NULL)";
                }
                else
                {
                    sqlQuery = @"SELECT DISTINCT up.UserId
                                FROM UserPermissions UP
                                INNER JOIN AspNetUserRoles anur 
                                    ON anur.UserId = up.UserId
                                INNER JOIN AspNetRoles anr 
                                    ON anr.Id = anur.RoleId
                                INNER JOIN Oficios o 
                                    ON o.IdEstabACargo = up.ScopeId
                                WHERE o.IdOficio = " + oficioId +" and anr.NormalizedName = '" + role +"'";  
                }
                 

                var thisUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var listUsers = await _context.UserPermissions
                    .FromSqlRaw(sqlQuery)
                    .Select(u => u.UserId)
                    .ToListAsync();

                var listUsersFiltered = listUsers
                    .Where(userName => userName != null && userName != thisUser )
                    .Cast<string>()
                    .ToList();

                return listUsersFiltered;

                }
                catch(Exception ex)
                {
                   _logger.LogCritical("Error al buscar usuarios por oficios " + ex.Message);
                    return null!;
                }
            }

        private async Task<List<string>> ACargoByOficioAsync(int oficioId, string rol)
        {
            try
            {
                var thisUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var usuarios = await (from user in _context.Users
                                     join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                     join role in _context.Roles on userRole.RoleId equals role.Id
                                     join userPermission in _context.UserPermissions on user.Id equals userPermission.UserId
                                     join oficio in _context.Oficios on userPermission.ScopeId equals oficio.IdEstabACargo
                                     where oficio.IdOficio == oficioId && role.Name == rol
                                     select user.Id).ToListAsync();             


                var listUsersFiltered = usuarios
                    .Where(userName => userName != null && userName != thisUser)
                    .Cast<string>()
                    .ToList();

                return listUsersFiltered;

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Error al buscar usuarios por oficios " + ex.Message);
                return null!;
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult>ParaControl(int oficioId)
        {
            var oficio = await _context.Oficios.FindAsync(oficioId);

            if (oficio == null || oficio.EliminadoLogico == true)
            {
                return NotFound("El oficio especificado no existe o ha sido eliminado.");
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var ups = await _userPermissionsService.GetPermissionsAsync(User);

                // Paso 1: Obtener todos los registros de Oficios_x_Area con el OficioId
                var oficiosArea = await _context.Oficios_x_Area
                    .Include(oa => oa.Sections)
                    .Where(oa => oa.OficiosId == oficioId)
                    .ToListAsync();
                var filteredSections = oficiosArea;
                // Paso 2: Actualizar solo los registros que cumplen la condición de SectionId
                if (ups.Roles.Contains("Asesor") || ups.Roles.Contains("Coordinador"))
                {
                    filteredSections = oficiosArea;
                }
                else
                {               
                    filteredSections = oficiosArea.Where(oa => ups.Sections.Contains(oa.SectionId)).ToList();
                }
                
                foreach (var oa in filteredSections)
                {
                    oa.EstadoId = 5;
                    oa.UserId = userId;
                    oa.TimeStamp = DateTime.Now;
                    oa.FechaFin = DateTime.Now;
                    _context.Oficios_x_Area.Update(oa);

                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "Usuarios");
                    listUsers.Add(userId);  
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                   
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Paso a Control el oficio Id: {oficio.IdOficio} desde el Área: {oa.Sections.Name}");
                }               

                // Paso 3: Verificar si queda algún registro con EstadoId diferente de 5
                bool existenOtrosEstados = oficiosArea.Any(oa => oa.EstadoId != 5);

                // Paso 4: Si no hay más registros con EstadoId != 5, actualizar la otra tabla
                if (!existenOtrosEstados)
                {                   
                    oficio.IdEstado = 5; 
                    _context.Oficios.Update(oficio);
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "ASESOR");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Control", "Tienes un nuevo oficio para controlar");
                    var coordinadores = await UsersByOficioAsync(oficio.IdOficio, "COORDINADOR");
                    listUsers.AddRange(coordinadores);
                    listUsers.Add(userId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    ;
                }
                await _context.SaveChangesAsync();             
                    return RedirectToAction("index", new { id = "Pendientes" });                  
            }
            catch (Exception ex) 
            {
                _logger.LogError("Error al actualizar el estado del oficio: " + ex.Message);
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [Authorize(Roles = "Asesor, Coordinador")]
        [HttpPost]
        public async Task<IActionResult>Finalizar(int oficioId)
        {
            var oficio = await _context.Oficios.FindAsync(oficioId);

            if (oficio == null)
            {
                return NotFound("El oficio especificado no existe.");
            }
            if (oficio.IdTribunal == 0)
            {
                return BadRequest("No se han completado todos los campos");
            }
            try
            {
                if (oficio.IdTipoOficio == 1)
                {
                    oficio.IdEstado = 6;
                }
                else if (oficio.IdTipoOficio == 2)
                {
                    oficio.IdEstado = 7;
                }                
                oficio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                oficio.Modificado = DateTime.Now;
                _context.Oficios.Update(oficio);

                var areasXOficio = await _context.Oficios_x_Area
                                    .Where(oa => oa.OficiosId == oficioId)
                                    .ToListAsync();               
                foreach (var area in areasXOficio)
                {
                    area.EstadoId = 6;
                    _context.Oficios_x_Area.Update(area);
                }
                await _context.SaveChangesAsync();

                var coordinador = await ACargoByOficioAsync(oficioId, "COORDINADOR");
                var listUsers = await UsersByOficioAsync(oficio.IdOficio, "ASESOR");
                await _hubContext.Clients.Users(coordinador).SendAsync("ReceiveNotify", "Para Enviar", "Tienes un nuevo oficio para enviar");
                listUsers.AddRange(coordinador);
                listUsers.Add(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
               
              
                _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Finalizó el oficio Id: {oficio.IdOficio}");

                return RedirectToAction("index", new { id = "Pendientes" });    
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al Finalizar oficio " + ex.Message);
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [Authorize(Roles ="Coordinador")]
        [HttpPost]
        public async Task<IActionResult>Enviado(int oficioId)
        {
            if (oficioId == 0)
            {
                return NotFound();
            }
            try
            {
                var oficio = await _context.Oficios.FindAsync(oficioId);
                if (oficio != null && oficio.EliminadoLogico == false)
                {
                    oficio.IdEstado = 7;
                    oficio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                    oficio.Modificado = DateTime.Now;
                    oficio.FechaFin = DateTime.Now;
                    _context.Oficios.Update(oficio);
                    await _context.SaveChangesAsync();
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "COORDINADOR");
                    listUsers.Add(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Envió el oficio Id: {oficio.IdOficio}");
                    return RedirectToAction("Index", new { id = "Para-Enviar" });
                }
                else
                {
                    return BadRequest("Oficio no encontrado");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Error al enviar oficio" + ex.Message);
                return BadRequest(ex);
            }
            
        }

        [Authorize(Roles = "Asesor, Coordinador")]
        [HttpPost]
        public async Task<IActionResult> Devolucion([FromBody] Oficios_x_Area OficioAreaJson)
        {
            if (OficioAreaJson == null ||
                 OficioAreaJson.OficiosId <= 0 ||
                 OficioAreaJson.SectionId <= 0 ||
                 OficioAreaJson.ScopeId <= 0)
            {
                return BadRequest("Datos inválidos en la solicitud.");
            }


            try
            {
                var OficioArea = await _context.Oficios_x_Area.Where(a => a.OficiosId == OficioAreaJson.OficiosId 
                                        && a.SectionId == OficioAreaJson.SectionId
                                        && a.ScopeId == OficioAreaJson.ScopeId)
                                        .Include(oa => oa.Sections)
                                        .Include(oa => oa.Estado)
                                        .FirstOrDefaultAsync();
               
                if (OficioArea == null)
                {
                    return NotFound("No se encontró el registro en la base de datos.");
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Usuario no autenticado.");
                }
                OficioArea.UserId = userId;
                OficioArea.FechaDerivado = DateTime.Now;               
                OficioArea.EstadoId = 4;
                OficioArea.FechaFin = null;
                _context.Oficios_x_Area.Update(OficioArea);
                

                var oficio = await _context.Oficios.FindAsync(OficioArea.OficiosId);
                if (oficio != null)
                {
                    if(oficio.IdEstado > 4)
                    {
                        oficio.IdEstado = 4;
                        oficio.Modificado = DateTime.Now;
                        oficio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                        _context.Oficios.Update(oficio);
                    }

                    var listUsers = await _context.UserPermissions
                                        .Where(up => up.SectionId == OficioArea.SectionId && up.ScopeId == OficioArea.ScopeId)
                                        .Select(up => up.UserId)
                                        .ToListAsync();


                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Devuelto", "Se ha devuelto un oficio entregado");
                    listUsers.Add(userId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Devolvió el oficio Id: {oficio.IdOficio} al Área {OficioArea.Sections.Name}");
                }

                await _context.SaveChangesAsync();
               
                return Ok(new { registro = OficioArea });
            }
            catch(Exception ex)
            {
                _logger.LogError("Error al intentar actualizar el registro" + ex.Message);
                return StatusCode(500, "error al intentar actualizar el registro");
            }
        }

        [Authorize(Roles = "Coordinador, Asesor")]
        [HttpPost]
        public async Task<IActionResult> DevuelveOficio(int oficioId)
        {
            if (oficioId == 0)
            {
                return NotFound();
            }
            try
            {
                var oficio = await _context.Oficios.FindAsync(oficioId);
                if (oficio != null && oficio.EliminadoLogico == false)
                {
                    oficio.IdEstado = 1;
                    oficio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                    oficio.Modificado = DateTime.Now;
                    oficio.FechaFin = DateTime.Now;
                    _context.Oficios.Update(oficio);
                    await _context.SaveChangesAsync();
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "COORDINADOR");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Oficio Devuelto", "Se devovlió un oficio para Derivar");
                    listUsers.Add(oficio.UserId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo", $"Devolvió el oficio Id: {oficio.IdOficio} a Borradores");
                    return RedirectToAction("Index", new { id = "Pendientes" });
                }
                else
                {
                    return BadRequest("Oficio no encontrado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al devolver el oficio a Borradores" + ex.Message);
                return BadRequest(ex);
            }

        }

        [HttpPost]
        public async Task<IActionResult> ReDerivacion([FromForm] int newACargo, [FromForm] int oficioId)
        {
            // Validar los datos recibidos
            if (newACargo <= 0 || oficioId <= 0)
            {
                return BadRequest("Datos inválidos");
            }

            try
            { 
                var oficio = await _context.Oficios
                                .Include(o => o.Scope)
                                .Where(o => o.IdOficio == oficioId)
                                .FirstOrDefaultAsync();
                string esatbAnterior = "";
                string estabNuevo = "";
                if (oficio != null)
                {
                    estabNuevo = (await _context.Scopes.Where(a => a.ScopeId == newACargo).Select(a => a.ScopeName).FirstAsync()).ToString() ?? "";
                    esatbAnterior = oficio.Scope?.ScopeName.ToString() ?? "";
                    oficio.IdEstabACargo = newACargo;
                    oficio.IdEstado = 2;
                    _context.Oficios.Update(oficio);
                    var areas = await _context.Oficios_x_Area
                                .Where(oa => oa.OficiosId == oficioId)
                                .ToListAsync();

                    if (areas.Any())
                    {
                        _context.Oficios_x_Area.RemoveRange(areas);                  
                    }

                    var observacion = new Observation
                    {
                        IdOficio = oficio.IdOficio,
                        Content = "Este oficio ha sido derivado desde: " + esatbAnterior,
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                        Timestamp = DateTime.Now
                    };
                    await _context.Observation.AddAsync(observacion);

                    await _context.SaveChangesAsync();
                    var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "Anonimo";
                    var listUsers = await UsersByOficioAsync(oficio.IdOficio, "COORDINADOR");
                    var asesores = await UsersByOficioAsync(oficio.IdOficio, "ASESOR");
                    listUsers.AddRange(asesores);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveNotify", "Nuevo Oficio", $"Tienes un Nuevo oficio derivado desde {esatbAnterior}");
                    listUsers.Add(oficio.UserId);
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveTableUpdate");
                    await _hubContext.Clients.Users(listUsers).SendAsync("ReceiveCantidadesUpdate");
                    _logger.Modificacion(usuario, $"Se ha derivado el oficio ID: {oficioId} desde el Establicimiento {esatbAnterior} al {estabNuevo}");
                    return Ok();
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError("Error Re - Derivando oficio ID:" + oficioId.ToString());
                return BadRequest(ex.Message);               
            }
            return Ok(); 
        }
    }
}
