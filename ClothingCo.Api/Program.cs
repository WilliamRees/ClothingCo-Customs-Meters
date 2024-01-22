using Azure.Monitor.OpenTelemetry.Exporter;
using ClothingCo.Api.Meters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ApiMeters>();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(ApiMeters.MeterName);
        metrics.AddAzureMonitorMetricExporter(x =>
        {
            x.ConnectionString = "";
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/orders", (
        [FromBody] OrderRequest request, 
        [FromServices] ApiMeters meters) =>
    {
        using var _ = meters.MeasureRequestDuration("create-order");
        meters.OrderPlaced();
        meters.ProductSold(request.Quantity, request.ProductName);
        
        Thread.Sleep(Random.Shared.Next(50, 500));
        return new OrderResponse(
            Guid.NewGuid().ToString(), 
            request.ProductName, 
            request.Quantity);
    })
    .WithName("CreateOrder")
    .WithOpenApi();

Console.WriteLine(Environment.ProcessId);

app.Run();

record OrderRequest(string ProductName, int Quantity);

record OrderResponse(string Id, string ProductName, int Quantity);
