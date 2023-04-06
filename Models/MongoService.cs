using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservedParser.Models
{
    internal class MongoService
    {
        static private MongoClient client = null!;
        static private IMongoDatabase db = null!;
        static public IMongoCollection<Product> products = null!;
        static private Config config = null!;
        static public void InitMongo(Config config1)
        {
            config = config1;
            client = new MongoClient(config.MongoString);
            db = client.GetDatabase(config.MongoDB);
            products = db.GetCollection<Product>(config.MongoCollection);
        }
        static public void Drop()
        {
            db.DropCollection(config.MongoCollection);
        }
    }
}
