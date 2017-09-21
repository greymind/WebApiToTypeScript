using Mono.Cecil;

namespace WebApiToTypeScript.Types
{
    public class CSharpType
    {
        public TypeDefinition TypeDefinition { get; set; }

        public bool IsNullable { get; set; }
        public int CollectionLevel { get; set; }

        public bool IsValueType { get; set; }

        public bool IsGenericParameter { get; set; }
        public string GenericParameterName { get; set; }

        public bool IsGenericInstance { get; set; }
        public TypeReference[] GenericArgumentTypes { get; set; }

        public bool IsValid
        {
            get
            {
                return IsGenericParameter || TypeDefinition != null;
            }
        }
    }
}