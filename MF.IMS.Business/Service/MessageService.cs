using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core;
using Mindflur.IMS.Application.ViewModel.Core;
using System.Text.Json;

namespace Mindflur.IMS.Business.Service
{
    public class MessageService : IMessageService
    {
        private readonly IOptions<CoreSettings> _coreSettings;
        private readonly ILogger<MessageService> _logger;

        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _emailMessageSender;
        private readonly ServiceBusSender _notificationMessagesender;
        public MessageService(IOptions<CoreSettings> coreConfig, ILogger<MessageService> logger)
        {
            _coreSettings = coreConfig;
            _logger = logger;
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };

            _client = new ServiceBusClient(_coreSettings.Value.SBConnectionString, clientOptions);
            _emailMessageSender = _client.CreateSender(_coreSettings.Value.EmailMessageQueueName);
            _notificationMessagesender = _client.CreateSender(_coreSettings.Value.NotificationMessageQueueName);
        }

        public async Task SendEmailMessage(string messageToSend)
        {
            var message = new ServiceBusMessage(messageToSend);

            try
            {
                await _emailMessageSender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to send email message to service bus queue {_coreSettings.Value.EmailMessageQueueName}", ex);
            }
        }

        public async Task SendNotificationMessage(NotificationMessage notificationMessage)
        {
            var message = new ServiceBusMessage(JsonSerializer.Serialize(notificationMessage));

            try
            {
                await _notificationMessagesender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to send notification message to service bus queue {_coreSettings.Value.NotificationMessageQueueName}", ex);
            }
        }
    }
}