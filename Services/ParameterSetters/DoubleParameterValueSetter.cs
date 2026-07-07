using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;
using System;
using System.Globalization;

namespace ParameterManager.Services.ParameterSetters
{
    public class DoubleParameterValueSetter : IParameterValueSetter
    {
        public bool CanHandle(StorageType storageType)
        {
            return storageType == StorageType.Double;
        }

        public void SetValue(Parameter parameter, ParameterTreeNode node)
        {
            string input = node.Value ?? string.Empty;

            try
            {
                if (parameter.SetValueString(input))
                    return;
            }
            catch
            {
            }

            parameter.Set(ParseDoubleValue(input));
        }

        private double ParseDoubleValue(string input)
        {
            if (double.TryParse(
                    input,
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out double currentResult))
            {
                return currentResult;
            }

            if (double.TryParse(
                    input,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double invariantResult))
            {
                return invariantResult;
            }

            throw new Exception($"Khong convert duoc '{input}' sang Double.");
        }
    }
}
