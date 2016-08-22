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

        public bool IsFunctionBlock { get; set; }

        public TypeScriptBlock(string outer = "")
        {
            Outer = outer;
        }

        public TypeScriptBlock AddBlock(string outer = null, bool isFunctionBlock = false)
        {
            var child = CreateChild(outer, isFunctionBlock);

            return this;
        }

        public TypeScriptBlock AddAndUseBlock(string outer = null, bool isFunctionBlock = false)
        {
            var child = CreateChild(outer, isFunctionBlock);

            return child;
        }

        private TypeScriptBlock CreateChild(string outer, bool isFunctionBlock)
        {
            var child = new TypeScriptBlock
            {
                Outer = outer,
                Parent = this,
                IsFunctionBlock = isFunctionBlock
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

            if (!string.IsNullOrEmpty(Outer))
                stringBuilder.AppendLine($"{Outer} {{");
            else
                stringBuilder.AppendLine($"{{");

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
                var isNextChildANonElseBlock = isNextChildABlock
                    && ((TypeScriptBlock)nextChild).Outer != "else";

                if ((isNextChildDifferent || isNextChildANonElseBlock)
                    && !isThisTheLastChild)
                {
                    stringBuilder.AppendLine(string.Empty);
                }
            }

            if (IsFunctionBlock)
                stringBuilder.AppendLine("});");
            else
                stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}