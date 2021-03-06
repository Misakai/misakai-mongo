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
    public class ListCollectionsOperation : IReadOperation<IReadOnlyList<BsonDocument>>
    {
        #region static
        // static fields
        private static readonly SemanticVersion __versionSupportingListCollectionsCommand = new SemanticVersion(2, 7, 6);
        #endregion

        // fields
        private BsonDocument _filter;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        public ListCollectionsOperation(
            DatabaseNamespace databaseNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, "databaseNamespace");
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, "messageEncoderSettings");
        }

        // properties
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
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
                if (channelSource.ServerDescription.Version >= __versionSupportingListCollectionsCommand)
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
            var command = new BsonDocument
            {
                { "listCollections", 1 },
                { "filter", _filter, _filter != null }
            };
            var operation = new ReadCommandOperation<BsonDocument>(_databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
            var response = await operation.ExecuteAsync(channelSource, readPreference, cancellationToken).ConfigureAwait(false);
            return response["collections"].AsBsonArray.Select(value => (BsonDocument)value).ToList();
        }

        private async Task<IReadOnlyList<BsonDocument>> ExecuteUsingQueryAsync(IChannelSourceHandle channelSource, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            // if the filter includes a comparison to the "name" we must convert the value to a full namespace
            var filter = _filter;
            if (filter != null && filter.Contains("name"))
            {
                var value = filter["name"];
                if (!value.IsString)
                {
                    throw new NotSupportedException("Name filter must be a plain string when connected to a server version less than 2.8.");
                }
                filter = (BsonDocument)filter.Clone(); // shallow clone
                filter["name"] = _databaseNamespace.DatabaseName + "." + value;
            }

            var operation = new FindOperation<BsonDocument>(_databaseNamespace.SystemNamespacesCollection, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Filter = filter
            };
            var cursor = await operation.ExecuteAsync(channelSource, readPreference, cancellationToken).ConfigureAwait(false);

            var collections = new List<BsonDocument>();
            var prefix = _databaseNamespace + ".";

            while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var batch = cursor.Current;
                foreach (var collection in batch)
                {
                    var name = (string)collection["name"];
                    if (name.StartsWith(prefix))
                    {
                        var collectionName = name.Substring(prefix.Length);
                        if (!collectionName.Contains('$'))
                        {
                            collection["name"] = collectionName; // replace the full namespace with just the collection name
                            collections.Add(collection);
                        }
                    }
                }
            }

            return collections;
        }
    }
}
