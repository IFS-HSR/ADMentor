using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ExportToADRepo
{
    public class ADRepoClient
    {
        private readonly HttpClient httpClient;
        private readonly AdRepository repository;

        public ADRepoClient(Uri repoUrl, AdRepository repo)
        {
            repository = repo;
            httpClient = new HttpClient();
            httpClient.BaseAddress = repoUrl;
        }

        public void ExportEntity(AdEntity entity)
        {
            var js = new JObject{
                {"kind", GetKind(entity)},
                {"path", GetPath(entity)},
                {"attributes", GetAttributes(entity)},
                {"notes", entity.Notes}
            };

            if (entity is OptionOccurrence && entity.Get(SolutionSpace.OptionStateTag).IsDefined)
            {
                js.Add("state", entity.Get(SolutionSpace.OptionStateTag).Value);
            }

            httpClient.PostAsync("element", new StringContent(js.ToString(), Encoding.Default, "application/json"));
        }

        private String GetKind(AdEntity entity)
        {
            if (entity is Problem)
                return "ProblemTemplate";
            if (entity is OptionEntity)
                return "OptionTemplate";
            else
                return entity.GetType().Name;
        }

        private JArray GetPath(AdEntity entity)
        {
            return new JArray(entity.GetPath(repository.GetPackage));
        }

        private JObject GetAttributes(AdEntity entity)
        {
            return entity.TaggedValues
                .Where(pair =>
                    !pair.Key.Equals(SolutionSpace.OptionStateTag.Name) &&
                    !pair.Key.Equals(SolutionSpace.ProblemOccurrenceStateTag.Name))
                .Aggregate(new JObject(), (jobj, pair) =>
                {
                    jobj.Add(pair.Key, pair.Value);
                    return jobj;
                });
        }
    }
}
