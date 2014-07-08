using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.Model
{
    public class Issue
    {
        public readonly int ID;
        public readonly String Name;

        public Issue(int id, String name)
        {
            ID = id;
            Name = name;
        }

        public Issue CopyWith(int? id = null, String name = null)
        {
            return new Issue(
                id: id ?? ID,
                name: name ?? Name
            );
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
