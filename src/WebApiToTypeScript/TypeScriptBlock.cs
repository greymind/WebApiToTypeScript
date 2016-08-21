using System.Collections.Generic;

namespace WebApiToTypeScript
{
    public interface ITypeScriptCode
    {
        string ToString(int indent);
    }

    public class TypeScriptStatement : ITypeScriptCode
    {
        public string Statement { get; set; }

        public string ToString(int indent)
        {
            var stringBuilder = new IndentAwareStringBuilder
            {
                Indent = indent
            };

            stringBuilder.AppendLine(Statement);

            return stringBuilder.ToString();
        }
    }

    public class TypeScriptBlock : ITypeScriptCode
    {
        private const int IndentPerLevel = 4;

        public string Outer { get; set; }

        public List<ITypeScriptCode> Children { get; set; }
            = new List<ITypeScriptCode>();

        public TypeScriptBlock Parent { get; set; }

        public TypeScriptBlock(string outer = "")
        {
            Outer = outer;
        }

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
            var child = new TypeScriptStatement
            {
                Statement = statement
            };

            Children.Add(child);

            return this;
        }

        public TypeScriptBlock AddNewLine()
            => AddStatement(string.Empty);

        public override string ToString()
            => ToString(0);

        public string ToString(int indent)
        {
            var stringBuilder = new IndentAwareStringBuilder
            {
                Indent = indent
            };

            stringBuilder.AppendLine($"{Outer} {{");

            for (int c = 0; c < Children.Count; c++)
            {
                var child = Children[c];

                var childIndent = stringBuilder.Indent + IndentPerLevel;
                var childString = child.ToString(childIndent);

                stringBuilder.AppendWithoutIndent(childString);

                var nextChild = c < Children.Count - 1 ? Children[c + 1] : null;

                var isNextChildDifferent = nextChild?.GetType() != child.GetType();
                var isNextChildABlock = nextChild is TypeScriptBlock;
                var isThisTheLastChild = c == Children.Count - 1;

                if ((isNextChildDifferent || isNextChildABlock)
                    && !isThisTheLastChild)
                {
                    stringBuilder.AppendLine(string.Empty);
                }
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}