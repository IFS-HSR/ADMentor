using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class Common
    {
        public class OrganisationalReachValue : Enumeration
        {
            public static readonly OrganisationalReachValue Global = new OrganisationalReachValue("Global");
            public static readonly OrganisationalReachValue Organisation = new OrganisationalReachValue("Organisation");
            public static readonly OrganisationalReachValue Program = new OrganisationalReachValue("Program");
            public static readonly OrganisationalReachValue Project = new OrganisationalReachValue("Project");
            public static readonly OrganisationalReachValue Subproject = new OrganisationalReachValue("Subproject");
            public static readonly OrganisationalReachValue BusinessUnit = new OrganisationalReachValue("Business Unit");
            public static readonly OrganisationalReachValue Individual = new OrganisationalReachValue("Individual");

            public static readonly IEnumerable<OrganisationalReachValue> All = new[] {
                Global, Organisation, Program, Project, Subproject, BusinessUnit, Individual
            };

            private OrganisationalReachValue(String name) : base(name) { }
        }

        public static readonly TaggedValue IntellectualPropertyRights = new TaggedValue(
            name: "IPR Classification",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue OrganisationalReach = new TaggedValue(
            name: "Organisational Reach",
            type: TaggedValueTypes.Enum(OrganisationalReachValue.All));

        public static readonly TaggedValue ProjectStage = new TaggedValue(
            name: "Project Stage",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue Viewpoint = new TaggedValue(
            name: "Viewpoint",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue DueDate = new TaggedValue(
            name: "Due Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValue RevisionDate = new TaggedValue(
            name: "Revision Date",
            type: TaggedValueTypes.DateTime);
    }
}
