using Autodesk.Revit.DB;
using ParameterManager.Models;
using System.Collections.Generic;

namespace ParameterManager.Contracts
{
    public interface IFamilyTypeRepository
    {
        IList<CategoryItem> GetCategories();

        IList<ElementType> GetTypesByCategoryIds(
            IEnumerable<long> categoryIds,
            bool activeViewOnly);

        IList<ElementType> GetTypesByIds(IEnumerable<long> typeIds);

        IList<Element> GetInstancesByTypeIds(
            IEnumerable<long> typeIds,
            bool activeViewOnly);
    }
}