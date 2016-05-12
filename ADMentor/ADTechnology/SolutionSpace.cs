using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.ADTechnology
{
    public static class SolutionSpace
    {
        public sealed class OptionState : Enumeration
        {
            public static readonly OptionState Eligible = new OptionState("Eligible", Color.LightYellow);
            public static readonly OptionState Tentative = new OptionState("Tentative", Color.Khaki);
            public static readonly OptionState Chosen = new OptionState("Chosen", Color.LightGreen);
            public static readonly OptionState Neglected = new OptionState("Neglected", Color.LightGray);
            public static readonly OptionState Challenged = new OptionState("Challenged", Color.Khaki);

            public static readonly IEnumerable<OptionState> AllStates = new OptionState[] {
                Eligible, Tentative, Chosen, Neglected, Challenged
            };

            private OptionState(String name, Color color)
                : base(name)
            {
                Color = color;
            }

            public Color Color { get; private set; }
        }

        public static readonly TaggedValueDefinition OptionStateTag = new TaggedValueDefinition(
            name: "Option State",
            description: "Option State",
            type: TaggedValueTypes.Enum(values: OptionState.AllStates).WithDefaultValue(OptionState.Eligible));

        public static readonly ElementStereotype OptionOccurrence = new ElementStereotype(
            name: "adOptionOccurrence",
            displayName: "Option Occurrence",
            type: ElementType.Object,
            icon: new Icon("ADMentor.ADTechnology.OptionOccurrence.bmp"),
            shapeScript: String.Format(@"
                shape main{{
	                h_align = ""center"";
	                v_align = ""center"";

                    {0}

	                roundrect(0,0,100,100,30,30);

                    println(""(#TAG:{1}#)"");

	                if(HasProperty(""name"", """")){{
                        if(HasProperty(""classifier.name"", """")){{
                        }} else {{
                            Println(""#classifier.name#"");
                        }}
	                }}else{{
	                    Print(""#name#"");
                    }}
                }}", 
                GenerateFillColors(OptionStateTag.Name, OptionState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)), 
                OptionStateTag.Name),
            width: 100,
            height: 70,
            taggedValues: new TaggedValueDefinition[]{
                OptionStateTag
            });

        public sealed class ProblemOccurrenceState : Enumeration
        {
            public static readonly ProblemOccurrenceState Open = new ProblemOccurrenceState("Open", Color.LightSalmon);
            public static readonly ProblemOccurrenceState PartiallySolved = new ProblemOccurrenceState("Partially Solved", Color.SandyBrown);
            public static readonly ProblemOccurrenceState Solved = new ProblemOccurrenceState("Solved", Color.LightGreen);
            public static readonly ProblemOccurrenceState NotApplicable = new ProblemOccurrenceState("Not Applicable", Color.LightGray);

            public static readonly IEnumerable<ProblemOccurrenceState> AllStates = new ProblemOccurrenceState[] {
                Open, PartiallySolved, Solved, NotApplicable
            };

            private ProblemOccurrenceState(String name, Color color)
                : base(name)
            {
                Color = color;
            }

            public Color Color { get; private set; }
        }

        public static readonly TaggedValueDefinition ProblemOccurrenceStateTag = new TaggedValueDefinition(
            name: "Problem State",
            description: "Problem State",
            type: TaggedValueTypes.Enum(values: ProblemOccurrenceState.AllStates).WithDefaultValue(ProblemOccurrenceState.Open));

        public static readonly ElementStereotype ProblemOccurrence = new ElementStereotype(
            name: "adProblemOccurrence",
            displayName: "Problem Occurrence",
            type: ElementType.Object,
            icon: new Icon("ADMentor.ADTechnology.ProblemOccurrence.bmp"),
            shapeScript: String.Format(@"
                shape main{{
	                h_align = ""center"";
	                v_align = ""center"";

                    {0}

	                StartPath();
	                MoveTo(50,0);
	                LineTo(100,50);
	                LineTo(50,100);
	                LineTo(0,50);
	                EndPath();
	                FillAndStrokePath();

                    println(""(#TAG:{1}#)"");

	                if(HasProperty(""name"", """")){{
                        if(HasProperty(""classifier.name"", """")){{
                        }} else {{
                            Println(""#classifier.name#"");
                        }}
	                }}else{{
	                    Print(""#name#"");
                    }}
                }}",
                GenerateFillColors(ProblemOccurrenceStateTag.Name, ProblemOccurrenceState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)),
                ProblemOccurrenceStateTag.Name),
            width: 100,
            height: 70,
            taggedValues: new[]{
                ProblemOccurrenceStateTag,
                Common.DueDate,
                Common.RevisionDate,
                Common.DecisionDate
            });

        private static String GenerateFillColors(String tagName, IDictionary<String, Color> valueColors)
        {
            return (from p in valueColors
                    select String.Format(@"if(HasTag(""{0}"", ""{1}"")) {{ SetFillColor({2}, {3}, {4}); }}",
                        tagName, p.Key, p.Value.R, p.Value.G, p.Value.B)).Join("\n");
        }

        public static readonly PackageStereotype SolutionSpacePackage = new PackageStereotype(
            name: "adSolutionSpace",
            displayName: "Solution Space Package",
            taggedValues: new[]{
                Common.IntellectualPropertyRights
            });
    }
}
