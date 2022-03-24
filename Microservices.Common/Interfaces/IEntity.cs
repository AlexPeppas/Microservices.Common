using System;

namespace Microservices.Common.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}