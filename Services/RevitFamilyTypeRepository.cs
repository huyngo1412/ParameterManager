using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParameterManager.Services
{
    public class RevitFamilyTypeRepository : IFamilyTypeRepository
    {
        private readonly Document _doc;
        private readonly List<ElementType> _allElementTypes;

        public RevitFamilyTypeRepository(Document doc)
        {
            _doc = doc;

            _allElementTypes = new FilteredElementCollector(_doc)
                .WhereElementIsElementType()
                .ToElements()
                .OfType<ElementType>()
                .Where(x => x.Category != null)
                .ToList();
        }

        public IList<CategoryItem> GetCategories()
        {
            return _allElementTypes
                .Where(x => x.Category != null)
                .GroupBy(x => x.Category.Id.Value)
                .Select(g => new CategoryItem
                {
                    CategoryId = g.Key,
                    Name = g.First().Category.Name
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IList<ElementType> GetTypesByCategoryIds(
            IEnumerable<long> categoryIds,
            bool activeViewOnly)
        {
            HashSet<long> categoryIdSet = new HashSet<long>(categoryIds);

            if (categoryIdSet.Count == 0)
                return new List<ElementType>();

            HashSet<long> visibleTypeIds = null;

            if (activeViewOnly)
            {
                visibleTypeIds = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Select(x => x.GetTypeId())
                    .Where(x => x != ElementId.InvalidElementId)
                    .Select(x => x.Value)
                    .ToHashSet();
            }

            return _allElementTypes
                .Where(x => x.Category != null)
                .Where(x => categoryIdSet.Contains(x.Category.Id.Value))
                .Where(x => !activeViewOnly || visibleTypeIds.Contains(x.Id.Value))
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => GetFamilyName(x))
                .ThenBy(x => x.Name)
                .ToList();
        }

        public IList<ElementType> GetTypesByIds(IEnumerable<long> typeIds)
        {
            HashSet<long> idSet = new HashSet<long>(typeIds);

            return _allElementTypes
                .Where(x => idSet.Contains(x.Id.Value))
                .ToList();
        }

        public IList<Element> GetInstancesByTypeIds(
            IEnumerable<long> typeIds,
            bool activeViewOnly)
        {
            HashSet<long> idSet = new HashSet<long>(typeIds);

            if (idSet.Count == 0)
                return new List<Element>();

            FilteredElementCollector collector = activeViewOnly
                ? new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                : new FilteredElementCollector(_doc);

            return collector
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x =>
                {
                    ElementId typeId = x.GetTypeId();

                    if (typeId == ElementId.InvalidElementId)
                        return false;

                    return idSet.Contains(typeId.Value);
                })
                .ToList();
        }

        private string GetFamilyName(ElementType type)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(type.FamilyName))
                    return type.FamilyName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"GetFamilyName - cannot read FamilyName of type {type.Id}: {ex.Message}");
            }

            return "System Family";
        }
    }
}