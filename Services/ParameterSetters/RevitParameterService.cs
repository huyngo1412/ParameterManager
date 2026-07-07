using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ParameterManager.Services
{
    public class RevitParameterService : IParameterService
    {
        private readonly Document _doc;
        private readonly IParameterValueSetterFactory _setterFactory;

        public RevitParameterService(
            Document doc,
            IParameterValueSetterFactory setterFactory)
        {
            _doc = doc;
            _setterFactory = setterFactory;
        }

        public IList<ParameterDescriptor> GetCommonEditableParameters(
            IList<ElementType> selectedTypes,
            IList<Element> selectedInstances)
        {
            List<ParameterDescriptor> result = new List<ParameterDescriptor>();

            if (selectedTypes != null && selectedTypes.Count > 0)
            {
                result.AddRange(
                    GetCommonEditableParametersFromElements(
                        selectedTypes.Cast<Element>().ToList(),
                        ParameterScope.Type));
            }

            if (selectedInstances != null && selectedInstances.Count > 0)
            {
                result.AddRange(
                    GetCommonEditableParametersFromElements(
                        selectedInstances,
                        ParameterScope.Instance));
            }

            return result
                .OrderBy(x => x.GroupName)
                .ThenBy(x => x.ParameterName)
                .ToList();
        }

        public AssignResult AssignParameters(
            IList<ElementType> selectedTypes,
            IList<Element> selectedInstances,
            IList<ParameterTreeNode> editedParameters)
        {
            AssignResult result = new AssignResult();

            if (editedParameters == null || editedParameters.Count == 0)
            {
                result.Errors.Add("No parameter has been edited.");
                return result;
            }

            List<ParameterTreeNode> typeParameters = editedParameters
                .Where(x => x.Scope == ParameterScope.Type)
                .ToList();

            List<ParameterTreeNode> instanceParameters = editedParameters
                .Where(x => x.Scope == ParameterScope.Instance)
                .ToList();

            using (Transaction tx = new Transaction(_doc, "Assign Editable Parameters"))
            {
                tx.Start();

                AssignToElements(
                    selectedTypes?.Cast<Element>().ToList(),
                    typeParameters,
                    result);

                AssignToElements(
                    selectedInstances,
                    instanceParameters,
                    result);

                if (result.SuccessCount > 0)
                    tx.Commit();
                else
                    tx.RollBack();
            }

            return result;
        }

        private IList<ParameterDescriptor> GetCommonEditableParametersFromElements(
            IList<Element> elements,
            ParameterScope scope)
        {
            if (elements == null || elements.Count == 0)
                return new List<ParameterDescriptor>();

            List<Dictionary<long, Parameter>> parameterMaps = elements
                .Select(GetEditableParameters)
                .ToList();

            if (parameterMaps.Count == 0 || parameterMaps.Any(x => x.Count == 0))
                return new List<ParameterDescriptor>();

            IEnumerable<long> commonParameterIds = parameterMaps
                .Select(map => (IEnumerable<long>)map.Keys)
                .Aggregate((a, b) => a.Intersect(b));

            List<ParameterDescriptor> descriptors = new List<ParameterDescriptor>();

            foreach (long parameterId in commonParameterIds)
            {
                Parameter representativeParameter = parameterMaps[0][parameterId];

                ValueInfo valueInfo =
                    GetCommonValueInfo(parameterMaps, parameterId, representativeParameter);

                string optionKind = GetElementOptionKind(representativeParameter);

                descriptors.Add(new ParameterDescriptor
                {
                    ParameterId = parameterId,
                    ParameterName = representativeParameter.Definition.Name,
                    GroupName = GetParameterGroupName(representativeParameter),
                    Source = GetParameterSource(representativeParameter),
                    StorageType = representativeParameter.StorageType.ToString(),
                    DisplayValue = valueInfo.DisplayValue,
                    CommonElementIdValue = valueInfo.CommonElementIdValue,
                    CommonBooleanValue = valueInfo.CommonBooleanValue,
                    Scope = scope,
                    EditorKind = GetEditorKind(representativeParameter),
                    ElementOptionKind = optionKind
                });
            }

            return descriptors;
        }

        private void AssignToElements(
            IList<Element> elements,
            IList<ParameterTreeNode> editedParameters,
            AssignResult result)
        {
            if (elements == null || elements.Count == 0)
                return;

            if (editedParameters == null || editedParameters.Count == 0)
                return;

            foreach (Element element in elements)
            {
                foreach (ParameterTreeNode editedParameter in editedParameters)
                {
                    Parameter parameter = GetParameterById(element, editedParameter.ParameterId);

                    if (parameter == null)
                    {
                        result.Errors.Add(
                            $"{GetSafeElementName(element)} - Parameter not found: {editedParameter.ParameterName}");
                        continue;
                    }

                    if (parameter.IsReadOnly)
                    {
                        result.Errors.Add(
                            $"{GetSafeElementName(element)} - Parameter is read-only: {editedParameter.ParameterName}");
                        continue;
                    }

                    try
                    {
                        IParameterValueSetter setter =
                            _setterFactory.GetSetter(parameter.StorageType);

                        setter.SetValue(parameter, editedParameter);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(
                            $"{GetSafeElementName(element)} - {editedParameter.ParameterName}: {ex.Message}");
                    }
                }
            }
        }

        private Dictionary<long, Parameter> GetEditableParameters(Element element)
        {
            Dictionary<long, Parameter> result = new Dictionary<long, Parameter>();

            foreach (Parameter parameter in element.Parameters)
            {
                if (!ShouldShowParameter(parameter))
                    continue;

                long id = parameter.Id.Value;

                if (!result.ContainsKey(id))
                    result.Add(id, parameter);
            }

            return result;
        }

        private bool ShouldShowParameter(Parameter parameter)
        {
            if (parameter == null || parameter.Definition == null)
                return false;

            if (parameter.IsReadOnly)
                return false;

            if (parameter.StorageType == StorageType.None)
                return false;

            return true;
        }

        private string GetParameterSource(Parameter parameter)
        {
            if (parameter == null)
                return "Unknown Parameter";

            if (parameter.IsShared)
                return "Shared Parameter";

            ParameterElement parameterElement =
                _doc.GetElement(parameter.Id) as ParameterElement;

            if (parameterElement != null)
                return "Project Parameter";

            if (parameter.Id.Value < 0)
                return "Built-in Parameter";

            return "Family/Revit Parameter";
        }

        private ParameterEditorKind GetEditorKind(Parameter parameter)
        {
            if (parameter.StorageType == StorageType.Integer &&
                IsBooleanParameter(parameter))
            {
                return ParameterEditorKind.BooleanCheckBox;
            }

            if (parameter.StorageType == StorageType.ElementId)
            {
                string optionKind = GetElementOptionKind(parameter);

                if (optionKind == ElementOptionKind.Image)
                    return ParameterEditorKind.ImagePicker;

                if (optionKind != ElementOptionKind.Unknown)
                    return ParameterEditorKind.ElementCombo;
            }

            return ParameterEditorKind.Text;
        }

        private bool IsBooleanParameter(Parameter parameter)
        {
            if (parameter == null || parameter.StorageType != StorageType.Integer)
                return false;

            try
            {
                ForgeTypeId dataType = parameter.Definition.GetDataType();

                if (dataType != null && dataType.TypeId != null)
                {
                    string typeId = dataType.TypeId.ToLowerInvariant();

                    if (typeId.Contains("boolean") ||
                        typeId.Contains("yesno") ||
                        typeId.Contains("yes.no"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            string valueString = parameter.AsValueString();

            if (!string.IsNullOrWhiteSpace(valueString))
            {
                string v = valueString.Trim().ToLowerInvariant();

                if (v == "yes" || v == "no" ||
                    v == "true" || v == "false" ||
                    v == "oui" || v == "non")
                {
                    return true;
                }
            }

            return false;
        }

        private string GetElementOptionKind(Parameter parameter)
        {
            if (parameter == null || parameter.StorageType != StorageType.ElementId)
                return ElementOptionKind.Unknown;

            string name = parameter.Definition.Name.ToLowerInvariant();

            if (IsMaterialParameter(parameter))
                return ElementOptionKind.Material;

            if (name.Contains("level") || name.Contains("niveau"))
                return ElementOptionKind.Level;

            if (name.Contains("phase"))
                return ElementOptionKind.Phase;

            if (name.Contains("rebar cover") ||
                name.Contains("cover") ||
                name.Contains("enrobage"))
                return ElementOptionKind.RebarCover;

            if (name == "image" || name.Contains("image"))
                return ElementOptionKind.Image;

            try
            {
                ElementId id = parameter.AsElementId();

                if (id != ElementId.InvalidElementId)
                {
                    Element element = _doc.GetElement(id);

                    if (element is Material)
                        return ElementOptionKind.Material;

                    if (element is Level)
                        return ElementOptionKind.Level;

                    if (element is Phase)
                        return ElementOptionKind.Phase;

                    if (element is RebarCoverType)
                        return ElementOptionKind.RebarCover;

                    if (element is ImageType)
                        return ElementOptionKind.Image;
                }
            }
            catch
            {
            }

            return ElementOptionKind.Unknown;
        }

        private bool IsMaterialParameter(Parameter parameter)
        {
            if (parameter == null)
                return false;

            if (parameter.StorageType != StorageType.ElementId)
                return false;

            try
            {
                ForgeTypeId dataType = parameter.Definition.GetDataType();

                if (dataType != null &&
                    dataType.TypeId != null &&
                    dataType.TypeId.ToLowerInvariant().Contains("material"))
                {
                    return true;
                }
            }
            catch
            {
            }

            string name = parameter.Definition.Name.ToLowerInvariant();

            return name.Contains("material") ||
                   name.Contains("matériau") ||
                   name.Contains("materiau");
        }

        private Parameter GetParameterById(Element element, long parameterId)
        {
            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Id.Value == parameterId)
                    return parameter;
            }

            return null;
        }

        private string GetParameterGroupName(Parameter parameter)
        {
            if (parameter == null || parameter.Definition == null)
                return "Other";

            try
            {
                ForgeTypeId groupTypeId = parameter.Definition.GetGroupTypeId();

                if (groupTypeId == null || string.IsNullOrWhiteSpace(groupTypeId.TypeId))
                    return "Other";

                return LabelUtils.GetLabelForGroup(groupTypeId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"GetParameterGroupName - cannot get group of '{parameter.Definition.Name}': {ex.Message}");

                return "Other";
            }
        }

        private ValueInfo GetCommonValueInfo(
            List<Dictionary<long, Parameter>> parameterMaps,
            long parameterId,
            Parameter representativeParameter)
        {
            List<string> values = new List<string>(parameterMaps.Count);
            List<long?> elementIds = new List<long?>(parameterMaps.Count);
            List<bool?> booleanValues = new List<bool?>(parameterMaps.Count);

            bool isBoolean =
                representativeParameter.StorageType == StorageType.Integer &&
                IsBooleanParameter(representativeParameter);

            foreach (Dictionary<long, Parameter> map in parameterMaps)
            {
                if (map.TryGetValue(parameterId, out Parameter parameter))
                {
                    values.Add(GetParameterDisplayValue(parameter));
                    elementIds.Add(GetElementIdValue(parameter));

                    if (isBoolean)
                        booleanValues.Add(parameter.AsInteger() != 0);
                }
            }

            if (values.Count == 0)
            {
                return new ValueInfo
                {
                    DisplayValue = string.Empty,
                    CommonElementIdValue = null,
                    CommonBooleanValue = null
                };
            }

            string firstValue = values[0];
            bool allSameValue = values.All(x => x == firstValue);

            long? firstId = elementIds[0];
            bool allSameId = elementIds.All(x => x == firstId);

            bool? commonBoolean = null;

            if (isBoolean && booleanValues.Count > 0)
            {
                bool? firstBool = booleanValues[0];
                bool allSameBool = booleanValues.All(x => x == firstBool);

                if (allSameBool)
                    commonBoolean = firstBool;
            }

            return new ValueInfo
            {
                DisplayValue = allSameValue ? firstValue : ParameterConstants.VariesText,
                CommonElementIdValue = allSameId ? firstId : null,
                CommonBooleanValue = commonBoolean
            };
        }

        private long? GetElementIdValue(Parameter parameter)
        {
            if (parameter == null)
                return null;

            if (parameter.StorageType != StorageType.ElementId)
                return null;

            try
            {
                return parameter.AsElementId().Value;
            }
            catch
            {
                return null;
            }
        }

        private string GetParameterDisplayValue(Parameter parameter)
        {
            if (parameter == null)
                return string.Empty;

            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString() ?? string.Empty;

                case StorageType.Integer:
                    if (IsBooleanParameter(parameter))
                        return parameter.AsInteger() != 0 ? "Yes" : "No";

                    return parameter.AsValueString()
                           ?? parameter.AsInteger().ToString();

                case StorageType.Double:
                    return parameter.AsValueString()
                           ?? parameter.AsDouble().ToString(CultureInfo.InvariantCulture);

                case StorageType.ElementId:
                    ElementId id = parameter.AsElementId();

                    if (id == ElementId.InvalidElementId)
                        return "<None>";

                    Element element = _doc.GetElement(id);

                    if (element != null && !string.IsNullOrWhiteSpace(element.Name))
                        return element.Name;

                    return id.Value.ToString();

                default:
                    return string.Empty;
            }
        }

        private string GetSafeElementName(Element element)
        {
            if (element == null)
                return "Unknown Element";

            try
            {
                if (!string.IsNullOrWhiteSpace(element.Name))
                    return element.Name;
            }
            catch
            {
            }

            return element.Id.Value.ToString();
        }

        private class ValueInfo
        {
            public string DisplayValue { get; set; }

            public long? CommonElementIdValue { get; set; }

            public bool? CommonBooleanValue { get; set; }
        }
    }
}