using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SUAP_PortalOficios.Data.Repository.Interfaces;
using SUAP_PortalOficios.Models.DTOs;
using System.Security.Claims;

namespace SUAP_PortalOficios.Data.Repository.Implementations
{
    public class InternoRepository : IInternoRepository
    {
        private readonly MyDbContext _context;
        private readonly MyDbContextSuap _contextSuap;
        private readonly IUserPermissionsService _userPermissionsService;


        public InternoRepository(MyDbContextSuap contextSuap, MyDbContext context, IUserPermissionsService userPermissionsService)
        {
            _contextSuap = contextSuap;
            _context = context;
            _userPermissionsService = userPermissionsService;
        }

        /// <summary>
        /// Obtiene los datos del interno ya sea que este alojado en otro establecimiento o en libertad
        /// </summary>
        /// <param name="legajo">Legajo del interno (int)</param>
        /// <returns>un unico resultado, null si no lo encuentra</returns>
        public async Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo) 
        {
            try
            {             
                var query = @"
                            SELECT 
                                i.legajo AS Legajo, 
                                i.apellido AS Apellido, 
                                i.nombre AS Nombre, 
                                td.nombre AS TipoDetencion,                                
                                i.art11 AS Art11, 
                                e.nombre AS Establecimiento,
                                e.idEstablecimiento AS IdEstablecimiento,
                                p.codigopabellon AS Pabellon,
                                (
                                    SELECT STRING_AGG(t.descripcion, ', ') 
                                    FROM Interno_x_Tribunal it
                                    INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                    WHERE it.legajo = i.legajo AND it.actuales = 1
                                ) AS Tribunales,
		                        f.nombre as Fase,
		                        c.descripcion as Concepto,
								i.idTipoConAlojado AS EnLibertad
                            FROM 
                                interno i
                            LEFT JOIN 
                                interno_x_pabellon_x_establecimiento ipe ON ipe.idLegajo = i.legajo
                            LEFT JOIN 
                                establecimiento e ON e.idEstablecimiento = ipe.idEstablecimiento
                            LEFT JOIN 
                                Pabellon p ON p.idPabellon = ipe.idPabellon
                            LEFT JOIN 
                                tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion
	                        LEFT JOIN 
	                        (
		                        SELECT TOP 1 WITH TIES *
		                        FROM DatosConsejo
		                        WHERE legajo = @legajo
		                        ORDER BY fecha_ult_concejo DESC
	                        ) dc ON dc.legajo = i.legajo
	                        left join fase f on f.idFase = dc.idFase
	                        left join concepto c on c.idconcepto = dc.idConcepto	
                            WHERE 
                                i.legajo = @legajo
								and (ipe.fechaHasta is null or i.idTipoConAlojado = 2)                
                            ";

                var paramLegajo = new SqlParameter("@legajo", legajo);

                return await _contextSuap.Database
                    .SqlQueryRaw<InternoResultadoDTO>(query, paramLegajo)
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());  
                return null;
            }
        }

        public async Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo, ClaimsPrincipal user, bool ampliar)
        {
            try
            {               
                var query = @"SELECT 
                                i.legajo AS Legajo, 
                                i.apellido AS Apellido, 
                                i.nombre AS Nombre, 
                                td.nombre AS TipoDetencion, 
                                i.idTipoConAlojado AS EnLibertad,
                                i.art11 AS Art11, 
                                e.nombre AS Establecimiento,
                                e.idEstablecimiento AS IdEstablecimiento,
                                p.codigopabellon AS Pabellon,
                                (
                                    SELECT STRING_AGG(t.descripcion, ', ') 
                                    FROM Interno_x_Tribunal it
                                    INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                    WHERE it.legajo = i.legajo AND it.actuales = 1
                                ) AS Tribunales
                            FROM 
                                interno i
                            INNER JOIN 
                                interno_x_pabellon_x_establecimiento ipe ON ipe.idLegajo = i.legajo
                            INNER JOIN 
                                establecimiento e ON e.idEstablecimiento = ipe.idEstablecimiento
                            INNER JOIN 
                                Pabellon p ON p.idPabellon = ipe.idPabellon
                            INNER JOIN 
                                tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion
                            WHERE 
                                i.legajo = @legajo 
                                and (i.idTipoConAlojado = 2 or ipe.fechaHasta is null)
								
                            ";

                var paramLegajo = new SqlParameter("@legajo", legajo);

                return await _contextSuap.Database
                    .SqlQueryRaw<InternoResultadoDTO>(query, paramLegajo)
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo, ClaimsPrincipal user, bool ampliar, bool enLibertad)
        {
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(user);

                var establecimientos = up.Scopes.ToList();

                var establecimientosString = string.Join(", ", establecimientos);

                var query = @"SELECT 
                                i.legajo AS Legajo, 
                                i.apellido AS Apellido, 
                                i.nombre AS Nombre, 
                                td.nombre AS TipoDetencion, 
                                i.idTipoConAlojado AS EnLibertad,
                                i.art11 AS Art11, 
                                e.nombre AS Establecimiento,
                                e.idEstablecimiento AS IdEstablecimiento,
                                p.codigopabellon AS Pabellon,
                                (
                                    SELECT STRING_AGG(t.descripcion, ', ') 
                                    FROM Interno_x_Tribunal it
                                    INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                    WHERE it.legajo = i.legajo AND it.actuales = 1
                                ) AS Tribunales,
		                        f.nombre as Fase,
		                        c.descripcion as Concepto
                            FROM 
                                interno i
                            LEFT JOIN 
                                interno_x_pabellon_x_establecimiento ipe ON ipe.idLegajo = i.legajo
                            LEFT JOIN 
                                establecimiento e ON e.idEstablecimiento = ipe.idEstablecimiento
                            LEFT JOIN 
                                Pabellon p ON p.idPabellon = ipe.idPabellon
                            LEFT JOIN 
                                tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion
	                        LEFT JOIN 
	                        (
		                        SELECT TOP 1 WITH TIES *
		                        FROM DatosConsejo
		                        WHERE legajo = @legajo
		                        ORDER BY fecha_ult_concejo DESC
	                        ) dc ON dc.legajo = i.legajo
	                        left join fase f on f.idFase = dc.idFase
	                        left join concepto c on c.idconcepto = dc.idConcepto	
                            WHERE 
                                i.legajo = @legajo
                            ";

                if (!enLibertad)
                {
                    query += " AND ipe.fechaHasta IS NULL";
                }
                else
                {
                    query += " AND i.idTipoConAlojado = 2 ";
                    ampliar = true;
                }

                if (!ampliar)
                {
                    if (!establecimientos.Contains(1))
                    {
                        query += " AND ipe.idEstablecimiento IN (" + establecimientosString + ") ";
                    }
                }         

                var paramLegajo = new SqlParameter("@legajo", legajo);

                return await _contextSuap.Database
                        .SqlQueryRaw<InternoResultadoDTO>(query, paramLegajo)
                        .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }


//******************************************************** LISTADOS DE INTERNOS ******************************************************//
        public async Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user)
        {            
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(user);

                var establecimientos = up.Scopes.ToList();

                var establecimientosString = string.Join(", ", establecimientos);

                var query = @"
                        CREATE TABLE #Palabras (
                            Palabra NVARCHAR(50)
                        );
                        INSERT INTO #Palabras (Palabra)
                        SELECT value 
                        FROM STRING_SPLIT(@busqueda, ' ');
                        SELECT 
                            i.legajo AS Legajo, 
                            i.apellido AS Apellido, 
                            i.nombre AS Nombre, 
                            td.nombre AS TipoDetencion, 
                            i.art11 AS Art11, 
                            e.nombre AS Establecimiento,
                            e.idEstablecimiento AS IdEstablecimiento,
                            p.codigopabellon AS Pabellon,
                            (SELECT STRING_AGG(t.descripcion, ', ') 
                                FROM Interno_x_Tribunal it
                                INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                WHERE it.legajo = i.legajo AND it.actuales = 1) AS tribunales
                        FROM 
                            interno i
                        INNER JOIN 
                            interno_x_pabellon_x_establecimiento ipe ON ipe.idLegajo = i.legajo
                        INNER JOIN 
                            establecimiento e ON e.idEstablecimiento = ipe.idEstablecimiento
                        INNER JOIN 
                            pabellon p ON p.idPabellon = ipe.idPabellon
                        INNER JOIN 
                            tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion
                        WHERE 
                            (NOT EXISTS (
                                SELECT 1 
                                FROM #Palabras p
                                WHERE i.nombre NOT LIKE CONCAT('%', p.Palabra, '%')
                                AND i.apellido NOT LIKE CONCAT('%', p.Palabra, '%')
                            ))
                            AND ipe.fechaHasta IS NULL ";

                if (!establecimientos.Contains(1))
                {
                    query += "AND ipe.idEstablecimiento IN (" + establecimientosString + ") ";
                }
                query += " DROP TABLE #Palabras";
                var param = new SqlParameter("@busqueda", busqueda);


                // Transformar los resultados a DTOs
                return await _contextSuap.Database
                        .SqlQueryRaw<InternoResultadoDTO?>(query, param)
                        .OrderBy(i => i.Apellido ?? string.Empty)
                        .ThenBy(i => i.Nombre ?? string.Empty)  
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user, bool ampliar)
        {
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(user);

                var establecimientos = up.Scopes.ToList();

                var establecimientosString = string.Join(", ", establecimientos);

                var query = @"
                       
                            AND ipe.idEstablecimiento IN (" + establecimientosString + ") DROP TABLE #Palabras";

                var param = new SqlParameter("@busqueda", busqueda);


                // Transformar los resultados a DTOs
                return await _contextSuap.Database
                        .SqlQueryRaw<InternoResultadoDTO?>(query, param)
                        .OrderBy(i => i.Apellido ?? string.Empty)
                        .ThenBy(i => i.Nombre ?? string.Empty)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user, bool ampliar, bool enLibertad)
        {
            try
            {
                var up = await _userPermissionsService.GetPermissionsAsync(user);

                var establecimientos = up.Scopes.ToList();

                var establecimientosString = string.Join(", ", establecimientos);

                var query = @"
                        WITH Palabras AS (
                            SELECT value AS Palabra
                            FROM STRING_SPLIT(@busqueda, ' ')
                        )
                        SELECT DISTINCT
                            i.legajo AS Legajo, 
                            i.apellido AS Apellido, 
                            i.nombre AS Nombre, 
                            i.idTipoConAlojado AS EnLibertad,
                            td.nombre AS TipoDetencion, 
                            i.art11 AS Art11, 
                            e.nombre AS Establecimiento,
                            e.idEstablecimiento AS IdEstablecimiento,
                            p.codigopabellon AS Pabellon,
                            (SELECT STRING_AGG(t.descripcion, ', ') 
                                FROM Interno_x_Tribunal it
                                INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                WHERE it.legajo = i.legajo AND it.actuales = 1) AS tribunales,
                            f.nombre AS Fase,
                            c.descripcion AS Concepto
                        FROM 
                            interno i
                        INNER JOIN 
                            interno_x_pabellon_x_establecimiento ipe ON ipe.idLegajo = i.legajo
                        INNER JOIN 
                            establecimiento e ON e.idEstablecimiento = ipe.idEstablecimiento
                        INNER JOIN 
                            pabellon p ON p.idPabellon = ipe.idPabellon
                        INNER JOIN 
                            tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion
                        LEFT JOIN (
                            SELECT *
                            FROM (
                                SELECT *,
                                    ROW_NUMBER() OVER (PARTITION BY legajo ORDER BY fecha_ult_concejo DESC) AS RowNum
                                FROM DatosConsejo
                            ) AS SubQuery
                            WHERE RowNum = 1
                        ) dc ON dc.legajo = i.legajo
                        LEFT JOIN 
                            fase f ON f.idFase = dc.idFase
                        LEFT JOIN 
                            concepto c ON c.idconcepto = dc.idConcepto
                        WHERE 
                             NOT EXISTS (
                                SELECT 1
                                FROM Palabras p
                                WHERE i.nombre  NOT LIKE   CONCAT('%', p.Palabra, '%') 
                                and i.apellido  NOT LIKE   CONCAT('%', p.Palabra, '%') 
                            )
                        and i.nombre is not null and i.apellido is not null
                             ";
              
                if (!enLibertad)
                {
                    query += " AND ipe.fechaHasta IS NULL ";
                }
                else
                {
                    query = @"
                        WITH Palabras AS (
                            SELECT value AS Palabra
                            FROM STRING_SPLIT(@busqueda, ' ')
                        )
                        SELECT DISTINCT
                            i.legajo AS Legajo, 
                            i.apellido AS Apellido, 
                            i.nombre AS Nombre, 
                           2 AS EnLibertad,
                            td.nombre AS TipoDetencion, 
                            i.art11 AS Art11, 
                            null AS Establecimiento,
                            null AS IdEstablecimiento,
                            null AS Pabellon,
                            (SELECT STRING_AGG(t.descripcion, ', ') 
                                FROM Interno_x_Tribunal it
                                INNER JOIN tribunal t ON t.idTribunal = it.idTribunal
                                WHERE it.legajo = i.legajo AND it.actuales = 1) AS tribunales, 
								null as Fase,
								null as Concepto
                        FROM 
                            interno i
                       
                        LEFT JOIN 
                            tipoDetencion td ON td.idTipoDetencion = i.idTipoDetencion       
                        WHERE 
                             NOT EXISTS (
                                SELECT 1
                                FROM Palabras p
                                WHERE i.nombre  NOT LIKE   CONCAT('%', p.Palabra, '%') 
                                and i.apellido  NOT LIKE   CONCAT('%', p.Palabra, '%') 
                            )
                        and i.nombre is not null and i.apellido is not null
                              AND i.idTipoConAlojado = 2 
							  ";
                    ampliar = true;
                }

                if (!ampliar)
                {
                    if (!establecimientos.Contains(1))
                    {
                        query += " AND ipe.idEstablecimiento IN (" + establecimientosString + ") ";
                    }
                }

                query += " ORDER BY i.apellido ASC, i.nombre ASC ";

                var param = new SqlParameter("@busqueda", busqueda);

                // Transformar los resultados a DTOs
                return await _contextSuap.Database
                        .SqlQueryRaw<InternoResultadoDTO?>(query, param)                       
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
