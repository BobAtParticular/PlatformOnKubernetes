namespace Sender;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using Shared;

class MessageSender(ILogger<MessageSender> logger, IMessageSession messageSession) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var guid = Guid.NewGuid();

            var message = new RequestMessage
            {
                Id = guid,
                Data = Convert.ToBase64String(guid.ToByteArray())
            };

            await messageSession.Send(message, cancellationToken);

            logger.LogInformation($"Message sent, requesting to get data by id: {guid:N}");

            await Task.Delay(250);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
