using ParameterManager.Models;
using System.Collections.Generic;

namespace ParameterManager.Contracts
{
    public interface IMaterialRepository
    {
        IList<MaterialItem> GetMaterials();
    }
}
