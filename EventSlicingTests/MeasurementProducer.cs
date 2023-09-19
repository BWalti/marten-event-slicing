using System.Diagnostics;
using Marten;
using Microsoft.Extensions.Hosting;

namespace WolverineTests;

public class MeasurementProducer : IHostedService
{
    private readonly IDocumentStore _store;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _task;

    public static readonly Guid StreamId = new ("A6860542-49A5-48D4-B95B-4739E04D88FC");

    public MeasurementProducer(IDocumentStore store)
    {
        _store = store;
        _cancellationTokenSource = new CancellationTokenSource();
        _task = new Task(Run, _cancellationTokenSource.Token);
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task.Start();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        await _task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
    }

    private void Run()
    {
        _store.Advanced.ResetAllData();
        
        ProduceMeasurements();
        
        Thread.Sleep(TimeSpan.FromMilliseconds(300));
        ProduceMeasurements();
        ProduceMeasurements();
        
        Console.WriteLine("You can now end the application using Ctrl+C!");
        return;
        
        void ProduceMeasurements()
        {
            using var session = _store.LightweightSession();

            var rnd = new Random();
            Console.WriteLine("Starting to produce Measurements...");

            var counter = 0;
            var start = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();
            while (!_cancellationTokenSource.IsCancellationRequested && DateTime.Now - start < TimeSpan.FromSeconds(2))
            {
                counter++;
                var value = rnd.NextDouble() * 30 + 5;
                session.Events.Append(StreamId, new MeasurementTaken(value, DateTime.Now));
                session.SaveChanges();

                var deltaMillis = 100 - stopwatch.ElapsedMilliseconds;
                if (deltaMillis > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(deltaMillis));
                }

                stopwatch.Restart();
            }

            stopwatch.Stop();
            Console.WriteLine($"Finished producing {counter}# Measurements.");
        }
    }
}