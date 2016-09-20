using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.Interfaces
{
    public class InterfaceService : ServiceAware
    {
        private readonly Regex genericNameRegEx
            = new Regex("(.*)(`(\\d*))");

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

                var excludeMatchConfigExists = !string.IsNullOrEmpty(interfaceMatch.ExcludeMatch);
                var excludeMatchRegEx = excludeMatchConfigExists ? new Regex(interfaceMatch.ExcludeMatch) : null;

                var baseTypeNameConfigExists = !string.IsNullOrEmpty(interfaceMatch.BaseTypeName);
                if (baseTypeNameConfigExists)
                {
                    var baseType = TypeService.GetTypeDefinition(interfaceMatch.BaseTypeName);
                    AddInterfaceNode(baseType);
                }

                foreach (var type in TypeService.Types)
                {
                    var isMatch = matchRegEx != null && matchRegEx.IsMatch(type.FullName)
                        && (!excludeMatchConfigExists || !excludeMatchRegEx.IsMatch(type.FullName));

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

            var baseInterfaceNode = InterfaceNode;

            var baseType = typeDefinition.BaseType;
            if (baseType.IsGenericInstance)
            {
                var genericBaseType = baseType as GenericInstanceType;
                var elementType = genericBaseType?.ElementType as TypeDefinition;

                if (elementType != null && elementType != typeDefinition)
                    baseInterfaceNode = AddInterfaceNode(elementType);
            }
            else
            {
                var baseClass = typeDefinition.BaseType as TypeDefinition;
                var isBaseClassNotObject = baseClass != null && baseClass.FullName != "System.Object";

                if (isBaseClassNotObject)
                    baseInterfaceNode = AddInterfaceNode(baseClass);
            }

            var things = GetMembers(typeDefinition)
                .Where(m => !m.CSharpType.IsGenericParameter);

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
                string implementsString = null;
                string extendsString = null;

                var iHaveGenericParameters = typeDefinition.HasGenericParameters;
                if (iHaveGenericParameters)
                {
                    var genericParameters = typeDefinition.GenericParameters
                        .Select(p => p.Name);

                    implementsString = WrapInAngledBrackets(string.Join(", ", genericParameters));
                }

                var hasBaseClass = interfaceNode.BaseInterface?.TypeDefinition != null;
                var baseTypeName = CleanName(interfaceNode.BaseInterface?.TypeDefinition?.Name);

                if (hasBaseClass)
                {
                    var iHaveGenericArguments = typeDefinition.BaseType.IsGenericInstance;
                    if (iHaveGenericArguments)
                    {
                        var baseTypeInstance = typeDefinition.BaseType as GenericInstanceType;
                        var genericArguments = baseTypeInstance.GenericArguments
                            .Select(p => p.IsGenericParameter
                                ? p.Name
                                : TypeService.GetTypeScriptType(p.GetElementType(), p.Name).TypeName);

                        extendsString = WrapInAngledBrackets(string.Join(", ", genericArguments));
                    }

                    var baseTypeHasGenericParameters = typeDefinition.BaseType.HasGenericParameters;
                    if (baseTypeHasGenericParameters)
                    {
                        var genericParameters = typeDefinition.BaseType.GenericParameters
                            .Select(p => p.Name);

                        extendsString = WrapInAngledBrackets(string.Join(", ", genericParameters));
                    }
                }

                var interfaceExtendsString = hasBaseClass
                    ? $" extends I{baseTypeName}{extendsString}"
                    : string.Empty;

                var classExtendsString = hasBaseClass
                    ? $" extends {baseTypeName}{extendsString}"
                    : string.Empty;

                var blockTypeName = CleanName(typeDefinition.Name);

                var classImplementsString =
                    $" implements I{blockTypeName}{implementsString}, {Config.EndpointsNamespace}.{nameof(IHaveQueryParams)}";

                var parameterOrInstanceString = iHaveGenericParameters
                    ? implementsString
                    : string.Empty;

                var interfaceBlock = interfacesBlock
                    .AddAndUseBlock($"export interface I{blockTypeName}{parameterOrInstanceString}{interfaceExtendsString}");

                var classBlock = interfacesBlock
                    .AddAndUseBlock($"export class {blockTypeName}{parameterOrInstanceString}{classExtendsString}{classImplementsString}");

                var things = GetMembers(typeDefinition);

                foreach (var thing in things)
                {
                    var interfaceName = string.Empty;
                    var typeName = string.Empty;

                    if (thing.CSharpType.IsGenericParameter)
                    {
                        typeName = interfaceName = thing.CSharpType.GenericParameterName;
                    }
                    else
                    {
                        var thingType = thing.CSharpType.TypeDefinition;
                        var typeScriptType = TypeService.GetTypeScriptType(thingType, thing.Name);

                        interfaceName = typeScriptType.InterfaceName;
                        typeName = typeScriptType.TypeName;
                    }

                    var thingName = Config.InterfaceMembersInCamelCase
                        ? Helpers.ToCamelCaseFromPascalCase(thing.Name)
                        : thing.Name;

                    var collectionString = thing.CSharpType.IsCollection ? "[]" : string.Empty;

                    interfaceBlock
                        .AddStatement($"{thingName}: {interfaceName}{collectionString};");

                    classBlock
                        .AddStatement($"{thingName}: {typeName}{collectionString};");
                }

                if (hasBaseClass)
                {
                    classBlock
                       .AddAndUseBlock("constructor()")
                       .AddStatement("super();");
                }

                classBlock
                    .AddAndUseBlock("getQueryParams()")
                    .AddStatement("return this;");
            }

            foreach (var derivedInterfaceNode in interfaceNode.DerivedInterfaces)
                WriteInterfaces(interfacesBlock, derivedInterfaceNode);
        }

        private string CleanName(string dirtyName)
        {
            return string.IsNullOrEmpty(dirtyName)
                ? string.Empty
                : genericNameRegEx.Replace(dirtyName, "$1Generic$3");
        }

        private string WrapInAngledBrackets(string genericString)
        {
            return string.IsNullOrEmpty(genericString)
                ? string.Empty
                : $"<{genericString}>";
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
                .Where(t => t.CSharpType.IsGenericParameter || t.CSharpType.TypeDefinition != null)
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