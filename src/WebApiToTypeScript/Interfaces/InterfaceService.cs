using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
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

        public InterfaceNode AddInterfaceNode(TypeDefinition typeDefinition)
        {
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

                var primitiveType = TypeService.GetPrimitiveTypeScriptType(thingType.FullName);
                if (primitiveType != null)
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

                var constructorParameters = new List<string>();

                foreach (var thing in things)
                {
                    var thingType = thing.CSharpType.TypeDefinition;
                    var collectionString = thing.CSharpType.IsCollection ? "[]" : string.Empty;

                    var primitiveType = TypeService.GetPrimitiveTypeScriptType(thingType.FullName);
                    if (primitiveType != null)
                    {
                        constructorParameters.Add($"{thing.Name}?: {primitiveType}{collectionString}");
                    }
                    else
                    {
                        if (thingType.IsEnum && Config.GenerateEnums)
                        {
                            constructorParameters.Add(
                                $"{thing.Name}?: {Config.EnumsNamespace}.{thingType.Name}{collectionString}");
                        }
                        else if (!thingType.IsPrimitive)
                        {
                            constructorParameters.Add($"{thing.Name}?: {thingType.Name}{collectionString}");
                        }
                    }
                }

                var constructorParameterStrings = constructorParameters
                    .Select(p => $"public {p}");

                var constructorParametersList =
                    string.Join(", ", constructorParameterStrings);

                var constructorBlock = interfaceBlock
                    .AddAndUseBlock($"constructor({constructorParametersList})");

                if (hasBaseClass)
                    constructorBlock.AddStatement("super();");

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