using ADMentor.ADTechnology;
using EAAddInBase.DataAccess;
using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.ADRepoConnector
{
    class EaToAdRepoConverter
    {
        private readonly ModelEntityRepository repo;

        public EaToAdRepoConverter(ModelEntityRepository repo)
        {
            this.repo = repo;
        }

        public Model ToModel(ModelEntity.Package package)
        {
            var elements = (from p in package.SubPackages
                            from e in p.Elements
                            select new Element
                            {
                                tpe = e.MetaType,
                                path = MkSegments(e, package),
                                attributes = MkAttributes(e)
                            });


            var relations = (from p in package.SubPackages
                             from e in p.Elements
                             from c in e.Connectors
                             from source in c.Source(repo.GetElement)
                             from target in c.Target(repo.GetElement)
                             select Tuple.Create(c.MetaType, source, target))
                            .Distinct()
                            .Select(r => new Relation
                            {
                                tpe = r.Item1,
                                @from = MkSegments(r.Item2, package),
                                to = MkSegments(r.Item3, package)
                            });

            return new Model
            {
                root = MkSegments(package),
                elements = elements,
                relations = relations
            };
        }

        private IDictionary<string, string> MkAttributes(ModelEntity.Element e)
        {
            return e.TaggedValues
                .Concat(new[] { 
                    new KeyValuePair<String, String>("Notes", e.Notes)
                 })
                .Where(p => p.Value != "")
                .ToDictionary();
        }

        private IImmutableList<String> MkSegments(ModelEntity e, ModelEntity.Package root = null)
        {
            if (Options.Some(e).Equals(Options.Some(root)))
            {
                return ImmutableList.Create<String>();
            }
            else
            {
                var pre = (from parent in e.GetParent(repo.GetPackage)
                           select MkSegments(parent, root));

                return pre.Fold(prefix => prefix.Add(e.Name), () => ImmutableList.Create(e.Name));
            }
        }
    }
}
