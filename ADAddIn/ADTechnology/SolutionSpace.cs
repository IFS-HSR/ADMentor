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
    public static class SolutionSpace
    {
        public static readonly IEnumerable<ITaggedValue> SolutionTaggedValues = new[]{
            Common.RevisionDate
        };

        public class OptionState : Enumeration
        {
            public static readonly OptionState Eligible = new OptionState("Eligible", Color.LightSkyBlue);
            public static readonly OptionState Chosen = new OptionState("Chosen", Color.LightGreen);
            public static readonly OptionState Neglected = new OptionState("Neglected", Color.LightGray);

            public static readonly IEnumerable<OptionState> AllStates = new OptionState[] {
                Eligible, Chosen, Neglected
            };

            private OptionState(String name, Color color)
                : base(name)
            {
                Color = color;
            }

            public Color Color { get; private set; }
        }

        public static readonly TaggedValue OptionStateTag = new TaggedValue(
            name: "State",
            description: "Option State",
            type: TaggedValueTypes.Enum(values: OptionState.AllStates).WithDefaultValue(OptionState.Eligible));

        public static readonly ElementStereotype OptionOccurrence = new ElementStereotype(
            name: "adOptionOccurrence",
            displayName: "Option Occurrence",
            type: ElementType.Object,
            icon: new Icon("AdAddIn.ADTechnology.OptionOccurrence.bmp"),
            shapeScript: @"
                shape main{
	                h_align = ""center"";
	                v_align = ""center"";

                    " + GenerateFillColors("State", OptionState.AllStates.ToDictionary(ds => ds.Name, ds => ds.Color)) + @"

	                roundrect(0,0,100,100,30,30);

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
            width: 100,
            height: 70,
            taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
                OptionStateTag
            }));

        public class ProblemOccurrenceState : Enumeration
        {
            public static readonly ProblemOccurrenceState Open = new ProblemOccurrenceState("Open", Color.LightSalmon);
            public static readonly ProblemOccurrenceState PartiallySolved = new ProblemOccurrenceState("Partially Solved", Color.LightSkyBlue);
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

        public static readonly TaggedValue ProblemOccurrenceStateTag = new TaggedValue(
            name: "State",
            description: "Problem State",
            type: TaggedValueTypes.Enum(values: ProblemOccurrenceState.AllStates).WithDefaultValue(ProblemOccurrenceState.Open));

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
            width: 100,
            height: 70,
             taggedValues: SolutionTaggedValues.Concat(new TaggedValue[]{
                ProblemOccurrenceStateTag
             }));

        private static String GenerateFillColors(String tagName, IDictionary<String, Color> valueColors)
        {
            return (from p in valueColors
                    select String.Format(@"if(HasTag(""{0}"", ""{1}"")) {{ SetFillColor({2}, {3}, {4}); }}",
                        tagName, p.Key, p.Value.R, p.Value.G, p.Value.B)).Join("\n");
        }
    }
}
