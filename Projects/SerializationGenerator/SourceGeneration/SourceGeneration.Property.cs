/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2021 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: SourceGeneration.Property.cs                                    *
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
using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace SerializationGenerator
{
    public static partial class SourceGeneration
    {
        public static void GenerateProperty(
            this StringBuilder source,
            IFieldSymbol fieldSymbol,
            ISymbol serializableFieldAttribute
        )
        {
            var allAttributes = fieldSymbol.GetAttributes();

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

            foreach (var attr in allAttributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, serializableFieldAttribute))
                {
                    continue;
                }

                if (attr.AttributeClass == null)
                {
                    continue;
                }

                source.GenerateAttribute(attr);
            }

            source.AppendLine($@"        public {fieldType} {propertyName}
        {{
            get => {fieldName};
            set
            {{
                if (value != {fieldName})
                {{
                    MarkDirty();
                    {fieldName} = value;
                }}
            }}
        }}
");
        }
    }
}
