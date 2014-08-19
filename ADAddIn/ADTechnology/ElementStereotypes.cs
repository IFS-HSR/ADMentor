using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ADTechnology
{
    public static class ElementStereotypes
    {
        public class OrganisationalReach : Enumeration
        {
            public static readonly OrganisationalReach Global = new OrganisationalReach("Global");
            public static readonly OrganisationalReach Organisation = new OrganisationalReach("Organisation");
            public static readonly OrganisationalReach Program = new OrganisationalReach("Program");
            public static readonly OrganisationalReach Project = new OrganisationalReach("Project");
            public static readonly OrganisationalReach Subproject = new OrganisationalReach("Subproject");
            public static readonly OrganisationalReach BusinessUnit = new OrganisationalReach("Business Unit");
            public static readonly OrganisationalReach Individual = new OrganisationalReach("Individual");

            public static readonly IEnumerable<OrganisationalReach> All = new[] {
                Global, Organisation, Program, Project, Subproject, BusinessUnit, Individual
            };

            private OrganisationalReach(String name) : base(name) { }
        }

        public static readonly ElementStereotype StakeholderRole = new ElementStereotype(
            name: "adStakeholderRole",
            displayName: "Stakeholder Role",
            type: ElementType.Actor);

        public static readonly IEnumerable<ITaggedValue> ProblemSpaceTaggedValues = new[]{
            new TaggedValue(name: "Stakeholder Role", type: TaggedValueTypes.Reference(StakeholderRole)),
            new TaggedValue(name: "Project Stage", type: TaggedValueTypes.String),
            new TaggedValue(name: "Viewpoint", type: TaggedValueTypes.String),
            new TaggedValue(name: "Organisational Reach", type: TaggedValueTypes.Enum(OrganisationalReach.All)),
            new TaggedValue(name: "Intellectual Property Rights", type: TaggedValueTypes.String)
        };

        public static readonly IEnumerable<ITaggedValue> SolutionTaggedValues = new[]{
            new TaggedValue(name: "Revision Date", type: TaggedValueTypes.DateTime)
        };

        public class DecisionState : Enumeration
        {
            /* Decision States according to "QUOSA;Kruchten;Building Up and Reasoning About Architectural Knowledge" */
            public static readonly DecisionState Idea = new DecisionState("Idea", Color.LightSkyBlue);
            public static readonly DecisionState Tentative = new DecisionState("Tentative", Color.LightBlue);
            public static readonly DecisionState Decided = new DecisionState("Decided", Color.LawnGreen);
            public static readonly DecisionState Approved = new DecisionState("Approved", Color.LightGreen);
            public static readonly DecisionState Challenged = new DecisionState("Challenged", Color.LightSalmon);
            public static readonly DecisionState Rejected = new DecisionState("Rejected", Color.LightGray);
            public static readonly DecisionState Obsolete = new DecisionState("Obsolete", Color.LightGray);

            public static readonly IEnumerable<DecisionState> AllStates = new DecisionState[] {
                Idea, Tentative, Decided, Approved, Challenged, Rejected, Obsolete
            };

            private DecisionState(String name, Color color)
                : base(name)
            {
                Color = color;
            }

            public Color Color { get; private set; }
        }

        public static readonly ElementStereotype Decision = new ElementStereotype(
            name: "adDecision",
            displayName: "Decision",
            type: ElementType.Object,
            icon: new Icon("AdAddIn.ADTechnology.Decision.bmp"),
            shapeScript: @"
                shape main{
	                h_align = ""center"";
	                v_align = ""center"";

                    " + GenerateFillColors("State", DecisionState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)) + @"

	                Rectangle(0,0,100,100);

                    println(""(#TAG:state#)"");

	                if(HasProperty(""name"", """")){
	                }else{
	                    Print(""#name#"");
                        if(HasProperty(""classifier.name"", """")){
                        } else {
                            Println("": "");
                        }
                    }
                    if(HasProperty(""classifier.name"", """")){
                    } else {
                        Println(""#classifier.name#"");
                    }
                }
            ",
            taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
                new TaggedValue(
                    name: "State",
                    description: "Decision State",
                    type: TaggedValueTypes.Enum(values: DecisionState.AllStates).WithDefaultValue(DecisionState.Idea))
            }));

        public class ProblemOccurrenceState : Enumeration
        {
            public static readonly ProblemOccurrenceState Open = new ProblemOccurrenceState("Open", Color.LightSalmon);
            public static readonly ProblemOccurrenceState Tentative = new ProblemOccurrenceState("Tentative", Color.LightBlue);
            public static readonly ProblemOccurrenceState Decided = new ProblemOccurrenceState("Decided", Color.LawnGreen);
            public static readonly ProblemOccurrenceState Approved = new ProblemOccurrenceState("Approved", Color.LightGreen);

            public static readonly IEnumerable<ProblemOccurrenceState> AllStates = new ProblemOccurrenceState[] {
                Open, Tentative, Decided, Approved
            };

            private ProblemOccurrenceState(String name, Color color)
                : base(name)
            {
                Color = color;
            }

            public Color Color { get; private set; }
        }

        public static readonly ElementStereotype ProblemOccurrence = new ElementStereotype(
            name: "adProblemOccurrence",
            displayName: "Problem Occurrence",
            type: ElementType.Object,
            icon: new Icon("AdAddIn.ADTechnology.ProblemOccurrence.bmp"),
            shapeScript: @"
                shape main{
	                h_align = ""center"";
	                v_align = ""center"";

                    " + GenerateFillColors("State", ProblemOccurrenceState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)) + @"

	                StartPath();
	                MoveTo(50,0);
	                LineTo(100,50);
	                LineTo(50,100);
	                LineTo(0,50);
	                EndPath();
	                FillAndStrokePath();

                    println(""(#TAG:state#)"");

	                if(HasProperty(""name"", """")){
	                }else{
	                    Print(""#name#"");
                        if(HasProperty(""classifier.name"", """")){
                        } else {
                            Println("": "");
                        }
                    }
                    if(HasProperty(""classifier.name"", """")){
                    } else {
                        Println(""#classifier.name#"");
                    }
                }
            ",
         taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
            new TaggedValue(
                name: "State",
                description: "Problem State",
                type: TaggedValueTypes.Enum(values: ProblemOccurrenceState.AllStates).WithDefaultValue(ProblemOccurrenceState.Open))
         }));

        public static readonly ElementStereotype Problem = new ElementStereotype(
            name: "adProblem",
            displayName: "Problem",
            type: ElementType.Class,
            instanceType: ProblemOccurrence,
            icon: new Icon("AdAddIn.ADTechnology.Problem.bmp"),
            shapeScript: @"
                shape main{
	                h_align = ""center"";
	                v_align = ""center"";

	                StartPath();
	                MoveTo(50,0);
	                LineTo(100,50);
	                LineTo(50,100);
	                LineTo(0,50);
	                EndPath();
	                FillAndStrokePath();
	
	                Println(""#NAME#"");
                }
            ",
            backgroundColor: Color.LightSkyBlue,
            width: 100,
            height: 70,
            taggedValues: ProblemSpaceTaggedValues);

        public static readonly ElementStereotype Option = new ElementStereotype(
            name: "adOption",
            displayName: "Option",
            type: ElementType.Class,
            instanceType: Decision,
            icon: new Icon("AdAddIn.ADTechnology.Option.bmp"),
            backgroundColor: Color.LightYellow,
            width: 100,
            height: 70,
            taggedValues: ProblemSpaceTaggedValues);

        private static String GenerateFillColors(String tagName, IDictionary<String, Color> valueColors)
        {
            return (from p in valueColors
                    select String.Format(@"if(HasTag(""{0}"", ""{1}"")) {{ SetFillColor({2}, {3}, {4}); }}",
                        tagName, p.Key, p.Value.R, p.Value.G, p.Value.B)).Join("\n");
        }
    }
}
