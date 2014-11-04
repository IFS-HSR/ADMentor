﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework.MDGBuilder
{
    public class ModelId
    {
        public ModelId(String technology, int version)
        {
            Technology = technology;
            Version = version;
        }

        public string Technology { get; private set; }

        public int Version { get; private set; }

        public bool IsPredecessorOf(ModelId other)
        {
            return Technology == other.Technology && Version < other.Version;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", Technology, Version);
        }

        public override bool Equals(object obj)
        {
            return obj.Match<ModelId>().Match(
                otherId => Technology.Equals(otherId.Technology) && Version.Equals(otherId.Version),
                () => false);
        }

        public static Option<ModelId> Parse(String id)
        {
            var parts = id.Split(':');

            if (parts.Count() == 2)
            {
                int version = 0;
                int.TryParse(parts[1], out version);
                return Options.Some(new ModelId(parts[0], version));
            }
            else
            {
                return Options.None<ModelId>();
            }
        }
    }
}
