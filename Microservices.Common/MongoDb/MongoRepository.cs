using Microservices.Common.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microservices.Common.Repository
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;

        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoDatabase database,string collectionName)
        {
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty)?.ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter)?.ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid Id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(it => it.Id, Id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(T Item)
        {
            if (Item == null)
                throw new ArgumentNullException(nameof(Item));

            await dbCollection.InsertOneAsync(Item);
        }

        public async Task UpdateAsync(T Item)
        {
            if (Item == null)
                throw new ArgumentNullException(nameof(Item));

            FilterDefinition<T> filter = filterBuilder.Eq(existingItem => existingItem.Id, Item.Id);
            await dbCollection.ReplaceOneAsync(filter, Item);
        }

        public async Task DeleteAsync(Guid Id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(it => it.Id, Id);
            await dbCollection.DeleteOneAsync(filter);
        }

    }
}
