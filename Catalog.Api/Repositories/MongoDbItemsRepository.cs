using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Api.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Api.Repositories
{
    public class MongoDbItemsRepository : IItemsRepositories
    {
        private const string databaseName = "catalog";
        private const string collectionName = "items";

        private readonly IMongoCollection<Item> itemsCollection; 
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter; // Need this to find individual items

        public MongoDbItemsRepository(IMongoClient mongoClient) 
        {
            // Need to recieve an instance of our mongodb client
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            itemsCollection = database.GetCollection<Item>(collectionName);
        }

        public async Task CreateItemAsync(Item item)
        {
            await itemsCollection.InsertOneAsync(item);
        }

        public async Task DeleteItemAsync(Guid id)
        {
            var filter = filterBuilder.Eq(item => item.Id, id);
            await itemsCollection.DeleteOneAsync(filter);
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            // Build filter
            var filter = filterBuilder.Eq(item => item.Id, id); // Find the items of the specific id we pass
            return await itemsCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await itemsCollection.Find(new BsonDocument()).ToListAsync(); // Will give us a list of all the items in the collection
        }

        public async Task UpdateItemAsync(Item item)
        {
            var filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
            await itemsCollection.ReplaceOneAsync(filter, item);
        }
    }
}