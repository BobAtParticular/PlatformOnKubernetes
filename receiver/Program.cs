using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Installation;
using NServiceBus.Logging;
using Shared;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        LogManager.GetLogger<Program>().Info("Starting receiver");
        var endpointConfiguration = new EndpointConfiguration("receiver");
        endpointConfiguration.CustomDiagnosticsWriter((d, ct) => Task.CompletedTask);

        var transport = new LearningTransport
        {
            StorageDirectory = "/transport"
        };

        endpointConfiguration.UseTransport(transport);

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

        var rootCommand = new RootCommand("Sample endpoint that receives messages and sends a response.");

        var setupOption = new Option<bool>(
            name: "--init",
            description: "Runs initialization routines");

        rootCommand.AddOption(setupOption);

        rootCommand.SetHandler((setupOptionValue) =>
        {
            LogManager.GetLogger<Program>().Info($"Processing receiver root command. --init is set to {setupOptionValue}");
            if (setupOptionValue)
            {
                LogManager.GetLogger<Program>().Info("Initializing receiver");
                return Installer.Setup(endpointConfiguration);
            }

            var builder = Host.CreateApplicationBuilder(args);

            builder.UseNServiceBus(endpointConfiguration);

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