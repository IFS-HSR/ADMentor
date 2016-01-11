using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ADMentor.ADRepoConnector
{
    sealed class ADRepoClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly JavaScriptSerializer serializer;

        public ADRepoClient(Uri repoUrl)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = repoUrl;
            serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[]{
                new TagConverter()
            });
        }

        private static MediaTypeWithQualityHeaderValue MediaType = new MediaTypeWithQualityHeaderValue("application/json");

        public async Task<IEnumerable<Tag>> GetModels()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "model");
            req.Headers.Accept.Add(MediaType);
            var res = await httpClient.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                return serializer.Deserialize<IEnumerable<Tag>>(content);
            }
            else
            {
                throw new ApplicationException(res.ReasonPhrase);
            }
        }

        public async Task<String> CommitModel(Commit commit)
        {
            var json = serializer.Serialize(commit);

            var response = await httpClient.PostAsync("model", new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new ApplicationException(response.ReasonPhrase);
            }
        }

        public void Dispose()
        {
            if (httpClient != null)
                httpClient.Dispose();
        }
    }
}
