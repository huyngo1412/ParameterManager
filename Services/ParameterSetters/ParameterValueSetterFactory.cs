using Autodesk.Revit.DB;
using ParameterManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParameterManager.Services.ParameterSetters
{
    public class ParameterValueSetterFactory : IParameterValueSetterFactory
    {
        private readonly IList<IParameterValueSetter> _setters;

        public ParameterValueSetterFactory()
        {
            _setters = new List<IParameterValueSetter>
            {
                new StringParameterValueSetter(),
                new IntegerParameterValueSetter(),
                new DoubleParameterValueSetter(),
                new ElementIdParameterValueSetter()
            };
        }

        public IParameterValueSetter GetSetter(StorageType storageType)
        {
            IParameterValueSetter setter =
                _setters.FirstOrDefault(x => x.CanHandle(storageType));

            if (setter == null)
            {
                throw new NotSupportedException(
                    $"StorageType '{storageType}' chua duoc ho tro.");
            }

            return setter;
        }
    }
}
