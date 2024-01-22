using System.Diagnostics.Metrics;
using ClothingCo.Api.Meters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using NSubstitute;

namespace ClothingCo.Api.Tests;

public class ApiMeterTests
{
    [Fact]
    public void OrderPlaced_Should_Count_Number_Of_Orders()
    {
        // Arrange
        var services = CreateServiceProvider();
        var metrics = services.GetRequiredService<ApiMeters>();
        var meterFactory = services.GetRequiredService<IMeterFactory>();
        var collector = new MetricCollector<int>(meterFactory, ApiMeters.MeterName, ApiMeters.OrderCounterMeterName);
        
        // Act
        metrics.OrderPlaced();

        // Assert
        var measurements = collector.GetMeasurementSnapshot();
        Assert.Equal(1, measurements.Count);
        Assert.Equal(1, measurements[0].Value);
    }
    
    [Fact]
    public void ProductSold_Should_Count_Number_Of_Products_Sold_By_Quantity_And_Product_Name()
    {
        // Arrange
        var services = CreateServiceProvider();
        var metrics = services.GetRequiredService<ApiMeters>();
        var meterFactory = services.GetRequiredService<IMeterFactory>();
        var collector = new MetricCollector<int>(meterFactory, ApiMeters.MeterName, ApiMeters.ProductsSoldCounterMeterName);
        
        // Act
        metrics.ProductSold(1, "iPhone");
        metrics.ProductSold(3, "iPad");

        // Assert
        var measurements = collector.GetMeasurementSnapshot();
        Assert.Equal(2, measurements.Count);
        Assert.Equal(1, measurements[0].Value);
        Assert.Equal("iPhone", measurements[0].Tags["product"]);
        Assert.Equal(3, measurements[1].Value);
        Assert.Equal("iPad", measurements[1].Tags["product"]);
    }

    [Fact] public void MeasureRequestDuration_Should_Record_The_Duration_Of_The_Request()
    {
        // Arrange
        var services = CreateServiceProvider();
        var metrics = services.GetRequiredService<ApiMeters>();
        var meterFactory = services.GetRequiredService<IMeterFactory>();
        var collector = new MetricCollector<double>(meterFactory, ApiMeters.MeterName, ApiMeters.RequestDurationMeterName);
        
        // Act
        using (var _ = metrics.MeasureRequestDuration("create-order"))
        {
        }

        // Assert
        var measurements = collector.GetMeasurementSnapshot();
        Assert.Equal(1, measurements.Count);
        Assert.Equal(60000, measurements[0].Value);
    }
    
    private static IServiceProvider CreateServiceProvider()
    {
        var jan_12_2024 = 1705035600;
        var jan_12_2024_and_one_minute = 1705035600 + 60;
        
        var timeProviderSub = Substitute.For<TimeProvider>();
        timeProviderSub.TimestampFrequency.Returns(x => 10000000);
        timeProviderSub.GetTimestamp().Returns(
            x => TimeSpan.FromSeconds(jan_12_2024).Ticks,
            x => TimeSpan.FromSeconds(jan_12_2024_and_one_minute).Ticks
        );
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        serviceCollection.AddSingleton<ApiMeters>();
        serviceCollection.AddSingleton(timeProviderSub);
        return serviceCollection.BuildServiceProvider();
    }
}