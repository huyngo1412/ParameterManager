using Autodesk.Revit.DB;
using ParameterManager.Models;
using System.Collections.Generic;

namespace ParameterManager.Contracts
{
    public interface IParameterService
    {
        IList<ParameterDescriptor> GetCommonEditableParameters(
            IList<ElementType> selectedTypes,
            IList<Element> selectedInstances);

        AssignResult AssignParameters(
            IList<ElementType> selectedTypes,
            IList<Element> selectedInstances,
            IList<ParameterTreeNode> editedParameters);
    }
}
