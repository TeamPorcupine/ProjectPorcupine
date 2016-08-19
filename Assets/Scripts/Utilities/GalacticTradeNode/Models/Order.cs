using System;
using MongoDB.Bson;

namespace Assets.Scripts.Utilities.GalacticTradeNode.Models
{
    public class Order : IGalacticMarketData
    {
        public ObjectId Id { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRunningOrder { get; set; }
        public int ItemId { get; set; }
        public float SingleItemPrice { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public OrderDirection Direction { get; set; }
        public string TraderName { get; set; }
        public string ItemName { get; set; }
    }
}