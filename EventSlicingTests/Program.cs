using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using Oakton.Resources;
using Weasel.Core;
using WolverineTests;

var builder = new HostBuilder();

// HOWTO:
// ensure that your postgres db runs according to configuration / adapt configuration.
// e.g. start a docker container with postgres using the following command, afterward you can launch this console application:
// docker run --rm -it --name "postgres" -p 5432:5432 --env "POSTGRES_USER=demo" --env "POSTGRES_PASSWORD=demo" --env "POSTGRES_DB=demo" "postgres:alpine" "postgres"

builder.ConfigureAppConfiguration(configure => configure.AddJsonFile("appsettings.json"));

builder.ConfigureServices((context, services) =>
{
    services
        .AddMarten(options =>
        {
            var connectionString = context.Configuration.GetConnectionString("Marten");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Need to configure connection to Marten first!");
            }

            options.Connection(connectionString);
            if (context.HostingEnvironment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }

            options.Projections.Add<MeasurementProjection>(ProjectionLifecycle.Inline);
        })
        .UseLightweightSessions()
        .ApplyAllDatabaseChangesOnStartup();

    services.AddResourceSetupOnStartup();
    services.AddHostedService<MeasurementProducer>();
});

builder.ApplyOaktonExtensions();

var app = builder.Build();
await app.RunOaktonCommands(args);

Console.WriteLine("Finished!");
