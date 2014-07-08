using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.Model
{
    public class Alternative
    {
        public readonly int ID;
        public readonly String Name;
        public readonly int AlternativeForIssue;

        public Alternative(int id, String name, int alternativeForIssue)
        {
            ID = id;
            Name = name;
            AlternativeForIssue = alternativeForIssue;
        }

        public Alternative CopyWith(int? id = null, String name = null, int? alternativeForIssue = null)
        {
            return new Alternative(
                id: id ?? ID,
                name: name ?? Name,
                alternativeForIssue: alternativeForIssue ?? AlternativeForIssue
            );
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
