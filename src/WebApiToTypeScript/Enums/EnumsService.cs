using System.Linq;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using WebApiToTypeScript.Block;

namespace WebApiToTypeScript.Enums
{
    public class EnumsService : ServiceAware
    {
        private List<TypeDefinition> Enums { get; }
            = new List<TypeDefinition>();

        private Dictionary<string, string> LogMessages { get; }
            = new Dictionary<string, string>();

        public TypeScriptBlock CreateEnumsBlock()
        {
            return Config.GenerateEnums
                ? new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.EnumsNamespace}")
                : new TypeScriptBlock();
        }

        public void AddEnum(TypeDefinition thingType)
        {
            if (Enums.All(e => e.FullName != thingType.FullName))
            {
                var conflictEnum = Enums.SingleOrDefault(e => e.Name == thingType.Name);
                if (conflictEnum == null)
                {
                    Enums.Add(thingType);
                }
                else
                {
                    LogMessages[thingType.Name] = $"Enum [{thingType.Name}] of type [{thingType.FullName}] conflicts with [{conflictEnum.FullName}]";
                }
            }
        }

        public TypeScriptBlock WriteEnumsToBlock(TypeScriptBlock enumsBlock)
        {
            foreach (var typeDefinition in Enums)
                CreateEnumForType(enumsBlock, typeDefinition);

            foreach (var logMessage in LogMessages)
                LogMessage(logMessage.Value);

            return enumsBlock;
        }

        private void CreateEnumForType(TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.HasConstant && !f.IsSpecialName);

            var enumBlock = enumsBlock
                .AddAndUseBlock($"export enum {typeDefinition.Name}");

            foreach (var field in fields)
                enumBlock.AddStatement($"{field.Name} = {field.Constant},");
        }
    }
}