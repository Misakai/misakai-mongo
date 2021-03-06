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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Misakai.Mongo.Core.Misc;
using Misakai.Mongo.Core.WireProtocol.Messages.Encoders;

namespace Misakai.Mongo.Core.WireProtocol.Messages
{
    public class KillCursorsMessage : RequestMessage
    {
        // fields
        private readonly List<long> _cursorIds;

        // constructors
        public KillCursorsMessage(
            int requestId,
            IEnumerable<long> cursorIds)
            : base(requestId)
        {
            _cursorIds = Ensure.IsNotNull(cursorIds, "cursorIds").ToList();
        }

        // properties
        public IReadOnlyList<long> CursorIds
        {
            get { return _cursorIds; }
        }

        // methods
        public new IMessageEncoder<KillCursorsMessage> GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetKillCursorsMessageEncoder();
        }

        protected override IMessageEncoder GetNonGenericEncoder(IMessageEncoderFactory encoderFactory)
        {
            return GetEncoder(encoderFactory);
        }
    }
}
