using Mono.Cecil;
using System.Collections.Generic;

namespace WebApiToTypeScript.Types
{
    public class MemberWithCSharpType
    {
        public string Name { get; set; }
        public CSharpType CSharpType { get; set; }
        public List<CustomAttribute> CustomAttributes { get; set; }
    }
}