using System.Collections.Generic;

namespace WebApiToTypeScript.Block
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

        public string TerminationString { get; set; }
            = string.Empty;

        public TypeScriptBlock(string outer = "")
        {
            Outer = outer;
        }

        public TypeScriptBlock AddBlock(TypeScriptBlock block)
        {
            if (block != null)
                CreateChild(block);

            return this;
        }

        public TypeScriptBlock AddBlock(string outer = null, bool isFunctionBlock = false, string terminationString = "")
        {
            var child = CreateChild(outer, isFunctionBlock, terminationString);

            return this;
        }

        public TypeScriptBlock AddAndUseBlock(string outer = null, bool isFunctionBlock = false, string terminationString = "")
        {
            var child = CreateChild(outer, isFunctionBlock, terminationString);

            return child;
        }

        private TypeScriptBlock CreateChild(TypeScriptBlock block)
        {
            Children.Add(block);

            return this;
        }

        private TypeScriptBlock CreateChild(string outer, bool isFunctionBlock, string terminationString = "")
        {
            var child = new TypeScriptBlock
            {
                Outer = outer,
                Parent = this,
                IsFunctionBlock = isFunctionBlock,
                TerminationString = terminationString
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
            {
                var outerPaddingString = Outer.EndsWith("(") ? string.Empty : " ";
                stringBuilder.AppendLine($"{Outer}{outerPaddingString}{{");
            }
            else
            {
                stringBuilder.AppendLine($"{{");
            }

            for (var c = 0; c < Children.Count; c++)
            {
                var child = Children[c];

                var childIndent = stringBuilder.Indent + IndentPerLevel;
                var childString = child.ToString(childIndent);

                stringBuilder.AppendWithoutIndent(childString);

                var nextChild = c < Children.Count - 1 ? Children[c + 1] : null;
                var isThisTheLastChild = c == Children.Count - 1;

                AppendNewLineIfApplicable(nextChild, child, isThisTheLastChild, stringBuilder);
            }

            var blockEndString = IsFunctionBlock ? "})" : "}";

            stringBuilder.AppendLine($"{blockEndString}{TerminationString}");

            return stringBuilder.ToString();
        }

        private static void AppendNewLineIfApplicable(ITypeScriptCode nextChild, ITypeScriptCode child,
            bool isThisTheLastChild, IndentAwareStringBuilder stringBuilder)
        {
            var isNextChildDifferent = nextChild?.GetType() != child.GetType();
            var isNextChildABlock = nextChild is TypeScriptBlock;
            var isNextChildANonElseBlock = isNextChildABlock
                && ((TypeScriptBlock)nextChild).Outer != "else";
            var isNextChildFunctionBlock = isNextChildABlock
                && ((TypeScriptBlock)nextChild).IsFunctionBlock;

            if ((isNextChildDifferent || isNextChildANonElseBlock)
                && !isThisTheLastChild && !isNextChildFunctionBlock)
            {
                stringBuilder.AppendLine(string.Empty);
            }
        }
    }
}