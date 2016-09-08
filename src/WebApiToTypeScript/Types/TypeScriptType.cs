namespace WebApiToTypeScript.Types
{
    public class TypeScriptType
    {
        public string TypeName { get; set; }
        public bool IsEnum { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsCollection { get; set; }
    }
}