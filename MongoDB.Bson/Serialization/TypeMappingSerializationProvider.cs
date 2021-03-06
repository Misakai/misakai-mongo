﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Concurrent;

namespace Misakai.Bson.Serialization
{
    /// <summary>
    /// Represents a serialization provider based on a mapping from value types to serializer types.
    /// </summary>
    public sealed class TypeMappingSerializationProvider : BsonSerializationProviderBase
    {
        // private fields
        private readonly ConcurrentDictionary<Type, Type> _serializerTypes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMappingSerializationProvider"/> class.
        /// </summary>
        public TypeMappingSerializationProvider()
        {
            _serializerTypes = new ConcurrentDictionary<Type, Type>();
        }

        // public methods
        /// <summary>
        /// Gets the serializer for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The serializer.
        /// </returns>
        public override IBsonSerializer GetSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsGenericType && type.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }

            Type serializerType;
            if (_serializerTypes.TryGetValue(type, out serializerType))
            {
                return CreateSerializer(serializerType);
            }

            if (type.IsGenericType && !type.ContainsGenericParameters)
            {
                Type serializerTypeDefinition;
                if (_serializerTypes.TryGetValue(type.GetGenericTypeDefinition(), out serializerTypeDefinition))
                {
                    return CreateGenericSerializer(serializerTypeDefinition, type.GetGenericArguments());
                }
            }

            return null;
        }

        /// <summary>
        /// Registers the serializer mapping.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        public void RegisterMapping(Type type, Type serializerType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (serializerType == null)
            {
                throw new ArgumentNullException("serializerType");
            }
            if (type.ContainsGenericParameters != serializerType.ContainsGenericParameters)
            {
                throw new ArgumentException("The type and the serializerType must have the same number of type parameters.");
            }
            if (type.ContainsGenericParameters)
            {
                if (!type.IsGenericTypeDefinition || !serializerType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException("A generic type must either have all or none of the type parameters assigned.");
                }
                if (type.GetGenericArguments().Length != serializerType.GetGenericArguments().Length)
                {
                    throw new ArgumentException("The type and the serializerType must have the same number of type parameters.");
                }
            }

            if (!_serializerTypes.TryAdd(type, serializerType))
            {
                var message = string.Format("There is already a serializer mapping registered for type {0}.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }
        }
    }
}
