using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;
using System.Globalization;

namespace ParameterManager.Services.ParameterSetters
{
    public class IntegerParameterValueSetter : IParameterValueSetter
    {
        public bool CanHandle(StorageType storageType)
        {
            return storageType == StorageType.Integer;
        }

        public void SetValue(Parameter parameter, ParameterTreeNode node)
        {
            if (node.EditorKind == ParameterEditorKind.BooleanCheckBox)
            {
                if (!node.BooleanValue.HasValue)
                    return;

                parameter.Set(node.BooleanValue.Value ? 1 : 0);
                return;
            }

            string input = node.Value ?? string.Empty;

            try
            {
                if (parameter.SetValueString(input))
                    return;
            }
            catch
            {
            }

            parameter.Set(ParseIntegerValue(input));
        }

        private int ParseIntegerValue(string input)
        {
            string text = input.Trim().ToLowerInvariant();

            if (text == "true" ||
                text == "yes" ||
                text == "y" ||
                text == "1" ||
                text == "đúng" ||
                text == "có")
            {
                return 1;
            }

            if (text == "false" ||
                text == "no" ||
                text == "n" ||
                text == "0" ||
                text == "sai" ||
                text == "không")
            {
                return 0;
            }

            if (int.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out int currentResult))
            {
                return currentResult;
            }

            if (int.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out int invariantResult))
            {
                return invariantResult;
            }

            throw new Exception($"Cannot convert '{input}' to Integer.");
        }
    }
}