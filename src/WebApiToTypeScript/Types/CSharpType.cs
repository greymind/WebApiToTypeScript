using Mono.Cecil;

namespace WebApiToTypeScript.Types
{
    public class CSharpType
    {
        public TypeDefinition TypeDefinition { get; set; }
        public bool IsNullable { get; set; }
        public bool IsCollection { get; set; }
        public bool IsValueType { get; set; }
        public bool IsGenericParameter { get; set; }
        public string GenericParameterName { get; set; }
    }
}