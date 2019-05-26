using System;

namespace TypeKitchen
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MetadataTypeAttribute : Attribute
    {
        public MetadataTypeAttribute(Type metadataType)
        {
            MetadataType = metadataType;
        }

        public Type MetadataType { get; set; }
    }
}
