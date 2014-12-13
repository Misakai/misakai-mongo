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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Misakai.Bson;
using Misakai.Bson.Serialization.Serializers;
using Misakai.Mongo.Core.Bindings;
using Misakai.Mongo.Core.Misc;
using Misakai.Mongo.Core.WireProtocol.Messages.Encoders;

namespace Misakai.Mongo.Core.Operations
{
    public class ListIndexesOperation : IReadOperation<IReadOnlyList<BsonDocument>>
    {
        #region static
        // static fields
        private static readonly SemanticVersion __serverVersionSupportingListIndexesCommand = new SemanticVersion(2, 7, 6);
        #endregion

        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        public ListIndexesOperation(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, "collectionNamespace");
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        // methods
        public async Task<IReadOnlyList<BsonDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, "binding");

            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            {
                if (channelSource.ServerDescription.Version >= __serverVersionSupportingListIndexesCommand)
                {
                    return await ExecuteUsingCommandAsync(channelSource, binding.ReadPreference, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await ExecuteUsingQueryAsync(channelSource, binding.ReadPreference, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<IReadOnlyList<BsonDocument>> ExecuteUsingCommandAsync(IChannelSourceHandle channelSource, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            var databaseNamespace = _collectionNamespace.DatabaseNamespace;
            var command = new BsonDocument("listIndexes", _collectionNamespace.CollectionName);
            var operation = new ReadCommandOperation<BsonDocument>(databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);

            BsonDocument result;
            try
            {
                result = await operation.ExecuteAsync(channelSource, readPreference, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code == 26)
                {
                    return new List<BsonDocument>();
                }
                throw;
            }

            return result["indexes"].AsBsonArray.Cast<BsonDocument>().ToList();
        }

        private async Task<IReadOnlyList<BsonDocument>> ExecuteUsingQueryAsync(IChannelSourceHandle channelSource, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            var indexes = new List<BsonDocument>();

            var systemIndexesCollection = _collectionNamespace.DatabaseNamespace.SystemIndexesCollection;
            var filter = new BsonDocument("ns", _collectionNamespace.FullName);
            var operation = new FindOperation<BsonDocument>(systemIndexesCollection, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Filter = filter
            };

            var cursor = await operation.ExecuteAsync(channelSource, readPreference, cancellationToken).ConfigureAwait(false);
            while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var batch = cursor.Current;
                foreach (var index in batch)
                {
                    indexes.Add(index);
                }
            }

            return indexes;
        }
    }
}