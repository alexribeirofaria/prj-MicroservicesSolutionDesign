using Application.Abstractions;
using Application.Dtos;
using Application.Implementations;
using AuthService.RegistersExtensions;
using AuthService.Settings;
using CrossCutting.CommonDependenceInject;
using Domain.Entities;
using Repository.Persistency.Generic;
using Repository.UnitOfWork;
using Repository.UnitOfWork.Abstractions;
using Infrastructure.CommonInjectDependence;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddEnvironmentVariables();

// Add services to the container.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.ListenAnyIP(9001);
});

var serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();

builder.Services.StartupBootstrap(builder.Configuration); 
builder.Services.AddConsulSettings(serviceSettings);

builder.Services.AddControllers();
builder.Services.AddScoped(typeof(ICategoriaBusiness<CategoriaDto, Categoria>), typeof(CategoriaBusinessImpl<CategoriaDto>));
builder.Services.AddAutoMapper(typeof(CategoriaProfile).Assembly);
builder.Services.ConfigureMySqlServerContext(builder.Configuration);
builder.Services.AddCrossCuttingConfiguration();
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(GenericRepositorio<>));
builder.Services.AddOptions();

var app = builder.Build();

app.UseConsul(serviceSettings);
app.UseRouting();
app.MapControllers();
app.Run();
