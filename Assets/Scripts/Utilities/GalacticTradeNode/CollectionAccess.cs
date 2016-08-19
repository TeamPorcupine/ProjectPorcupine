using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Assets.Scripts.Utilities.GalacticTradeNode
{
    public class CollectionAccess<T> where T : IGalacticMarketData
    {
        private readonly MongoDatabase database;
        private readonly MongoCollection<BsonDocument> collection;

        public CollectionAccess(MongoDatabase db)
        {
            database = db;
            collection = database.GetCollection<BsonDocument>(typeof (T).Name);
        }

        public T GetById(int id)
        {
            var document = collection.FindOneById(id);
            var data = BsonSerializer.Deserialize<T>(document);
            return data;
        }

        public List<T> ToList()
        {
            var documents = collection.FindAll().ToList();
            var datas = new List<T>();
            documents.ForEach(d => datas.Add(BsonSerializer.Deserialize<T>(d)));
            return datas;
        }

        public List<T> ToList(IMongoQuery query)
        {
            var documents = collection.Find(query).ToList();
            var datas = new List<T>();
            documents.ForEach(d => datas.Add(BsonSerializer.Deserialize<T>(d)));
            return datas;
        }

        public void Save(T data /*, bool overrideExisting = false*/)
        {
            var document = data.ToBsonDocument();
            collection.Save(document);

            //if (!overrideExisting)
            //{
            //    //no FIXME: the auto id is not working, currently using the tick
            //    //data.Id = (int)DateTime.Now.Ticks;
            //}
            //else
            //{
            //    var document = data.ToBsonDocument();
            //    FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", data.Id);
            //    collection.FindAndModify(filter, document);
            //}
        }
    }
}