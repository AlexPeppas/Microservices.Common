﻿using Microservices.Common;
using Microservices.Common.Client;
using Microservices.Common.Interfaces;
using Microservices.Common.Repository;
using Microservices.Common.Settings;
using Microservices.Common.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microservices.Common.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddCustomHttpClient(this IServiceCollection services,HttpClientParameters clientParams)
        {
            /*services.AddSingleton<IHttpCustomClient>(httpCustomClient =>
            {
                return  new HttpCustomClient(baseUrl, headers ?? null);   
            });*/

            //AddHttpClient extension maps the Interface with the Method and set the baseAddress of the client
            //in linq and the custom headers.
            Random jitterer = new Random();
            services.AddHttpClient<IHttpCustomClient, HttpCustomClient>(client =>
            {
                client.BaseAddress = new Uri(clientParams.baseUrl);
                if (clientParams.headers != null && clientParams.headers.Count > 0)
                {
                    foreach (var header in clientParams.headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            })
            //AddTransientHttpErrorPolicy receives a builder which uses Or<> to handle timeout exception and wait and retry on the following logic
            //first input is the retries count
            //second is the retryAttempt which dictates the expo backoff time to wait between each retry.
            //third is an Action delegate onRetry which gets an instance of buildServiceProvider so we can leverage getService from provider and get ILogger interface
            //Logs in console the warning message (DO NOT USE IN PRODUCTION, IT'S SIMULATION LOGGING)
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5, //retries count
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)) //2^1 next retry 2^2 , 2^3 and so on ...
                                +TimeSpan.FromMilliseconds(jitterer.Next(0,1000)),//(plus some randomness not to fire Catalog from multiple clients using Inventory service on exact same time)
                onRetry: (outcome, timeSpan, retryAttempt) => 
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<HttpCustomClient>>()
                    ?.LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }
            ))
            //AddPolicyHandler dictates the client to throw TimeoutRejectedException on specified input timeoutPolicy param.
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(clientParams.timeoutPolicy));

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
