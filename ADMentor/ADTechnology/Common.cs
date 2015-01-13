using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.ADTechnology
{
    public static class Common
    {
        public class OrganisationalReachValue : Enumeration
        {
            public static readonly OrganisationalReachValue Global = new OrganisationalReachValue("Global");
            public static readonly OrganisationalReachValue Organisation = new OrganisationalReachValue("Organisation");
            public static readonly OrganisationalReachValue Division = new OrganisationalReachValue("Division");
            public static readonly OrganisationalReachValue BusinessUnit = new OrganisationalReachValue("Business Unit");
            public static readonly OrganisationalReachValue Program = new OrganisationalReachValue("Program");
            public static readonly OrganisationalReachValue Project = new OrganisationalReachValue("Project");
            public static readonly OrganisationalReachValue Subproject = new OrganisationalReachValue("Subproject");
            public static readonly OrganisationalReachValue Individual = new OrganisationalReachValue("Individual");

            public static readonly IEnumerable<OrganisationalReachValue> All = new[] {
                Global, Organisation, Division, BusinessUnit, Program, Project, Subproject, Individual
            };

            private OrganisationalReachValue(String name) : base(name) { }
        }

        public static readonly TaggedValue IntellectualPropertyRights = new TaggedValue(
            name: "Intellectual Property Rights",
            type: TaggedValueTypes.String.WithDefaultValue("Unrestricted"));

        public static readonly TaggedValue OrganisationalReach = new TaggedValue(
            name: "Organisational Reach",
            type: TaggedValueTypes.Enum(OrganisationalReachValue.All).WithDefaultValue(OrganisationalReachValue.Project));

        public static readonly TaggedValue ProjectStage = new TaggedValue(
            name: "Project Stage",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue Viewpoint = new TaggedValue(
            name: "Viewpoint",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue KnowledgeProvenance = new TaggedValue(
            name: "Knowledge Provenance",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue DueDate = new TaggedValue(
            name: "Due Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValue RevisionDate = new TaggedValue(
            name: "Revision Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValue DecisionDate = new TaggedValue(
            name: "Decision Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValue OwnerRole = new TaggedValue(
            name: "Owner Role",
            type: TaggedValueTypes.String.WithDefaultValue("Any"));

        public static readonly TaggedValue StakeholderRoles = new TaggedValue(
            name: "Stakeholder Roles",
            type: TaggedValueTypes.String.WithDefaultValue("Any"));

        public static readonly TaggedValue RefinementLevel = new TaggedValue(
            name: "Refinement Level",
            type: TaggedValueTypes.String);

        public static readonly TaggedValue RemoteId = new TaggedValue(
            name: "xRemoteId",
            type: TaggedValueTypes.Const(""));
    }
}
