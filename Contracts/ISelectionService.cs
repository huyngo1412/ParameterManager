using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ParameterManager.Contracts
{
    public interface ISelectionService
    {
        void SelectElements(IList<Element> elements);

        void DeselectElements();

        int HideElementsInActiveView(IList<Element> elements);

        int UnhideElementsInActiveView(IList<Element> elements);
    }
}