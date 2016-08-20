using System.Text;

namespace WebApiToTypeScript
{
    public class IndentAwareStringBuilder
    {
        private StringBuilder stringBuilder
            = new StringBuilder();

        public int Indent { get; set; }

        public void AppendLine(string line)
        {
            AppendIndent();
            stringBuilder.AppendLine(line);
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