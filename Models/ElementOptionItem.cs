namespace ParameterManager.Models
{
    public class ElementOptionItem
    {
        public long? ElementIdValue { get; set; }

        public string Name { get; set; }

        public bool IsPlaceholder { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}