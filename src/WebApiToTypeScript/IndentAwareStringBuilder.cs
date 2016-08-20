using System.Text;

namespace WebApiToTypeScript
{
    public class IndentAwareStringBuilder
    {
        private StringBuilder stringBuilder
            = new StringBuilder();

        public int Indent { get; set; }

        public void AppendLineWithoutIndent(string line)
        {
            stringBuilder.AppendLine(line);
        }

        public void AppendLine(string line)
        {
            AppendIndent();
            AppendLineWithoutIndent(line);
        }

        public void AppendWithoutIndent(string line)
        {
            stringBuilder.Append(line);
        }

        public void Append(string line)
        {
            AppendIndent();
            AppendWithoutIndent(line);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        private void AppendIndent()
        {
            for (int i = 0; i < Indent; i++)
                stringBuilder.Append(' ');
        }
    }
}