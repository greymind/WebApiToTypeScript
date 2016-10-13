using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebApiToTypeScript.Block;

namespace WebApiToTypeScript.Views
{
    public class ViewsService : ServiceAware
    {
        private string ViewsSourceDirectory { get; set; }

        private List<ViewNode> FeatureViews { get; }
            = new List<ViewNode>();

        public TypeScriptBlock CreateViewsBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.ViewsNamespace}");
        }

        public TypeScriptBlock WriteViewsToBlock(TypeScriptBlock viewsBlock)
        {
            ViewsSourceDirectory = Path.GetFullPath(Config.ViewsSourceDirectory);

            var viewFiles = Directory.GetFiles(ViewsSourceDirectory, "*.view.*", SearchOption.AllDirectories);

            foreach (var viewFile in viewFiles)
            {
                var featureViewPath = Path.GetFullPath(viewFile)
                    .Split(new[] { $@"{ViewsSourceDirectory}\" }, StringSplitOptions.RemoveEmptyEntries)
                    .Last();

                var parts = featureViewPath
                    .Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

                var namespaces = parts
                    .Take(parts.Length - 1)
                    .Select(Helpers.ToPascalCaseFromCamelCase)
                    .ToList();

                var featureNamespace = namespaces.First();

                if (FeatureViews.All(v => v.Name != featureNamespace))
                {
                    FeatureViews.Add(new ViewNode
                    {
                        Name = featureNamespace
                    });
                }

                var subFeatures = namespaces
                    .Skip(1)
                    .ToList();

                var viewNode = FeatureViews.Single(v => v.Name == featureNamespace);
                var nameThusFar = subFeatures.Count == 0 // instead should i skip last or first or something 
                    ? featureNamespace
                    : string.Empty;

                foreach (var subFeature in subFeatures)
                {
                    var doesViewNodeHasSubFeature = viewNode.ChildViews
                        .Any(v => v.Name == subFeature);

                    if (!doesViewNodeHasSubFeature)
                    {
                        var subFeatureNode = new ViewNode
                        {
                            Name = subFeature
                        };

                        viewNode.ChildViews.Add(subFeatureNode);
                    }

                    viewNode = viewNode.ChildViews
                        .Single(v => v.Name == subFeature);

                    nameThusFar += viewNode.Name;
                }

                var fullViewNameInKebabCase = parts
                    .Last()
                    .Split(new[] {".view."}, StringSplitOptions.RemoveEmptyEntries)
                    .First();

                var fullViewNameInPascalCase = Helpers.ToPascalCaseFromKebabCase(fullViewNameInKebabCase);

                var viewName = fullViewNameInPascalCase != nameThusFar
                    ? Regex.Replace(fullViewNameInPascalCase, $"^{nameThusFar}", string.Empty)
                    : fullViewNameInPascalCase;

                viewNode.ViewEntries.Add(new ViewEntry { 
                    Name = viewName,
                    Path = featureViewPath.Replace(@"\", "/")
                });
            }

            foreach (var featureView in FeatureViews)
            {
                WriteViewEntry(viewsBlock, featureView);
            }

            return viewsBlock;
        }

        private void WriteViewEntry(TypeScriptBlock viewsBlock, ViewNode featureViewNode, bool isChild = false)
        {
            var viewNamespace = isChild
                ? string.Empty
                : ".Views";

            var featureBlock = viewsBlock
                .AddAndUseBlock($"export namespace {featureViewNode.Name}{viewNamespace}");

            foreach (var viewEntry in featureViewNode.ViewEntries)
            {
                featureBlock
                    .AddStatement($"export const {viewEntry.Name} = '{viewEntry.Path}';");
            }

            foreach (var childView in featureViewNode.ChildViews)
            {
                WriteViewEntry(featureBlock, childView, isChild: true);
            }
        }
    }
}