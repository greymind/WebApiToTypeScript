using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Config;

namespace WebApiToTypeScript.Enums
{
    public class EnumsService : ServiceAware
    {
        private readonly Regex regexToFindUppercases
            = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

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
            var sortedEnums = Enums
                .OrderBy(e => e.Name);

            foreach (var typeDefinition in sortedEnums)
            {
                WriteEnumForType(enumsBlock, typeDefinition);
                if (Config.GenerateEnumDescriptions)
                    CreateGetDescriptionExtensionForType(enumsBlock, typeDefinition);
            }

            foreach (var logMessage in LogMessages)
                LogMessage(logMessage.Value);

            return enumsBlock;
        }

        private void WriteEnumForType(TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.HasConstant && !f.IsSpecialName);

            var enumBlock = enumsBlock
                .AddAndUseBlock($"export enum {typeDefinition.Name}");

            foreach (var field in fields)
                enumBlock.AddStatement($"{field.Name} = {field.Constant},");
        }

        private void CreateGetDescriptionExtensionForType(TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.HasConstant && !f.IsSpecialName);

            var switchBlock = enumsBlock
                .AddAndUseBlock($"export namespace {typeDefinition.Name}")
                .AddAndUseBlock($"export function getDescription(enumValue: {typeDefinition.Name})")
                .AddAndUseBlock("switch (enumValue)");

            foreach (var field in fields)
            {
                var fieldDescription = GetFieldDescription(field);

                switchBlock
                    .AddStatement($"case {typeDefinition.Name}.{field.Name}: return \"{fieldDescription}\";");
            }
        }

        private string GetFieldDescription(FieldDefinition field)
        {
            var descriptionAttributeName = typeof(DescriptionAttribute).FullName;

            var fieldDescription =
                field.CustomAttributes.Any(attr => attr.AttributeType.FullName == descriptionAttributeName)
                    ? field.CustomAttributes
                           .Single(attr => attr.AttributeType.FullName == descriptionAttributeName)
                           .ConstructorArguments[0]
                           .Value
                           .ToString()
                    : regexToFindUppercases.Replace(field.Name, " ");

            return fieldDescription;
        }

        internal void AddMatchingEnums()
        {
            foreach (var enumMatchConfig in Config.EnumMatches)
            {
                var matchingTypes = TypeService
                    .Types
                    .Where(type => MatchConfigWithBaseType.IsMatch(enumMatchConfig, type.FullName));

                foreach (var matchingType in matchingTypes)
                {
                    AddEnum(matchingType);
                }
            }
        }
    }
}