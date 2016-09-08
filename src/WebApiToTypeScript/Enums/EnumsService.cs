using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using WebApiToTypeScript.Block;

namespace WebApiToTypeScript.Enums
{
    public class EnumsService
    {
        private List<TypeDefinition> Enums { get; }
            = new List<TypeDefinition>();

        public void AddEnum(TypeDefinition thingType)
        {
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