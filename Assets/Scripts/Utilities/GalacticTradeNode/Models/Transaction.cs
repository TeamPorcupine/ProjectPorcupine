using MongoDB.Bson;

namespace Assets.Scripts.Utilities.GalacticTradeNode.Models
{
    public class Transaction : IGalacticMarketData
    {
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public int TraderPartnerName { get; set; }
        public int Quantity { get; set; }
        public int Delivered { get; set; }
        public int DateCreated { get; set; }
        public int ExpirationTimeIfNotDelivered { get; set; }
    }
}
