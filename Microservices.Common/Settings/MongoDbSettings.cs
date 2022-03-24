using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservices.Common.Settings
{
    public class MongoDbSettings
    {
        //if properties does not change after microsrvice load change set => init so they cannot
        //be changed after initialization
        public string Host { get; init; }
        public int Port { get; init; }

        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}
