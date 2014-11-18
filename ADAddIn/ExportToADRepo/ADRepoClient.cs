using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework.DataAccess;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Utils;

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

        public void ExportPackage(ModelEntity.Package package)
        {

            var entities = from p in package.SubPackages
                           from e in p.Elements
                           from entity in e.Match<AdEntity>()
                           select entity;

            var entityIds = (from e in entities
                             from remoteId in ExportEntity(e)
                             select Tuple.Create(e.Guid, remoteId)).ToDictionary();

            var relations = from e in entities
                            from c in e.Connectors
                            from source in c.Source(repository.GetElement)
                            from sourceId in entityIds.Get(source.Guid)
                            from target in c.Target(repository.GetElement)
                            from targetId in entityIds.Get(target.Guid)
                            from relationType in GetRelationType(c.Stereotype)
                            select Tuple.Create(sourceId, relationType, targetId);

            relations.ForEach(ExportRelation);

            var templateRelations = from e in entities
                                    from t in e.GetClassifier(repository.GetElement)
                                    from sourceId in entityIds.Get(e.Guid)
                                    from targetId in entityIds.Get(t.Guid)
                                    select Tuple.Create(sourceId, "HasTemplate", targetId);

            templateRelations.ForEach(ExportRelation);
        }

        private Option<String> GetRelationType(String stereotype)
        {
            if (stereotype.Equals(ConnectorStereotypes.AddressedBy.Name))
                return Options.Some("AddressedBy");
            if (stereotype.Equals(ConnectorStereotypes.Suggests.Name))
                return Options.Some("Suggests");
            if (stereotype.Equals(ConnectorStereotypes.Raises.Name))
                return Options.Some("Raises");
            return Options.None<String>();
        }

        private void ExportRelation(int sourceId, string relationType, int targetId)
        {
            var ressource = String.Format("element/{0}/relation/{1}/{2}", sourceId, relationType, targetId);
            httpClient.PutAsync(ressource, new StringContent(""));
        }

        public Option<int> ExportEntity(AdEntity entity)
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

            var response = httpClient.PostAsync("element", new StringContent(js.ToString(), Encoding.Default, "application/json")).Result;

            if (response.IsSuccessStatusCode)
            {
                var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return Options.Some((int)json["id"]);
            }

            return Options.None<int>();
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
