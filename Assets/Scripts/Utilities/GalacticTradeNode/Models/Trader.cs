using MongoDB.Bson;

namespace Assets.Scripts.Utilities.GalacticTradeNode.Models
{
    public class Trader : IGalacticMarketData
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string PassworHash { get; set; }
        public float Balance { get; set; }
    }
}