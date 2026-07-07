using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace ParameterManager.Services
{
    public class RevitElementOptionRepository : IElementOptionRepository
    {
        private readonly Document _doc;

        public RevitElementOptionRepository(Document doc)
        {
            _doc = doc;
        }

        public IList<ElementOptionItem> GetOptions(string optionKind)
        {
            List<ElementOptionItem> result = new List<ElementOptionItem>
            {
                new ElementOptionItem
                {
                    ElementIdValue = null,
                    Name = ParameterConstants.VariesText,
                    IsPlaceholder = true
                },
                new ElementOptionItem
                {
                    ElementIdValue = ElementId.InvalidElementId.Value,
                    Name = "<None>",
                    IsPlaceholder = false
                }
            };

            IEnumerable<Element> elements = Enumerable.Empty<Element>();

            switch (optionKind)
            {
                case ElementOptionKind.Material:
                    elements = new FilteredElementCollector(_doc)
                        .OfClass(typeof(Material))
                        .ToElements();
                    break;

                case ElementOptionKind.Level:
                    elements = new FilteredElementCollector(_doc)
                        .OfClass(typeof(Level))
                        .ToElements();
                    break;

                case ElementOptionKind.Phase:
                    List<Element> phases = new List<Element>();

                    foreach (Phase phase in _doc.Phases)
                    {
                        phases.Add(phase);
                    }

                    elements = phases;
                    break;

                case ElementOptionKind.RebarCover:
                    elements = new FilteredElementCollector(_doc)
                        .OfClass(typeof(RebarCoverType))
                        .ToElements();
                    break;

                case ElementOptionKind.Image:
                    elements = new FilteredElementCollector(_doc)
                        .OfClass(typeof(ImageType))
                        .ToElements();
                    break;
            }

            result.AddRange(
                elements
                    .Where(x => x != null)
                    .OrderBy(x => x.Name)
                    .Select(x => new ElementOptionItem
                    {
                        ElementIdValue = x.Id.Value,
                        Name = x.Name,
                        IsPlaceholder = false
                    }));

            return result;
        }
    }
}