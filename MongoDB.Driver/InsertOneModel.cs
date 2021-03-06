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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Misakai.Mongo.Core.Operations;

namespace Misakai.Mongo
{
    /// <summary>
    /// Model for inserting a single document.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    [Serializable]
    public sealed class InsertOneModel<T> : WriteModel<T>
    {
        // fields
        private readonly T _document;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertOneModel{T}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public InsertOneModel(T document)
        {
            _document = document;
        }

        // properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        public T Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public override WriteModelType ModelType
        {
            get { return WriteModelType.InsertOne; }
        }
    }
}
