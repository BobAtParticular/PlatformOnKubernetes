using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus.Installation;
using NServiceBus.Logging;
using Sender;
using Shared;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var endpointConfiguration = new EndpointConfiguration("sender");
        endpointConfiguration.CustomDiagnosticsWriter((d, ct) => Task.CompletedTask);

        var transport = new LearningTransport
        {
            StorageDirectory = "/transport"
        };

        var routing = endpointConfiguration.UseTransport(transport);

        routing.RouteToEndpoint(typeof(RequestMessage), "receiver");

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.DefineCriticalErrorAction(async (context, cancellationToken) => {
            try
            {
                await context.Stop(cancellationToken);
            }
            finally
            {
                var message = $"Critical error, shutting down: {context.Error}";
                try
                {
                LogManager.GetLogger(typeof(CriticalError)).Fatal(message, context.Exception);
                }
                finally
                {
                    Environment.FailFast(message, context.Exception);
                }
            }
        });
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        var metrics = endpointConfiguration.EnableMetrics();

        metrics.SendMetricDataToServiceControl(
            serviceControlMetricsAddress: "Particular.Monitoring",
            interval: TimeSpan.FromMilliseconds(500));

        var rootCommand = new RootCommand("Sample endpoint that sends messages and receives a response.");

        var setupOption = new Option<bool>(
            name: "--init",
            description: "Runs initialization routines");

        rootCommand.AddOption(setupOption);

        rootCommand.SetHandler((setupOptionValue) =>
        {
            if (setupOptionValue)
            {
                LogManager.GetLogger<Program>().Info("Initializing sender");
                return Installer.Setup(endpointConfiguration);
            }

            var builder = Host.CreateApplicationBuilder(args);

            builder.UseNServiceBus(endpointConfiguration);
            builder.Services.AddHostedService<MessageSender>();

            var app = builder.Build();
            return app.RunAsync();
        }, setupOption);


        try
        {
            return await rootCommand.InvokeAsync(args);
        }
        catch
        {
            return 1;
        }
    }
}