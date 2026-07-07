using Autodesk.Revit.DB;

namespace ParameterManager.Contracts
{
    public interface IParameterValueSetterFactory
    {
        IParameterValueSetter GetSetter(StorageType storageType);
    }
}
