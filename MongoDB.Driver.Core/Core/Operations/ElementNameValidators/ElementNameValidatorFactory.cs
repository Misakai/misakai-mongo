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

using Misakai.Bson.IO;

namespace Misakai.Mongo.Core.Operations.ElementNameValidators
{
    public static class ElementNameValidatorFactory
    {
        public static IElementNameValidator ForUpdateType(UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.Replacement:
                    return CollectionElementNameValidator.Instance;
                case UpdateType.Update:
                    return UpdateElementNameValidator.Instance;
                default:
                    return new UpdateOrReplacementElementNameValidator();
            }
        }
    }
}