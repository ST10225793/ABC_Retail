using ABC_Retail.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABC_Retail.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _queueClient = new QueueClient(connectionString, "order-processing");
            _queueClient.CreateIfNotExists();
        }

        // Send message to Azure Queue
        public async Task SendMessageAsync(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await _queueClient.SendMessageAsync(message);
            }
        }

        // Peek messages (view without removing from queue)
        public async Task<List<QueueMessageModel>> PeekMessagesAsync(int maxMessages = 32)
        {
            var messageList = new List<QueueMessageModel>();
            PeekedMessage[] peekedMessages = await _queueClient.PeekMessagesAsync(maxMessages);

            foreach (var msg in peekedMessages)
            {
                messageList.Add(new QueueMessageModel
                {
                    MessageId = msg.MessageId,
                    MessageText = msg.MessageText,
                    InsertedOn = msg.InsertedOn
                });
            }

            return messageList;
        }

        // Dequeue (Process and remove top message)
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }

        // Clear all messages
        public async Task ClearQueueAsync()
        {
            await _queueClient.ClearMessagesAsync();
        }
    }
}