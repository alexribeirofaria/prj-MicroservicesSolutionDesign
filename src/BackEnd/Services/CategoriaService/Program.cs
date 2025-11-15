using Application.Abstractions;
using Application.Dtos;
using Application.Implementations;
using AuthService.RegistersExtensions;
using AuthService.Settings;
using CrossCutting.CommonDependenceInject;
using Domain.Entities;
using Migrations.MySqlServer.CommonInjectDependence;
using Repository.Persistency.Generic;
using Repository.UnitOfWork;
using Repository.UnitOfWork.Abstractions;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// Configurar Kestrel (porta 9000 e remover header do servidor)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.ListenAnyIP(9001);
});

var serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
builder.Services.StartupBootstrap(builder.Configuration); 
builder.Services.AddConsulSettings(serviceSettings);

builder.Services.AddControllers();
builder.Services.ConfigureMySqlServerMigrationsContext(builder.Configuration);
builder.Services.AddScoped(typeof(ICategoriaBusiness<CategoriaDto, Categoria>), typeof(CategoriaBusinessImpl<CategoriaDto>));
builder.Services.AddAutoMapper(typeof(CategoriaProfile).Assembly);
builder.Services.AddCrossCuttingConfiguration();
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(GenericRepositorio<>));
builder.Services.AddOptions();

var app = builder.Build();

app.UseConsul(serviceSettings);
app.UseRouting();
app.MapControllers();
app.Run();
