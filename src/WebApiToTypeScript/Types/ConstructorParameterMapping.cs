using WebApiToTypeScript.Config;

namespace WebApiToTypeScript.Types
{
    public class ConstructorParameterMapping
    {
        public bool IsOptional { get; set; }
        public TypeMapping TypeMapping { get; set; }
        public string Name { get; set; }
        public string String { get; set; }
    }
}