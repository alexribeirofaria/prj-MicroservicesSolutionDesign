using EasyCryptoSalt;
using LiteStreaming.STS;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using STS.Server.Autorization;
using STS.ServerData;
using STS.ServerData.Interfaces;
using STS.ServerData.Options;
using STS.ServerGrantType;
using STS.ServerProfileService;
using STS.ServerSwaggerUIDocumentation;

// Appliction Parameteres
var appName = "Security Token Service";
var currentVersion = "v1";
var appDescription = $"Este � um componente de Servi�os de Token de Seguran�a para autenticar e autorizar acesso a servi�os. ";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(c =>
{
    c.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();

    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(currentVersion, new OpenApiInfo
    {
        Title = appName,
        Version = currentVersion,
        Description = appDescription,
        Contact = new OpenApiContact
        {
            Name = "Alex Ribeiro de Faria - Projeto Security Token Service",
            Url = new Uri("https://github.com/alexfariakof/prj-sistemas-web-.net-infnet/tree/main/LiteStreaming.STS")
        }
    });

    c.AddDocumentFilterInstance<AuthenticationOperationFilter>(new AuthenticationOperationFilter());
});

builder.Services.AddMvc();
builder.Services.AddControllersWithViews();
//builder.Services.Configure<DataBaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));

// Carrega as configura��es de User Secrets (somente no ambiente de desenvolvimento)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(); // Ou qualquer outra classe do projeto
    // Configura as op��es CryptoOptions a partir da se��o CryptoConfigurations
    builder.Services.Configure<CryptoOptions>(builder.Configuration.GetSection("CryptoConfigurations"));
}
else
{
    // Adiciona configura��es de ambiente (que inclui vari�veis de ambiente)
    builder.Configuration.AddEnvironmentVariables();
    // Registra as op��es de configura��o de CryptoOptions
    builder.Services.Configure<CryptoOptions>(builder.Configuration.GetSection("CryptoConfigurations"));
}

// Registra o servi�o ICrypto com a implementa��o Crypto
builder.Services.AddSingleton<ICrypto, Crypto>();
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();

builder.Services.Configure<DataBaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services
    .AddIdentityServer()   
    .AddSigningCredential(SigningCredential.LoadCertificate("sts-cert.pfx", "12345!"))
    .AddInMemoryIdentityResources(IdentityServerConfigurations.GetIdentityResource())
    .AddInMemoryApiResources(IdentityServerConfigurations.GetApiResources())
    .AddInMemoryApiScopes(IdentityServerConfigurations.GetApiScopes())
    .AddInMemoryClients(IdentityServerConfigurations.GetClients())
    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
    .AddProfileService<ProfileService>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"/swagger/{currentVersion}/swagger.json", $"{currentVersion} {appName} ");
    });
    app.UseDeveloperExceptionPage();
}

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseIdentityServer();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();



// https://localhost:7199/.well-known/openid-configuration
// https://github.com/identityServer/IdentityServer4.Quickstart.UI