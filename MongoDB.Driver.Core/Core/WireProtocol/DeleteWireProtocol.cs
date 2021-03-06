﻿/* Copyright 2013-2014 MongoDB Inc.
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

using Misakai.Bson;
using Misakai.Mongo.Core.Connections;
using Misakai.Mongo.Core.Misc;
using Misakai.Mongo.Core.Operations;
using Misakai.Mongo.Core.WireProtocol.Messages;
using Misakai.Mongo.Core.WireProtocol.Messages.Encoders;

namespace Misakai.Mongo.Core.WireProtocol
{
    public class DeleteWireProtocol : WriteWireProtocolBase
    {
        // fields
        private readonly bool _isMulti;
        private readonly BsonDocument _query;

        // constructors
        public DeleteWireProtocol(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            bool isMulti,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern)
            : base(collectionNamespace, messageEncoderSettings, writeConcern)
        {
            _query = Ensure.IsNotNull(query, "query");
            _isMulti = isMulti;
        }

        // methods
        protected override RequestMessage CreateWriteMessage(IConnection connection)
        {
            return new DeleteMessage(
                RequestMessage.GetNextRequestId(),
                CollectionNamespace,
                _query,
                _isMulti);
        }
    }
}
