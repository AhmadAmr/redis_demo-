using k8s.KubeConfigModels;

namespace WebhookAggergator.Models
{
    public class MessengerWebHookModel
    {
        public string? Object { get; set; }
        public List<FbMessageEntry> Entry { get; set; } = new List<FbMessageEntry>();

    }

    public class FbMessageEntry
    {
        public required string Id { get; set; }
        public required long Time { get; set; }
        public List<FbMessaging> Messaging { get; set; } = new List<FbMessaging>();
    }

    public class FbMessaging
    {
        public required FbUser Sender { get; set; }
        public required FbUser Recipient { get; set; }
        public required long Timestamp { get; set; }
        public FbMessage? Message { get; set; }
    }


    public class FbUser
    {
        public required string Id { get; set; }
    }
}
