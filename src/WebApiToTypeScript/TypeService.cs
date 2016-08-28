using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiToTypeScript
{
    public class TypeService
    {

        private Dictionary<string, List<Type>> PrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        public TypeService()
        {
            LoadPrimitiveTypesMapping();
        }

        private void LoadPrimitiveTypesMapping()
        {
            var mapping = PrimitiveTypesMapping;

            mapping["string"] = new List<Type> { typeof(string), typeof(System.Guid), typeof(DateTime) };
            mapping["boolean"] = new List<Type> { typeof(bool) };
            mapping["number"] = new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };
        }


        public string GetPrimitiveType(string typeName)
        {
            return PrimitiveTypesMapping
                .Select(m => m.Value.Any(t => t.FullName == typeName) ? m.Key : string.Empty)
                .SingleOrDefault(name => !string.IsNullOrEmpty(name));
        }

        public bool IsPrimitiveType(string typeName)
        {
            return PrimitiveTypesMapping.Keys
                .Contains(typeName);
        }
    }
}
