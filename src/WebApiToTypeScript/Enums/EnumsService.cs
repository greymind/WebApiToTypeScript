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

        public TypeScriptBlock CreateEnumsBlock()
        {
            return Config.GenerateEnums
                ? new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.EnumsNamespace}")
                : new TypeScriptBlock();
        }

        public void AddEnum(TypeDefinition thingType)
        {
            // todo-balki duplicate enums detection
            // fullname might be the issue here, since we are actually going by final namespaced name
            // but we might want to detect that AFTER adding all unique types so we can complain correctly what conflicted

            if (Enums.All(e => e.FullName != thingType.FullName))
                Enums.Add(thingType);
        }

        public TypeScriptBlock WriteEnumsToBlock(TypeScriptBlock enumsBlock)
        {
            foreach (var typeDefinition in Enums)
                CreateEnumForType(enumsBlock, typeDefinition);

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