using System.Collections.Generic;

namespace ParameterManager.Models
{
    public class AssignResult
    {
        public int SuccessCount { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public bool HasErrors => Errors.Count > 0;
    }
}
