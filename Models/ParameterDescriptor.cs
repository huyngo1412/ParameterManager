namespace ParameterManager.Models
{
    public class ParameterDescriptor
    {
        public long ParameterId { get; set; }

        public string ParameterName { get; set; }

        public string GroupName { get; set; }

        public string Source { get; set; }

        public string StorageType { get; set; }

        public string DisplayValue { get; set; }

        public long? CommonElementIdValue { get; set; }

        public bool? CommonBooleanValue { get; set; }

        public ParameterScope Scope { get; set; }

        public ParameterEditorKind EditorKind { get; set; }

        public string ElementOptionKind { get; set; }

        public string ScopeText => Scope == ParameterScope.Type ? "Type" : "Instance";

        public string SourceWithScope => $"{Source} ({ScopeText})";
    }
}