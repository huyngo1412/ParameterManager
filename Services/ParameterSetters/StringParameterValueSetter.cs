using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using ParameterManager.Models;

namespace ParameterManager.Services.ParameterSetters
{
    public class StringParameterValueSetter : IParameterValueSetter
    {
        public bool CanHandle(StorageType storageType)
        {
            return storageType == StorageType.String;
        }

        public void SetValue(Parameter parameter, ParameterTreeNode node)
        {
            parameter.Set(node.Value ?? string.Empty);
        }
    }
}
