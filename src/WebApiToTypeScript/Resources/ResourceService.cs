using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using WebApiToTypeScript.Block;

namespace WebApiToTypeScript.Resources
{
    public class ResourceService : ServiceAware
    {
        public Regex ParamRegex { get; }
            = new Regex(@"{(\w*)}");

        public IEnumerable<ResourceBlock> GetBlocksForResources()
        {
            foreach (var resourceConfig in Config.ResourceConfigs)
            {
                var resourceFilename = Path.GetFileNameWithoutExtension(resourceConfig.SourcePath);
                var resourceBlock = CreateResourceBlock();

                var block = resourceBlock
                    .AddAndUseBlock($"export var {resourceFilename} = ");

                var resourceReader = new ResXResourceReader(resourceConfig.SourcePath);
                var dictionary = resourceReader.GetEnumerator();

                while (dictionary.MoveNext())
                {
                    var parameters = new List<ParameterTransform>();

                    var matches = ParamRegex.Matches(dictionary.Value.ToString())
                        .GetEnumerator();

                    while (matches.MoveNext())
                    {
                        var match = (Match)matches.Current;

                        var source = match.Groups[1].Value;

                        int integer;
                        var isInteger = int.TryParse(source, out integer);

                        var destination = isInteger
                            ? $"slot{source}"
                            : source;

                        parameters.Add(new ParameterTransform
                        {
                            Source = source,
                            Destination = destination
                        });
                    }

                    var originalValue = dictionary.Value.ToString()
                        .Replace("\"", "\\\"");

                    if (parameters.Any())
                    {
                        var paramsString = string.Join(", ", parameters.Select(p => $"{p.Destination}: string"));

                        var transformedValue = parameters
                            .Aggregate(originalValue, (current, parameterTransform) => current.Replace(parameterTransform.Source, parameterTransform.Destination))
                            .Replace("{", "${");

                        block
                            .AddAndUseBlock($"{dictionary.Key} : function({paramsString})", terminationString: ",")
                            .AddStatement($"return `{transformedValue}`;");
                    }
                    else
                    {
                        block
                            .AddStatement($"{dictionary.Key} : `{originalValue}`,");
                    }
                }

                yield return new ResourceBlock
                {
                    TypeScriptBlock = resourceBlock,
                    Filename = resourceConfig.OutputFilename
                };
            }
        }

        private TypeScriptBlock CreateResourceBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.ResourcesNamespace}");
        }
    }
}