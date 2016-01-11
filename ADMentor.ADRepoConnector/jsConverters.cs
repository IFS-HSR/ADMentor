using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using EAAddInBase.Utils;
using System.ComponentModel;
using System.Globalization;

namespace ADMentor.ADRepoConnector
{
    class TagConverter : JavaScriptConverter
    {
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var result = new Dictionary<string, object>();

            obj.TryCast<Tag>().Do(tag =>
            {
                var specificEntries = tag.Match<Tag, IEnumerable<Tuple<String, object>>>()
                    .Case<HeadTag>(ht => new[]{
                        Tuple.Create(TypeKey, HeadTagType as object)
                    })
                    .Case<CustomTag>(ct => new[]{
                        Tuple.Create(TypeKey, CustomTagType as object),
                        Tuple.Create("name", ct.name as object)
                    })
                    .GetOrThrowNotImplemented();

                var commonEntries = new[]{
                    Tuple.Create("modelRoot", tag.modelRoot as object),
                    Tuple.Create("commitId", tag.commitId as object)
                };

                specificEntries.Concat(commonEntries).ForEach(result.Add);
            });

            return result;
        }

        public override object Deserialize(IDictionary<string, object> dict, Type type, JavaScriptSerializer serializer)
        {
            Func<String, IEnumerable<String>, Option<String>, Tag> mkTag = (commitId, modelRoot, nameOpt) =>
                nameOpt.Fold<Tag>(name => new CustomTag
                {
                    name = name,
                    commitId = commitId,
                    modelRoot = modelRoot
                }, () => new HeadTag
                {
                    commitId = commitId,
                    modelRoot = modelRoot
                });

            return (from tpe in dict.Get(TypeKey).Select(serializer.ConvertToType<String>)
                    from commitId in dict.Get("commitId").Select(serializer.ConvertToType<String>)
                    from modelRoot in dict.Get("modelRoot").Select(serializer.ConvertToType<IEnumerable<String>>)
                    let name = dict.Get("name").Select(serializer.ConvertToType<String>)
                    select mkTag(commitId, modelRoot, name))
                        .GetOrDefault();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new Type[] { typeof(Tag) };
            }
        }

        private static String TypeKey = "$type";
        private static String HeadTagType = "ch.hsr.ifs.adRepo.api.HeadTag";
        private static String CustomTagType = "ch.hsr.ifs.adRepo.api.CustomTag";
    }
}
