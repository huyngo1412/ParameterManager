using Autodesk.Revit.DB;
using ParameterManager.Models;

namespace ParameterManager.Contracts
{
    public interface IParameterValueSetter
    {
        bool CanHandle(StorageType storageType);

        void SetValue(Parameter parameter, ParameterTreeNode node);
    }
}
