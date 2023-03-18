using System.Text.RegularExpressions;
using System.Net.Http;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Microsoft.Extensions.Logging;
using Polly.Timeout;
using Play.Common.MassTransit;
using Play.Common.Identity;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
var services = builder.Services;

const string AllowedOriginSetting = "AllowedOrigin";

builder.Services.AddMongo()
                .AddMongoRepository<InventoryItem>("inventoryitems")
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddMassTransitWithRabbitMq()
                .AddJwtBearerAuthentication();

AddCatalogClient(services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Catalog.Service v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseCors(options =>
    {
        options.WithOrigins(Configuration[AllowedOriginSetting])
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(IServiceCollection services)
{
    Random jitterer = new Random();

    services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001");
    })
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        retryCount: 5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, the making retry {retryAttempt}");
        }

    ))
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
                .LogWarning($"Closing the circuit");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}