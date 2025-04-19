using WebhookAggergator.Models;

namespace WebhookAggergator.Services
{
    public interface IMessageAggregator
    {
        /// <summary>
        /// Queues a message for processing.
        /// </summary>
        /// <param name="senderId">The ID of the sender.</param>
        /// <param name="message">The message to queue.</param>
        void QueueMessage(string senderId, FbMessage message);
        /// <summary>
        /// Processes the queued messages.
        /// </summary>
        void ProcessMessages(string userId);
    }
}
