using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Services;

namespace Toolbox.Tools
{
    public static class RestClientExtensions
    {
        public static async Task<T> GetContentAsync<T>(this Task<HttpResponseMessage> message)
        {
            RestResponse<T> response = await message.GetResponseAsync<T>();
            return response.Content;
        }

        public static async Task<T> GetContentAsync<T>(this HttpResponseMessage message)
        {
            RestResponse<T> response = await message.GetResponseAsync<T>();
            return response.Content;
        }

        public static async Task<RestResponse> GetResponseAsync(this Task<HttpResponseMessage> message)
        {
            HttpResponseMessage restResponse = await message;
            return new RestResponse(restResponse);
        }

        public static async Task<RestResponse<T>> GetResponseAsync<T>(this Task<HttpResponseMessage> message)
        {
            HttpResponseMessage restResponse = await message;
            return await GetResponseAsync<T>(restResponse);
        }

        public static async Task<RestResponse<T>> GetResponseAsync<T>(this HttpResponseMessage message, IJson? json = null)
        {
            string contentJson = await message.Content.ReadAsStringAsync();

            if (typeof(T) == typeof(string))
            {
                return new RestResponse<T>(message, (T)(object)contentJson, contentJson);
            }

            T returnType = message.StatusCode == HttpStatusCode.OK ? (json ?? Json.Default).Deserialize<T>(contentJson) : default;
            return new RestResponse<T>(message, returnType!, contentJson);
        }
    }
}
