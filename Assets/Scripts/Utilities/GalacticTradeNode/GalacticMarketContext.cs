using System.Collections.Generic;
using Assets.Scripts.Utilities.GalacticTradeNode.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Assets.Scripts.Utilities.GalacticTradeNode
{
    public class GalacticMarketContext
    {
        private readonly MongoClient client;
        private readonly MongoDatabase database;

        const string dbconstr = "mongodb://galactictradenodemaster:galactictradenodemaster2016@ds044229.mlab.com:44229/projectporcupineprimegalactictradenode";
        const string dbname = "projectporcupineprimegalactictradenode";

        public GalacticMarketContext()
        {
            client = new MongoClient(dbconstr);
            database = client.GetServer().GetDatabase(dbname);

            Traders = new CollectionAccess<Trader>(database);
            Orders = new CollectionAccess<Order>(database);
            Transactions = new CollectionAccess<Transaction>(database);
        }

        public CollectionAccess<Trader> Traders;
        public CollectionAccess<Order> Orders;
        public CollectionAccess<Transaction> Transactions;

        public List<Order> GetRunningOrders()
        {
            return Orders.ToList(Query<Order>.EQ(o => o.IsRunningOrder, true));
        }

        public List<Transaction> GetOrderTransactions(int orderId)
        {
            return Transactions.ToList(Query<Transaction>.EQ(t => t.OrderId, orderId));
        }
    }
}