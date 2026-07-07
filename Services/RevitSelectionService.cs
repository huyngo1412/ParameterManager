using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ParameterManager.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace ParameterManager.Services
{
    public class RevitSelectionService : ISelectionService
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public RevitSelectionService(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void SelectElements(IList<Element> elements)
        {
            IList<ElementId> ids = elements
                .Where(x => x != null)
                .Select(x => x.Id)
                .ToList();

            _uidoc.Selection.SetElementIds(ids);
        }

        public void DeselectElements()
        {
            _uidoc.Selection.SetElementIds(new List<ElementId>());
        }

        public int HideElementsInActiveView(IList<Element> elements)
        {
            View view = _doc.ActiveView;

            IList<ElementId> ids = elements
                .Where(x => x != null)
                .Where(x => x.CanBeHidden(view))
                .Select(x => x.Id)
                .ToList();

            if (ids.Count == 0)
                return 0;

            using (Transaction tx = new Transaction(_doc, "Hide Selected Elements"))
            {
                tx.Start();
                view.HideElements(ids);
                tx.Commit();
            }

            return ids.Count;
        }

        public int UnhideElementsInActiveView(IList<Element> elements)
        {
            View view = _doc.ActiveView;

            IList<ElementId> ids = elements
                .Where(x => x != null)
                .Where(x => x.IsHidden(view))
                .Select(x => x.Id)
                .ToList();

            if (ids.Count == 0)
                return 0;

            using (Transaction tx = new Transaction(_doc, "Unhide Selected Elements"))
            {
                tx.Start();
                view.UnhideElements(ids);
                tx.Commit();
            }

            return ids.Count;
        }
    }
}