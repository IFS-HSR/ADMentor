using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.CopyMetadata
{
    public class UpdateProblemOccurrenceStateCommand : ICommand<Func<EA.Element>, Unit>
    {
        private readonly ElementRepository ElementRepo;
        private readonly DiagramRepository DiagramRepo;

        public UpdateProblemOccurrenceStateCommand(ElementRepository elementRepo, DiagramRepository diagramRepo)
        {
            ElementRepo = elementRepo;
            DiagramRepo = diagramRepo;
        }

        public Unit Execute(Func<EA.Element> getElement)
        {
            var element = getElement();

            if (element.Is(Solution.OptionOccurrence))
            {
                var dependencyGraph = DependencyGraph.Create(ElementRepo, element, TraverseAlternativeConnectors);
                dependencyGraph.NodeLabels.FirstOption(e => e.Is(Solution.ProblemOccurrence)).Do(problemOccurrence =>
                {
                    var alternatives = dependencyGraph.NodeLabels.Where(
                        e => e.Is(Solution.OptionOccurrence));

                    var noAlternatives = alternatives.Count();
                    var noCandidates = alternatives.Count(e => e.Get(Solution.OptionStateTag).Equals(Solution.OptionState.Candidate.Name.AsOption()));
                    var noChosen = alternatives.Count(e => e.Get(Solution.OptionStateTag).Equals(Solution.OptionState.Chosen.Name.AsOption()));
                    var noNeglected = alternatives.Count(e => e.Get(Solution.OptionStateTag).Equals(Solution.OptionState.Neglected.Name.AsOption()));

                    if (noAlternatives == noCandidates)
                    {
                        problemOccurrence.Set(Solution.ProblemOccurrenceStateTag, Solution.ProblemOccurrenceState.Open.Name);
                    }
                    else if (noCandidates > 0 && noCandidates < noAlternatives)
                    {
                        problemOccurrence.Set(Solution.ProblemOccurrenceStateTag, Solution.ProblemOccurrenceState.PartiallySolved.Name);
                    }
                    else if (noChosen > 0 && noCandidates == 0)
                    {
                        problemOccurrence.Set(Solution.ProblemOccurrenceStateTag, Solution.ProblemOccurrenceState.Solved.Name);
                    }
                    else
                    {
                        problemOccurrence.Set(Solution.ProblemOccurrenceStateTag, Solution.ProblemOccurrenceState.NotApplicable.Name);
                    }

                    ElementRepo.ElementChanged(problemOccurrence);
                });
            }

            return Unit.Instance;
        }

        private bool TraverseAlternativeConnectors(EA.Element from, EA.Connector via, EA.Element to)
        {
            return via.Is(ConnectorStereotypes.HasAlternative);
        }

        public bool CanExecute(Func<EA.Element> _)
        {
            return true;
        }
    }
}
