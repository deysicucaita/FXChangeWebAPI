using FXChangeWebAPI.Abstractions;
using FXChangeWebAPI.Context;
using FXChangeWebAPI.Endpoints;
using FXChangeWebAPI.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpoints();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

// Add DbContext for SQL Server.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//APIExchange 
builder.Services.AddHttpClient("ForeignExchangeAPI", client =>
{

    client.BaseAddress = new Uri("https://www.alphavantage.co/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    // Configura otros aspectos del HttpClient si es necesario.
});

// Registrar AlphaVantageApiClient como servicios
builder.Services.AddTransient<AlphaVantageClient>();

// registra IFXRatesApiClient con su implementación
builder.Services.AddTransient<IFXRatesApiClient, FXRatesApiClient>();
builder.Services.AddTransient<AlphaVantageClient, AlphaVantageClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();