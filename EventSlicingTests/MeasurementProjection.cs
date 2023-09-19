using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Marten.Storage;

namespace WolverineTests;

public class MeasurementProjection : MultiStreamProjection<Measurement, string>
{
    public class CustomSlicer : IEventSlicer<Measurement, string>
    {
        public ValueTask<IReadOnlyList<EventSlice<Measurement, string>>> SliceInlineActions(IQuerySession querySession, IEnumerable<StreamAction> streams)
        {
            var slices = streams
                .SelectMany(s => s.Events)
                .Where(e => e.Data is MeasurementTaken)
                .GroupBy(s =>
                {
                    var dt = s.Timestamp;
                    return (s.StreamId,
                        Timestamp: new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
                })
                .Select(g =>
                {
                    var id = $"{g.Key.StreamId}-{g.Key.Timestamp:yyyy.MM.dd hh:mm:ss}";
                    var eventSlice = new EventSlice<Measurement, string>(id, querySession, g.ToList());

                    return eventSlice;
                })
                .ToList();

            return new (slices);
        }

        public ValueTask<IReadOnlyList<TenantSliceGroup<Measurement, string>>> SliceAsyncEvents(IQuerySession querySession, List<IEvent> events)
        {
            var slices = events
                .Where(e => e.Data is MeasurementTaken)
                .GroupBy(s =>
                {
                    var dt = s.Timestamp;
                    return (s.StreamId,
                        Timestamp: new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
                })
                .Select(g =>
                {
                    var tenantSliceGroup = new TenantSliceGroup<Measurement, string>(Tenant.ForDatabase(querySession.Database));
                    tenantSliceGroup.AddEventsWithMetadata<MeasurementTaken>(
                        e => $"{e.StreamId}-{e.Timestamp:yyyy.MM.dd hh:mm:ss}",
                        g.ToList());   
                    
                    return tenantSliceGroup;
                })
                .ToList();

            return new(slices);
        }
    }
    
    public MeasurementProjection()
    {
        CustomGrouping(new CustomSlicer());
    }
}