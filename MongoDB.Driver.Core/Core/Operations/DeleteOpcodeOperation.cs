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
using System.Threading;
using System.Threading.Tasks;
using Misakai.Mongo.Core.Bindings;
using Misakai.Mongo.Core.Connections;
using Misakai.Mongo.Core.Misc;
using Misakai.Mongo.Core.WireProtocol;
using Misakai.Mongo.Core.WireProtocol.Messages.Encoders;

namespace Misakai.Mongo.Core.Operations
{
    public class DeleteOpcodeOperation : IWriteOperation<WriteConcernResult>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly DeleteRequest _request;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        public DeleteOpcodeOperation(
            CollectionNamespace collectionNamespace,
            DeleteRequest request,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, "collectionNamespace");
            _request = Ensure.IsNotNull(request, "request");
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public DeleteRequest Request
        {
            get { return _request; }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, "value"); }
        }

        // methods
        private Task<WriteConcernResult> ExecuteProtocolAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            return channel.DeleteAsync(
                _collectionNamespace,
                _request.Filter,
                _request.Limit != 1,
                _messageEncoderSettings,
                _writeConcern,
                cancellationToken);
        }

        private async Task<WriteConcernResult> ExecuteAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(channel, "channel");

            if (channel.ConnectionDescription.BuildInfoResult.ServerVersion >= new SemanticVersion(2, 6, 0) && _writeConcern.IsAcknowledged)
            {
                var emulator = new DeleteOpcodeOperationEmulator(_collectionNamespace, _request, _messageEncoderSettings)
                {
                    WriteConcern = _writeConcern
                };
                return await emulator.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await ExecuteProtocolAsync(channel, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<WriteConcernResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, "binding");

            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
