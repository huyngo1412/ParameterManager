using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;

namespace ParameterManager.Services.ParameterSetters
{
    public class ElementIdParameterValueSetter : IParameterValueSetter
    {
        public bool CanHandle(StorageType storageType)
        {
            return storageType == StorageType.ElementId;
        }

        public void SetValue(Parameter parameter, ParameterTreeNode node)
        {
            if (node.EditorKind == ParameterEditorKind.ElementCombo ||
                node.EditorKind == ParameterEditorKind.ImagePicker)
            {
                if (!node.ElementIdValue.HasValue)
                    return;

                parameter.Set(new ElementId(node.ElementIdValue.Value));
                return;
            }

            string input = node.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                parameter.Set(ElementId.InvalidElementId);
                return;
            }

            string rawValue = input.Trim();

            if (rawValue.Contains("|"))
            {
                rawValue = rawValue.Split('|')[0].Trim();
            }

            if (!long.TryParse(rawValue, out long idValue))
            {
                throw new Exception(
                    $"ElementId parameter requires an ElementId number. Current value: '{input}'.");
            }

            parameter.Set(new ElementId(idValue));
        }
    }
}