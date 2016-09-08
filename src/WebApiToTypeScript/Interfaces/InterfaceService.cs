using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace WebApiToTypeScript.Interfaces
{
    public class InterfaceService
    {
        private readonly Config Config;
        private readonly TypeService typeService;

        private List<TypeDefinition> Enums;

        private List<TypeDefinition> Interfaces { get; }
            = new List<TypeDefinition>();

        private InterfaceNode InterfaceNode { get; }
            = new InterfaceNode();


        public InterfaceService(
            Config config, 
            TypeService typeService, 
            List<TypeDefinition> enums)
        {
            Config = config;
            this.typeService = typeService;
            Enums = enums;
        }

        public void AddInterface(TypeDefinition typeDefinition)
        {
            if (Interfaces.All(i => i.FullName != typeDefinition.FullName))
                Interfaces.Add(typeDefinition);
        }

        public TypeScriptBlock CreateInterfacesNode()
        {
            var interfacesBlock = new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.InterfacesNamespace}");

            foreach (var typeDefinition in Interfaces)
                AddInterfaceNode(typeDefinition);

            WriteInterfaces(interfacesBlock, InterfaceNode);

            return interfacesBlock;
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

                    var primitiveType = typeService.GetPrimitiveTypeScriptType(thingType.FullName);
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
                    CSharpType = typeService.GetCSharpType(p.FieldType)
                });

            var properties = typeDefinition.Properties
                .Where(p => !p.IsSpecialName && p.SetMethod != null)
                .Select(p => new MemberWithCSharpType
                {
                    Name = p.Name,
                    CSharpType = typeService.GetCSharpType(p.PropertyType)
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

        private InterfaceNode AddInterfaceNode(TypeDefinition typeDefinition)
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

                var primitiveType = typeService.GetPrimitiveTypeScriptType(thingType.FullName);
                if (primitiveType != null)
                    continue;

                if (thingType.IsEnum && Config.GenerateEnums)
                {
                    if (Enums.All(e => e.FullName != thingType.FullName))
                        Enums.Add(thingType);
                }
                else if (!thingType.IsPrimitive)
                {
                    AddInterfaceNode(thingType);
                }
            }

            return AddInterfaceNode(typeDefinition, baseInterfaceNode);
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
