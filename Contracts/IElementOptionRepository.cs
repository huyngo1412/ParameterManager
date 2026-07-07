using ParameterManager.Models;
using System.Collections.Generic;

namespace ParameterManager.Contracts
{
    public interface IElementOptionRepository
    {
        IList<ElementOptionItem> GetOptions(string optionKind);
    }
}