using System.Diagnostics.Metrics;

namespace ClothingCo.Api.Meters;

public sealed class ApiMeters
{
    private readonly TimeProvider _timeProvider;
    private readonly Counter<int> _orderCounter;
    private readonly Counter<int> _productsSoldCounter;
    private readonly Histogram<double> _requestDuration;

    public static string MeterName = "ClothingCo.Api.Meters";
    public static string OrderCounterMeterName = "clothing_co.api.meters.orders";
    public static string ProductsSoldCounterMeterName = "clothing_co.api.meters.product_sold";
    public static string RequestDurationMeterName = "clothing_co.api.meters.request_duration";
    
    public ApiMeters(IMeterFactory meterFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        
        var meter = meterFactory.Create(MeterName);
        _orderCounter = meter.CreateCounter<int>(OrderCounterMeterName);
        _productsSoldCounter = meter.CreateCounter<int>(ProductsSoldCounterMeterName);
        _requestDuration = meter.CreateHistogram<double>(RequestDurationMeterName,
            unit: "milliseconds");
    }

    public void OrderPlaced()
    {
        this._orderCounter.Add(1);
    }
    
    public void ProductSold(int quantity, string productName)
    {
        this._productsSoldCounter.Add(quantity, 
            new KeyValuePair<string, object?>("product", productName));
    }
    
    public TrackDuration MeasureRequestDuration(string requestName)
    {
        return new TrackDuration(this._requestDuration, this._timeProvider, requestName);
    }
}