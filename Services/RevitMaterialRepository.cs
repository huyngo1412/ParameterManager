using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace ParameterManager.Services
{
    public class RevitMaterialRepository : IMaterialRepository
    {
        private readonly Document _doc;

        public RevitMaterialRepository(Document doc)
        {
            _doc = doc;
        }

        public IList<MaterialItem> GetMaterials()
        {
            List<MaterialItem> materials = new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .ToElements()
                .OfType<Material>()
                .Select(x => new MaterialItem
                {
                    MaterialId = x.Id.Value,
                    Name = x.Name,
                    IsPlaceholder = false
                })
                .OrderBy(x => x.Name)
                .ToList();

            materials.Insert(0, new MaterialItem
            {
                MaterialId = null,
                Name = ParameterConstants.VariesText,
                IsPlaceholder = true
            });

            materials.Insert(1, new MaterialItem
            {
                MaterialId = ElementId.InvalidElementId.Value,
                Name = "<None>",
                IsPlaceholder = false
            });

            return materials;
        }
    }
}
