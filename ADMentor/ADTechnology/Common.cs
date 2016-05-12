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
        public sealed class OrganisationalReachValue : Enumeration
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

        public static readonly TaggedValueDefinition IntellectualPropertyRights = new TaggedValueDefinition(
            name: "Intellectual Property Rights",
            type: TaggedValueTypes.String.WithDefaultValue("Unrestricted"));

        public static readonly TaggedValueDefinition OrganisationalReach = new TaggedValueDefinition(
            name: "Organisational Reach",
            type: TaggedValueTypes.Enum(OrganisationalReachValue.All).WithDefaultValue(OrganisationalReachValue.Project));

        public static readonly TaggedValueDefinition ProjectStage = new TaggedValueDefinition(
            name: "Project Stage",
            type: TaggedValueTypes.String);

        public static readonly TaggedValueDefinition Viewpoint = new TaggedValueDefinition(
            name: "Viewpoint",
            type: TaggedValueTypes.String);

        public static readonly TaggedValueDefinition KnowledgeProvenance = new TaggedValueDefinition(
            name: "Knowledge Provenance",
            type: TaggedValueTypes.String);

        public static readonly TaggedValueDefinition DueDate = new TaggedValueDefinition(
            name: "Due Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValueDefinition RevisionDate = new TaggedValueDefinition(
            name: "Revision Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValueDefinition DecisionDate = new TaggedValueDefinition(
            name: "Decision Date",
            type: TaggedValueTypes.DateTime);

        public static readonly TaggedValueDefinition OwnerRole = new TaggedValueDefinition(
            name: "Owner Role",
            type: TaggedValueTypes.String.WithDefaultValue("Any"));

        public static readonly TaggedValueDefinition StakeholderRoles = new TaggedValueDefinition(
            name: "Stakeholder Roles",
            type: TaggedValueTypes.String.WithDefaultValue("All"));

        public static readonly TaggedValueDefinition RefinementLevel = new TaggedValueDefinition(
            name: "Refinement Level",
            type: TaggedValueTypes.String);

        public static readonly TaggedValueDefinition RemoteId = new TaggedValueDefinition(
            name: "xRemoteId",
            type: TaggedValueTypes.Const(""));
    }
}
