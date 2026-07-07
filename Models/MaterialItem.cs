namespace ParameterManager.Models
{
    public class MaterialItem
    {
        public long? MaterialId { get; set; }

        public string Name { get; set; }

        public bool IsPlaceholder { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
