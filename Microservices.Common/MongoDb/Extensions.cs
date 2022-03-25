using Microservices.Common;
using Microservices.Common.Client;
using Microservices.Common.Interfaces;
using Microservices.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Microservices.Common.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddCustomHttpClient(this IServiceCollection services,string baseUrl,IDictionary<string,string> headers=null)
        {
            services.AddSingleton<IHttpCustomClient>(httpCustomClient =>
            {
                return new HttpCustomClient(baseUrl, headers ?? null);
            });
            return services;
        }

        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            //this serializes Guids and DateTimeOffset to strings into MongoDb
            //so they can be in a readable format
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            //initialize the MongoClient and retrieve the Catalog Database
            services.AddSingleton(serviceProvider =>
            {
                //IConfiguration has already been registered in serviceProvider before this point so we can re-use it.
                var configuration = serviceProvider.GetService<IConfiguration>();
                MongoDbSettings mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                //serviceSettings contains microService Name
                ServiceSettings serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>( this IServiceCollection services,string collectionName)
            where T: IEntity
        {
            //register Interface, Service Map
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                //get ImongoDatabase service which has been added in serviceCollection previously
                var database = serviceProvider.GetService<IMongoDatabase>();
                //Construct MongoRepository with database(IMongoDatabase) and collectionName "Items")
                //and map it with IRepository Interface
                return new MongoRepository<T>(database, collectionName);
            });

            return services;
        }
    }
}
