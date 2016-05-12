using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.CopyPasteTaggedValues
{
    public class PackageTree
    {
        public readonly ModelEntity Entity;
        public readonly IImmutableList<PackageTree> Children;
        public readonly bool Selectable;

        public PackageTree(ModelEntity entity, IImmutableList<PackageTree> children = null, bool selectable = true)
        {
            Entity = entity;
            Children = children ?? ImmutableList.Create<PackageTree>();
            Selectable = selectable;
        }


    }
}
