using Mono.Cecil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.Interfaces
{
    public class InterfaceService : ServiceAware
    {
        private InterfaceNode InterfaceNode { get; }
            = new InterfaceNode();

        public TypeScriptBlock CreateInterfacesBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.InterfacesNamespace}");
        }

        public TypeScriptBlock WriteInterfacesToBlock(TypeScriptBlock interfacesBlock)
        {
            WriteInterfaces(interfacesBlock, InterfaceNode);

            return interfacesBlock;
        }

        public void AddMatchingInterfaces()
        {
            foreach (var interfaceMatch in Config.InterfaceMatches)
            {
                var matchConfigExists = !string.IsNullOrEmpty(interfaceMatch.Match);
                var matchRegEx = matchConfigExists ? new Regex(interfaceMatch.Match) : null;

                var baseTypeNameConfigExists = !string.IsNullOrEmpty(interfaceMatch.BaseTypeName);
                if (baseTypeNameConfigExists)
                {
                    var baseType = TypeService.GetTypeDefinition(interfaceMatch.BaseTypeName);
                    AddInterfaceNode(baseType);
                }

                foreach (var type in TypeService.Types)
                {
                    var isMatch = matchRegEx != null && matchRegEx.IsMatch(type.FullName);
                    var doesBaseTypeMatch = !baseTypeNameConfigExists
                        || TypeService.GetBaseTypes(type).Any(t => t.FullName.EndsWith(interfaceMatch.BaseTypeName));

                    if (isMatch && doesBaseTypeMatch)
                    {
                        AddInterfaceNode(type);
                    }
                }
            }
        }

        public InterfaceNode AddInterfaceNode(TypeDefinition typeDefinition)
        {
            Debug.Assert(typeDefinition != null);

            var interfaceNode = SearchForInterfaceNode(InterfaceNode, typeDefinition);

            if (interfaceNode != null)
                return interfaceNode;

            var baseClass = typeDefinition.BaseType as TypeDefinition;
            var isBaseClassNotObject = baseClass != null && baseClass.FullName != "System.Object";

            var baseInterfaceNode = InterfaceNode;
            if (isBaseClassNotObject)
                baseInterfaceNode = AddInterfaceNode(baseClass);

            var things = GetMembers(typeDefinition);

            foreach (var thing in things)
            {
                var thingType = thing.CSharpType.TypeDefinition;

                var typeMapping = TypeService.GetTypeScriptType(thingType, thing.Name);
                if (typeMapping != null)
                    continue;

                if (thingType.IsEnum && Config.GenerateEnums)
                {
                    EnumsService.AddEnum(thingType);
                }
                else if (!thingType.IsPrimitive)
                {
                    AddInterfaceNode(thingType);
                }
            }

            return AddInterfaceNode(typeDefinition, baseInterfaceNode);
        }

        private void WriteInterfaces(TypeScriptBlock interfacesBlock, InterfaceNode interfaceNode)
        {
            var typeDefinition = interfaceNode.TypeDefinition;

            if (typeDefinition != null)
            {
                var hasBaseClass = interfaceNode.BaseInterface?.TypeDefinition != null;

                var extendsString = hasBaseClass
                    ? $" extends {interfaceNode.BaseInterface.TypeDefinition.Name}"
                    : string.Empty;

                var interfaceBlock = interfacesBlock
                    .AddAndUseBlock($"export class {typeDefinition.Name}{extendsString}");

                var things = GetMembers(typeDefinition);

                foreach (var thing in things)
                {
                    var thingType = thing.CSharpType.TypeDefinition;
                    var collectionString = thing.CSharpType.IsCollection ? "[]" : string.Empty;

                    var typeScriptType = TypeService.GetTypeScriptType(thingType, thing.Name);

                    interfaceBlock
                        .AddStatement($"{thing.Name}: {typeScriptType.TypeName}{collectionString};");
                }

                if (hasBaseClass)
                {
                    interfaceBlock
                       .AddAndUseBlock("constructor()")
                       .AddStatement("super();");
                }

                interfaceBlock
                    .AddAndUseBlock("getQueryParams()")
                    .AddStatement("return this;");
            }

            foreach (var derivedInterfaceNode in interfaceNode.DerivedInterfaces)
                WriteInterfaces(interfacesBlock, derivedInterfaceNode);
        }

        private List<MemberWithCSharpType> GetMembers(TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.IsPublic && !f.IsSpecialName && !f.IsStatic)
                .Select(p => new MemberWithCSharpType
                {
                    Name = p.Name,
                    CSharpType = TypeService.GetCSharpType(p.FieldType)
                });

            var properties = typeDefinition.Properties
                .Where(p => !p.IsSpecialName && p.SetMethod != null)
                .Select(p => new MemberWithCSharpType
                {
                    Name = p.Name,
                    CSharpType = TypeService.GetCSharpType(p.PropertyType)
                });

            return fields.Union(properties)
                .Where(t => t.CSharpType.TypeDefinition != null)
                .ToList();
        }

        private InterfaceNode SearchForInterfaceNode(InterfaceNode interfaceNode, TypeDefinition typeDefinition)
        {
            if (typeDefinition == null)
                return null;

            if (interfaceNode.TypeDefinition?.FullName == typeDefinition.FullName)
                return interfaceNode;

            return interfaceNode.DerivedInterfaces
                .Select(derivedInterfaceNode => SearchForInterfaceNode(derivedInterfaceNode, typeDefinition))
                .FirstOrDefault(resultNode => resultNode != null);
        }

        private InterfaceNode AddInterfaceNode(TypeDefinition typeDefinition, InterfaceNode baseInterfaceNode)
        {
            var interfaceNode = SearchForInterfaceNode(InterfaceNode, typeDefinition);

            if (interfaceNode != null)
                return interfaceNode;

            var derivedInterfaceNode = new InterfaceNode
            {
                TypeDefinition = typeDefinition,
                BaseInterface = baseInterfaceNode
            };

            baseInterfaceNode.DerivedInterfaces
                .Add(derivedInterfaceNode);

            return derivedInterfaceNode;
        }
    }
}