using System.Collections.Generic;
using System.Linq;

namespace WebApiToTypeScript
{
    public class TypeScriptBlock
    {
        private const int IndentPerLevel = 4;

        public string Outer { get; set; }

        public List<string> Statements { get; set; }
            = new List<string>();

        public List<TypeScriptBlock> Children { get; set; }
            = new List<TypeScriptBlock>();

        public TypeScriptBlock Parent { get; set; }

        public TypeScriptBlock AddBlock(string outer = null)
        {
            var child = CreateChild(outer);

            return this;
        }

        public TypeScriptBlock AddAndUseBlock(string outer = null)
        {
            var child = CreateChild(outer);

            return child;
        }

        private TypeScriptBlock CreateChild(string outer)
        {
            var child = new TypeScriptBlock
            {
                Outer = outer,
                Parent = this
            };

            Children.Add(child);

            return child;
        }

        public TypeScriptBlock AddStatement(string statement)
        {
            Statements.Add(statement);

            return this;
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

            stringBuilder.Indent += IndentPerLevel;

            foreach (var statement in Statements)
                stringBuilder.AppendLine(statement);

            stringBuilder.Indent -= IndentPerLevel;

            if (Statements.Any() && Children.Any())
                stringBuilder.AppendLine(string.Empty);

            foreach (var child in Children)
            {
                var childIndent = stringBuilder.Indent + IndentPerLevel;
                var childString = child.ToString(childIndent);

                stringBuilder.AppendLineWithoutIndent(childString);
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}