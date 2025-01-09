using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SUAP_PortalOficios.Data;
using SUAP_PortalOficios.Data.Repository.Implementations;
using SUAP_PortalOficios.Data.Repository.Interfaces;
using SUAP_PortalOficios.Hubs;
using SUAP_PortalOficios.Views.Shared.Components.Services;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = environment
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
//añade RazorPages 
builder.Services.AddRazorPages();
builder.Services.AddHttpClient(); 
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    })
     .AddCircuitOptions(options => { options.DetailedErrors = true; }); 
// Add Blazorise 
builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
})
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

// configuracion de coneccion a la BBDD
var connectionString = builder.Configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");
builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
var connectionStringSuap = builder.Configuration.GetConnectionString("ConnectionSuap") ?? throw new InvalidOperationException("No se pudo conectar con Base de Datos Suap");
builder.Services.AddDbContext<MyDbContextSuap>(options =>
{
    options.UseSqlServer(connectionStringSuap);
});

// configuracion de Identity
builder.Services.AddIdentity<MyUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Tiempo de inactividad permitido
    options.SlidingExpiration = true; // Renueva la cookie si el usuario está activo
    options.LoginPath = "/Identity/Account/Login";  // configuracion de la redireccion en caso de que no este autorizado el usuario
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // configuracion de la pagina de acceso denegado
}
);

//Añadir servicios de antifalsificación
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery";
options.HeaderName = "X-CSRF-TOKEN"; // Usar un encabezado para enviar el token CSRF
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<HttpClientService>();

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Logging a la consola
    .WriteTo.Debug() // Logging al depurador
    .WriteTo.File(
        path: "Logs/General-.log", // Archivo para logs generales
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90
    )
    .WriteTo.File(
        path: "Logs/Errores-.log", // Archivo para los logs de error
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
        outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level}] {Message}{NewLine}Error: {Exception}"
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(m => m.RenderMessage().Contains("[MOD]"))
        .WriteTo.File(
        path: "Logs/Modificaciones-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} {Message}{NewLine}{Exception}"
        ))   
    .CreateLogger();

builder.Host.UseSerilog(); 


// Register the repositories
builder.Services.AddScoped<IInternoRepository, InternoRepository>();

builder.Services.AddSignalR();

builder.Services.AddMemoryCache(); // Añade el servicio de caché en memoria
builder.Services.AddScoped<IUserPermissionsService, UserPermissionsService>(); // Registra el servicio UserPermissionsService



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();

app.MapHub<UpdateHub>("/UpdateHub");
//app.MapHub<MessageHub>("/Hubs/MessageHub");
//app.MapHub<SignalRConnection>("/SignalRConnection");

app.Run();
