using Microservices.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace Microservices.Common.Client
{
    public class HttpCustomClient : IHttpCustomClient
    {
        private static HttpClient HttpClient { get; set; }

        private static string _baseUrl;
        private static IDictionary<string, string> _headers;

        public HttpCustomClient(string BaseUrl, IDictionary<string, string> Headers=null)
        {
            _baseUrl = BaseUrl;
            _headers = Headers;
            InitializeClient();
        }

        private void InitializeClient()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            
            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        private void BuildQueries<T>(T request,string endpoint)
        {
            var properties = request.GetType().GetProperties();
            if (properties.Count() > 0)
            {
                var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
                foreach (var prop in properties)
                {
                    var objValue = prop.GetValue(request, null);
                    if (objValue != null)
                    {
                        string value = objValue.ToString();
                        if (!string.IsNullOrEmpty(value))
                            query.Add(prop.Name.ToString(), value);
                    }
                }

                string queryString = query.ToString();
                _baseUrl += "/"+endpoint;
                _baseUrl += $"?{queryString}";
            }
        }

        public async Task<List<R>> GetGenericAsync<T, R>(string endpoint,T request)
        {
            if (request != null)
                BuildQueries(request,endpoint);

            try
            {
                using (HttpResponseMessage clientResponse = await HttpClient.GetAsync(_baseUrl))
                {
                    if (clientResponse.IsSuccessStatusCode)
                    {
                        List<R> result = await clientResponse.Content.ReadAsAsync<List<R>>();
                        return result;
                    }
                    else
                    {
                        throw new Exception(clientResponse.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<List<R>> GetGenericAsync<R>(string endpoint)
        {
            _baseUrl += "/" + endpoint;
            try
            {
                using (HttpResponseMessage clientResponse = await HttpClient.GetAsync(_baseUrl))
                {
                    if (clientResponse.IsSuccessStatusCode)
                    {
                        List<R> result = await clientResponse.Content.ReadAsAsync<List<R>>();
                        return result;
                    }
                    else
                    {
                        throw new Exception(clientResponse.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<List<R>> PostGenericAsync<T, R>(T request,string endpoint)
        {
            _baseUrl += "/" + endpoint;
            try
            {
                using (HttpResponseMessage clientResponse = await HttpClient.PostAsJsonAsync<object>(_baseUrl, request))
                {
                    if (clientResponse.IsSuccessStatusCode)
                    {
                        List<R> result = await clientResponse.Content.ReadAsAsync<List<R>>();
                        return result;
                    }
                    else
                    {
                        throw new Exception(clientResponse.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<List<R>> PutGenericAsync<T, R>(T request,string endpoint)
        {
            _baseUrl += "/" + endpoint;
            try
            {
                using (HttpResponseMessage clientResponse = await HttpClient.PutAsJsonAsync<object>(_baseUrl, request))
                {
                    if (clientResponse.IsSuccessStatusCode)
                    {
                        List<R> result = await clientResponse.Content.ReadAsAsync<List<R>>();
                        return result;
                    }
                    else
                    {
                        throw new Exception(clientResponse.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

    }
}
