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
using System.Collections.ObjectModel;
using System.Linq;
using Misakai.Bson;
using Misakai.Bson.Serialization.Attributes;
using Misakai.Mongo.Support;

namespace Misakai.Mongo
{
    /// <summary>
    /// Represents the details of a write error for a particular request.
    /// </summary>
    [Serializable]
    public class BulkWriteError : WriteError
    {
        // private fields
        private readonly int _index;

        // constructors
        internal BulkWriteError(int index, int code, string message, BsonDocument details)
            : base(code, message, details)
        {
            _index = index;
        }

        // public properties
        /// <summary>
        /// Gets the index of the request that had an error.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index
        {
            get { return _index; }
        }

        // internal static methods
        internal static BulkWriteError FromCore(Core.Operations.BulkWriteOperationError error)
        {
            return new BulkWriteError(error.Index, error.Code, error.Message, error.Details);
        }

        // internal methods
        internal BulkWriteError WithMappedIndex(IndexMap indexMap)
        {
            var mappedIndex = indexMap.Map(_index);
            return (_index == mappedIndex) ? this : new BulkWriteError(mappedIndex, Code, Message, Details);
        }
    }
}

