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
    public static class Solution
    {
        public static readonly IEnumerable<ITaggedValue> SolutionTaggedValues = new[]{
            Common.RevisionDate
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

        public static readonly ElementStereotype OptionOccurrence = new ElementStereotype(
            name: "adOptionOccurrence",
            displayName: "Option Occurrence",
            type: ElementType.Object,
            icon: new Icon("AdAddIn.ADTechnology.OptionOccurrence.bmp"),
            shapeScript: @"
                shape main{
	                h_align = ""center"";
	                v_align = ""center"";

                    " + GenerateFillColors("State", DecisionState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)) + @"

	                Rectangle(0,0,100,100);

                    println(""(#TAG:state#)"");

	                if(HasProperty(""name"", """")){
                        if(HasProperty(""classifier.name"", """")){
                        } else {
                            Println(""#classifier.name#"");
                        }
	                }else{
	                    Print(""#name#"");
                    }
                }
            ",
            taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
                new TaggedValue(
                    name: "State",
                    description: "Option State",
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
                        if(HasProperty(""classifier.name"", """")){
                        } else {
                            Println(""#classifier.name#"");
                        }
	                }else{
	                    Print(""#name#"");
                    }
                }
            ",
             taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
                new TaggedValue(
                    name: "State",
                    description: "Problem State",
                    type: TaggedValueTypes.Enum(values: ProblemOccurrenceState.AllStates).WithDefaultValue(ProblemOccurrenceState.Open))
             }));

        private static String GenerateFillColors(String tagName, IDictionary<String, Color> valueColors)
        {
            return (from p in valueColors
                    select String.Format(@"if(HasTag(""{0}"", ""{1}"")) {{ SetFillColor({2}, {3}, {4}); }}",
                        tagName, p.Key, p.Value.R, p.Value.G, p.Value.B)).Join("\n");
        }
    }
}
