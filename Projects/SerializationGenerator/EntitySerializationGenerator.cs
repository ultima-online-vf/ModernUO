/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2021 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: EntityJsonGenerator.cs                                          *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SerializationGenerator
{
    [Generator]
    public class EntitySerializationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }
#endif

            context.RegisterForPostInitialization(i =>
            {
                SerializerSyntaxReceiver.AttributeTypes.Add("Server.SerializableAttribute");
                SerializerSyntaxReceiver.AttributeTypes.Add("Server.SerializableFieldAttribute");
            });

            context.RegisterForSyntaxNotifications(() => new SerializerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SerializerSyntaxReceiver receiver)
            {
                return;
            }

            var serializableEntityAttribute = context.Compilation.GetTypeByMetadataName("Server.SerializableAttribute");
            var serializableFieldAttribute = context.Compilation.GetTypeByMetadataName("Server.SerializableFieldAttribute");
            var serializableInterfaceAttribute = context.Compilation.GetTypeByMetadataName("Server.ISerializable");

            foreach (IGrouping<ISymbol, IFieldSymbol> group in receiver.Fields.GroupBy(f => f.ContainingType, SymbolEqualityComparer.Default))
            {
                string classSource = GenerateSerializationPartialClass(
                    group.Key as INamedTypeSymbol,
                    group.ToList(),
                    serializableEntityAttribute,
                    serializableFieldAttribute,
                    serializableInterfaceAttribute,
                    context
                );

                if (classSource != null)
                {
                    context.AddSource($"{group.Key.Name}.Serialization.cs", SourceText.From(classSource, Encoding.UTF8));
                }
            }
        }

        private static string GenerateSerializationPartialClass(
            INamedTypeSymbol classSymbol,
            List<IFieldSymbol> fields,
            ISymbol serializableEntityAttribute,
            ISymbol serializableFieldAttribute,
            ISymbol serializableInterfaceAttribute,
            GeneratorExecutionContext context
        )
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null;
            }

            // Require ISerializable (for now)
            if (!classSymbol.AllInterfaces.Any(i => i.Equals(serializableInterfaceAttribute, SymbolEqualityComparer.Default)))
            {
                return null;
            }

            var versionValue = classSymbol.GetAttributes()
                .FirstOrDefault(
                    attr => attr.AttributeClass?.Equals(serializableEntityAttribute, SymbolEqualityComparer.Default) ?? false
                )
                ?.ConstructorArguments.FirstOrDefault()
                .Value;

            if (versionValue == null)
            {
                return null; // We don't have the attribute
            }

            var version = int.Parse(versionValue.ToString());

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            StringBuilder source = new StringBuilder();

            GenerateClass(source, namespaceName, classSymbol.Name, version);

            foreach (IFieldSymbol fieldSymbol in fields)
            {
                GenerateProperty(source, fieldSymbol, serializableFieldAttribute);
            }

            GenerateSerializeMethod(source, fields, version);
            source.AppendLine();
            GenerateDeserializeMethod(source, fields, version);

            source.AppendLine(@"    }
}
");

            return source.ToString();
        }

        private static void GenerateClass(StringBuilder source, string namespaceName, string className, int version)
        {
            source.AppendLine($@"/// <auto-generated />
using System;
using System.Collections.Generic;
using System.IO;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        private const int _version = {version};

        public {className}(Serial serial) : base(serial)
        {{
        }}
");
        }

        private static void GenerateProperty(
            StringBuilder source,
            IFieldSymbol fieldSymbol,
            ISymbol serializableFieldAttribute
        )
        {
            var hasAttribute = fieldSymbol.GetAttributes()
                .Any(
                    attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttribute)
                );

            if (!hasAttribute)
            {
                return;
            }

            // get the name and type of the field
            var fieldName = fieldSymbol.Name;
            var fieldType = fieldSymbol.Type;

            var propertyName = fieldName;

            if (propertyName.StartsWith("m_", StringComparison.OrdinalIgnoreCase))
            {
                propertyName = propertyName.Substring(2);
            }
            else if (propertyName.StartsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                propertyName = propertyName.Substring(1);
            }

            propertyName = propertyName.Dehumanize();

            source.AppendLine($@"        public {fieldType} {propertyName}
        {{
            get => {fieldName};
            set
            {{
                if (value != {fieldName})
                {{
                    MarkDirtyg();
                    {fieldName} = value;
                }}
            }}
        }}
");
        }

        private static void GenerateSerializeMethod(StringBuilder source, List<IFieldSymbol> fields, int version)
        {
            // TODO: Detect if thsi should be an override
            source.AppendLine($@"        public override void Serialize(IGenericWriter writer)
        {{
            writer.WriteEncodedInt({version});
");

            source.AppendLine(@"        }");
        }

        private static void GenerateDeserializeMethod(StringBuilder source, List<IFieldSymbol> fields, int version)
        {
            // TODO: Detect if this should be an override
            source.AppendLine($@"        public override void Deserialize(IGenericReader reader)
        {{

");

            source.AppendLine(@"        }");
        }
    }
}
