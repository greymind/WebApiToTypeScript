using System.Collections.Generic;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class WebApiRoutePart
    {
        public string Name { get; set; }
        public string ParameterName { get; set; }
        public ParameterDefinition Parameter { get; set; }

        public List<string> Constraints { get; set; }
            = new List<string>();

        public List<string> CustomAttributes { get; set; }
            = new List<string>();
    }
}