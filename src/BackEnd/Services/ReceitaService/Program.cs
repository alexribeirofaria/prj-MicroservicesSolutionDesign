using Application.Abstractions;
using Application.Dtos;
using Application.Dtos.Profile;
using Application.Implementations;
using AuthService.RegistersExtensions;
using AuthService.Settings;
using Domain.Entities;
using Infrastructure.CommonInjectDependence;
using Repository.Persistency.Generic;
using Repository.UnitOfWork;
using Repository.UnitOfWork.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddEnvironmentVariables();

// Add services to the container.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.ListenAnyIP(9003);
});

var serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
builder.Services.StartupBootstrap(builder.Configuration);
builder.Services.AddConsulSettings(serviceSettings);

builder.Services.AddControllers();
builder.Services.AddScoped(typeof(IBusinessBase<ReceitaDto, Receita>), typeof(ReceitaBusinessImpl<ReceitaDto>));
builder.Services.AddAutoMapper(typeof(ReceitaProfile).Assembly);
builder.Services.ConfigureMySqlServerContext(builder.Configuration);
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(GenericRepositorio<>));
builder.Services.AddOptions();

var app = builder.Build();

app.UseConsul(serviceSettings); 
app.UseRouting();
app.MapControllers();
app.Run();
