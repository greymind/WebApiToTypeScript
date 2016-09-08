using Mono.Cecil;
using System.Collections.Generic;

namespace WebApiToTypeScript.Interfaces
{
    public class InterfaceNode
    {
        public TypeDefinition TypeDefinition { get; set; }
        public InterfaceNode BaseInterface { get; set; }

        public List<InterfaceNode> DerivedInterfaces { get; }
            = new List<InterfaceNode>();
    }
}
