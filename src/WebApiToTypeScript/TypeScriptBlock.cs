using System.Collections.Generic;

namespace WebApiToTypeScript
{
    public class TypeScriptBlock
    {
        private const int IndentPerLevel = 4;

        public string Outer { get; set; }

        public List<TypeScriptBlock> Children { get; set; }
            = new List<TypeScriptBlock>();

        public TypeScriptBlock Parent { get; set; }

        public TypeScriptBlock WithBlock(string outer = null)
        {
            var child = new TypeScriptBlock
            {
                Outer = outer,
                Parent = this
            };

            Children.Add(child);

            return child;
        }

        public override string ToString()
            => ToString(0);

        public string ToString(int indent)
        {
            var stringBuilder = new IndentAwareStringBuilder
            {
                Indent = indent
            };

            stringBuilder.AppendLine($"{Outer} {{");

            foreach (var child in Children)
            {
                var childIndent = indent + IndentPerLevel;
                var childString = child.ToString(childIndent);

                stringBuilder.AppendLine(childString);
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}