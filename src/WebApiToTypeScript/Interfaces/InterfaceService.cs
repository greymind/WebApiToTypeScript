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

        private List<InterfaceNode> InterfaceNodes { get; }
            = new List<InterfaceNode>();

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

            if (typeDefinition.IsInterface)
                return null;

            interfaceNode = AddInterfaceNode(typeDefinition, InterfaceNode);

            var baseInterfaceNode = InterfaceNode;

            var baseType = typeDefinition.BaseType;
            if (baseType.IsGenericInstance)
            {
                var genericBaseType = baseType as GenericInstanceType;
                var elementType = TypeService.GetTypeDefinition(genericBaseType?.ElementType?.FullName);
                var typeMapping = TypeService.GetTypeMapping("", elementType?.FullName);

                if (elementType != null && elementType != typeDefinition
                    && typeMapping == null)
                {
                    baseInterfaceNode = AddInterfaceNode(elementType);
                }
            }
            else
            {
                var baseClass = TypeService.GetTypeDefinition(typeDefinition.BaseType.FullName);
                var isBaseClassNotObject = baseClass != null && baseClass.FullName != "System.Object";
                var typeMapping = TypeService.GetTypeMapping("", baseClass?.FullName);

                if (isBaseClassNotObject && baseClass != typeDefinition
                    && typeMapping == null)
                {
                    baseInterfaceNode = AddInterfaceNode(baseClass);
                }
            }

            interfaceNode = AdjustBaseClass(interfaceNode, baseInterfaceNode);

            var nestedTypes = typeDefinition.NestedTypes
                .Where(t => !t.IsNestedPrivate);

            foreach (var nestedType in nestedTypes)
            {
                AddInterfaceNode(nestedType);
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

            return interfaceNode;
        }

        public IEnumerable<MemberWithCSharpType> SameNamedDerivedMembers(
            MemberWithCSharpType member,
            InterfaceNode interfaceNode)
        {
            var members = Enumerable.Empty<MemberWithCSharpType>();

            foreach (var derived in interfaceNode.DerivedInterfaces)
            {
                members = members.Concat(
                    GetMembers(derived.TypeDefinition)
                        .Where(m => m.Name == member.Name));

                members = members.Concat(SameNamedDerivedMembers(member, derived));
            }

            return members;
        }

        private void WriteInterfaces(TypeScriptBlock interfacesBlock, InterfaceNode interfaceNode)
        {
            var typeDefinition = interfaceNode.TypeDefinition;
            if (typeDefinition != null)
            {
                WriteInterfaceNode(interfacesBlock, interfaceNode);
            }

            foreach (var derivedInterfaceNode in interfaceNode.DerivedInterfaces.ToList())
                WriteInterfaces(interfacesBlock, derivedInterfaceNode);
        }

        private void WriteInterfaceNode(TypeScriptBlock interfacesBlock, InterfaceNode interfaceNode)
        {
            var typeDefinition = interfaceNode.TypeDefinition;

            string implementsString = null;

            string _interfaceExtendsString = null;
            string _classExtendsString = null;

            var iHaveGenericParameters = typeDefinition.HasGenericParameters;
            if (iHaveGenericParameters)
            {
                var genericParameters = typeDefinition.GenericParameters
                    .Select(p => p.Name);

                implementsString = WrapInAngledBrackets(string.Join(", ", genericParameters));
            }

            var hasBaseClass = interfaceNode.BaseInterface?.TypeDefinition != null;
            var baseTypeName = TypeService.CleanGenericName(interfaceNode.BaseInterface?.TypeDefinition?.Name);
            var things = GetMembers(typeDefinition);

            if (hasBaseClass)
            {
                if (typeDefinition.BaseType is GenericInstanceType baseTypeInstance)
                {
                    var classGenericArguments = baseTypeInstance.GenericArguments
                        .Select(p => p.IsGenericParameter
                            ? p.Name
                            : TypeService.GetTypeScriptType(p.GetElementType(), p.Name).TypeName);

                    var interfaceGenericArguments = baseTypeInstance.GenericArguments
                        .Select(p => p.IsGenericParameter
                            ? p.Name
                            : TypeService.GetTypeScriptType(p.GetElementType(), p.Name).InterfaceName);

                    _classExtendsString = WrapInAngledBrackets(string.Join(", ", classGenericArguments));
                    _interfaceExtendsString = WrapInAngledBrackets(string.Join(", ", interfaceGenericArguments));
                }
                else
                {
                    var baseTypeHasGenericParameters = typeDefinition.BaseType.HasGenericParameters;
                    if (baseTypeHasGenericParameters)
                    {
                        var genericParameters = typeDefinition.BaseType.GenericParameters
                            .Select(p => p.Name);

                        _classExtendsString = _interfaceExtendsString = WrapInAngledBrackets(string.Join(", ", genericParameters));
                    }
                }
            }

            var interfaceExtendsString = hasBaseClass
                ? $" extends I{baseTypeName}{_interfaceExtendsString}"
                : string.Empty;

            var classExtendsString = hasBaseClass
                ? $" extends {baseTypeName}{_classExtendsString}"
                : string.Empty;

            var blockTypeName = TypeService.CleanGenericName(typeDefinition.Name);

            if (!TypeService.IsValidTypeName(blockTypeName))
            {
                LogMessage($"Interface name [{blockTypeName}] of type [{typeDefinition.FullName}] is invalid!");
                return;
            }

            var classImplementsString =
                $" implements I{blockTypeName}{implementsString}";

            var parameterOrInstanceString = iHaveGenericParameters
                ? implementsString
                : string.Empty;

            var interfaceBlock = interfacesBlock
                .AddAndUseBlock($"export interface I{blockTypeName}{parameterOrInstanceString}{interfaceExtendsString}");

            TypeScriptBlock classBlock = null;
            if (Config.GenerateInterfaceClasses)
            {
                classBlock = interfacesBlock
                    .AddAndUseBlock($"export class {blockTypeName}{parameterOrInstanceString}{classExtendsString}{classImplementsString}");
            }

            foreach (var thing in things)
            {
                var thingType = thing.CSharpType.TypeDefinition;

                var union = SameNamedDerivedMembers(thing, interfaceNode)
                    .Select(e => e.CSharpType.TypeDefinition)
                    .ToList();

                union.Add(thingType);

                string interfaceName;
                string typeName;
                if (thing.CSharpType.IsGenericParameter)
                {
                    typeName = interfaceName = thing.CSharpType.GenericParameterName;
                }
                else if (thing.CSharpType.IsGenericInstance)
                {
                    var baseTypeScriptType = TypeService.GetTypeScriptType(thing.CSharpType.TypeDefinition, thing.Name);

                    if (baseTypeScriptType.IsMappedType)
                    {
                        typeName = baseTypeScriptType.TypeName;
                        interfaceName = baseTypeScriptType.InterfaceName;
                    }
                    else
                    {
                        var typeNames = string.Join(", ", thing.CSharpType.GenericArgumentTypes
                            .Select(t =>
                            {
                                var genericArgumentTypeScriptType = TypeService.GetTypeScriptType(t, thing.Name);
                                return genericArgumentTypeScriptType.TypeName;
                            }));

                        var interfaceNames = string.Join(", ", thing.CSharpType.GenericArgumentTypes
                            .Select(t =>
                            {
                                var genericArgumentTypeScriptType = TypeService.GetTypeScriptType(t, thing.Name);
                                return genericArgumentTypeScriptType.InterfaceName;
                            }));

                        typeName = $"{baseTypeScriptType.TypeName}<{typeNames}>";
                        interfaceName = $"{baseTypeScriptType.InterfaceName}<{interfaceNames}>";
                    }
                }
                else
                {
                    if (thingType.IsInterface)
                        continue;

                    if (union.Count == 1)
                    {
                        var typeScriptType = TypeService.GetTypeScriptType(union[0], thing.Name);

                        typeName = typeScriptType.TypeName;
                        interfaceName = typeScriptType.InterfaceName;
                    }
                    else
                    {
                        typeName = string.Join(" | ", union
                            .Select(t =>
                            {
                                var type = TypeService.GetTypeScriptType(t, thing.Name);
                                return type.TypeName;
                            })
                            .Distinct());

                        interfaceName = string.Join(" | ", union
                            .Select(t =>
                            {
                                var type = TypeService.GetTypeScriptType(t, thing.Name);
                                return type.InterfaceName;
                            }).Distinct());
                    }
                }

                var thingName = thing.Name;

                var jsonProperty = thing.CustomAttributes.SingleOrDefault(a => a.AttributeType.Name == "JsonPropertyAttribute");

                if (jsonProperty != null)
                {
                    try
                    {
                        thingName = jsonProperty?.HasProperties ?? false
                            ? jsonProperty.Properties.Single(p => p.Name == "PropertyName").Argument.Value.ToString()
                            : jsonProperty?.HasConstructorArguments ?? false
                                ? jsonProperty.ConstructorArguments.Single().Value.ToString()
                                : thing.Name;
                    }
                    catch
                    {
                        // This is to suppress a assembly load execption which I am unsure of why it is happening
                    }
                }

                if (Config.InterfaceMembersInCamelCase
                    || typeDefinition.CustomAttributes.Any(a => a.AttributeType.Name == Config.InterfaceCamelCaseCustomAttribute))
                {
                    thingName = Helpers.ToCamelCaseFromPascalCase(thingName);
                }

                var collectionString = Helpers.GetCollectionPostfix(thing.CSharpType.CollectionLevel);

                thingName = TypeService.FixIfReservedWord(thingName);

                interfaceBlock
                    .AddStatement($"{thingName}: {interfaceName}{collectionString};");

                if (Config.GenerateInterfaceClasses)
                {
                    classBlock
                        .AddStatement($"{thingName}: {typeName}{collectionString};");
                }
            }

            if (Config.GenerateInterfaceClasses)
            {
                if (hasBaseClass)
                {
                    classBlock
                       .AddAndUseBlock("constructor()")
                       .AddStatement("super();");
                }
            }
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
                .Select(f => new MemberWithCSharpType
                {
                    Name = f.Name,
                    CSharpType = TypeService.GetCSharpType(f.FieldType, f.FullName),
                    CustomAttributes = f.CustomAttributes.ToList()
                });

            var properties = typeDefinition.Properties
                .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && !p.IsSpecialName && !Helpers.HasCustomAttribute(p, "JsonIgnoreAttribute"))
                .Select(p => new MemberWithCSharpType
                {
                    Name = p.Name,
                    CSharpType = TypeService.GetCSharpType(p.PropertyType, p.FullName),
                    CustomAttributes = p.CustomAttributes.ToList()
                });

            return fields.Union(properties)
                .Where(t => t.CSharpType.IsValid)
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

        private InterfaceNode AdjustBaseClass(InterfaceNode interfaceNode, InterfaceNode baseInterfaceNode)
        {
            interfaceNode
                .BaseInterface
                .DerivedInterfaces
                .Remove(interfaceNode);

            interfaceNode.BaseInterface = baseInterfaceNode;

            baseInterfaceNode
                .DerivedInterfaces
                .Add(interfaceNode);

            return interfaceNode;
        }
    }
}