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

namespace Misakai.Mongo
{
    /// <summary>
    /// The document to return when executing a FindAndModify command.
    /// </summary>
    public enum ReturnDocument
    {
        /// <summary>
        /// Returns the document before the modification.
        /// </summary>
        Before,
        /// <summary>
        /// Returns the document after the modification.
        /// </summary>
        After
    }

    internal static class ReturnDocumentExtensions
    {
        public static Core.Operations.ReturnDocument ToCore(this ReturnDocument returnDocument)
        {
            switch (returnDocument)
            {
                case ReturnDocument.Before:
                    return Core.Operations.ReturnDocument.Before;
                case ReturnDocument.After:
                    return Core.Operations.ReturnDocument.After;
                default:
                    throw new ArgumentException("Unrecognized ReturnDocument.", "returnDocument");
            }
        }
    }
}