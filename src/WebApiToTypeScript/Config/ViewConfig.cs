namespace WebApiToTypeScript.Config
{
    public class ViewConfig
    {
        public string SourceDirectory { get; set; }
        public string Namespace { get; set; }
        public string OutputFilename { get; set; }

        public string Prefix { get; set; }
            = "";

        public bool UrlEncodePath { get; set; }
            = false;

        public bool GenerateAsLowercase { get; set; }
            = false;
    }
}