using System.Diagnostics.Metrics;

namespace ClothingCo.Api.Meters;

public class TrackDuration(Histogram<double> histogram, TimeProvider timeProvider, string requestName) : IDisposable
{
    private readonly long _startTime = timeProvider.GetTimestamp();

    public void Dispose()
    {
        var elapsed = timeProvider.GetElapsedTime(this._startTime);
        histogram.Record(elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("requestName", requestName));
    }
}