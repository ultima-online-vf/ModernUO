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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        private static bool ContainsInterface(ITypeSymbol symbol, ISymbol interfaceSymbol) =>
            symbol?.AllInterfaces.Any(i => i.Equals(interfaceSymbol, SymbolEqualityComparer.Default)) ?? false;

        private static string GenerateSerializationPartialClass(
            INamedTypeSymbol classSymbol,
            List<IFieldSymbol> fields,
            ISymbol serializableEntityAttribute,
            ISymbol serializableFieldAttribute,
            ISymbol serializableInterfaceAttribute,
            GeneratorExecutionContext context
        )
        {
            // This is a class symbol if the containing symbol is the namespace
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null;
            }

            // If we have a parent that is or derives from ISerializable, then we are in override
            var isOverride = ContainsInterface(classSymbol.BaseType, serializableInterfaceAttribute);

            if (!isOverride && !ContainsInterface(classSymbol, serializableInterfaceAttribute))
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
                var hasAttribute = fieldSymbol.GetAttributes()
                    .Any(
                        attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttribute)
                    );

                if (!hasAttribute)
                {
                    continue;
                }

                source.GenerateProperty(fieldSymbol, serializableFieldAttribute);
            }

            source.GenerateSerializeMethod(fields, version);
            source.AppendLine();
            source.GenerateDeserializeMethod(fields, version);

            source.GenerateClassEnd();

            return source.ToString();
        }
    }
}
