using System.Collections.Generic;

namespace WebApiToTypeScript.Views
{
    public class ViewNode
    {
        public string Name { get; set; }

        public List<ViewEntry> ViewEntries { get; }
            = new List<ViewEntry>();

        public List<ViewNode> ChildViews { get; }
            = new List<ViewNode>();
    }
}
