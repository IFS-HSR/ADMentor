using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Model
{
    public class Decision
    {
        public readonly int ID;
        public readonly String Name;
        public readonly Option<int> AddressesIssue;
        public readonly Option<int> InstantiatesAlternative;

        public Decision(int id, string name, Option<int> addressesIssue = null, Option<int> instantiatesAlternative = null)
        {
            ID = id;
            Name = name;
            AddressesIssue = addressesIssue ?? Options.None<int>();
            InstantiatesAlternative = instantiatesAlternative ?? Options.None<int>();
        }

        public Decision CopyWith(int? id = null, string name = null, Option<int> addressesIssue = null, Option<int> instantiatesAlternative = null)
        {
            return new Decision(
                id: id ?? ID,
                name: name ?? Name,
                addressesIssue: addressesIssue ?? AddressesIssue,
                instantiatesAlternative: instantiatesAlternative ?? InstantiatesAlternative
            );
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
