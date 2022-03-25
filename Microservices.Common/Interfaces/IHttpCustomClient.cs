using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microservices.Common.Interfaces
{
    public interface IHttpCustomClient
    {
        Task<List<R>> GetGenericAsync<T, R>(string endpoint,T request);

        Task<List<R>> PostGenericAsync<T, R>(T request, string endpoint);

        Task<List<R>> PutGenericAsync<T, R>(T request, string endpoint);

        Task<List<R>> GetGenericAsync<R>(string endpoint);
    }
}