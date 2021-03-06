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
using Misakai.Bson.Serialization.Attributes;
using Misakai.Mongo.GeoJsonObjectModel.Serializers;

namespace Misakai.Mongo.GeoJsonObjectModel
{
    /// <summary>
    /// Represents the coordinates of a GeoJson MultiPolygon object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonMultiPolygonCoordinatesSerializer<>))]
    public class GeoJsonMultiPolygonCoordinates<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private ReadOnlyCollection<GeoJsonPolygonCoordinates<TCoordinates>> _polygons;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonMultiPolygonCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="polygons">The polygons.</param>
        /// <exception cref="System.ArgumentNullException">polygons</exception>
        /// <exception cref="System.ArgumentException">One of the polygons is null.;polygons</exception>
        public GeoJsonMultiPolygonCoordinates(IEnumerable<GeoJsonPolygonCoordinates<TCoordinates>> polygons)
        {
            if (polygons == null)
            {
                throw new ArgumentNullException("polygons");
            }
            
            var polygonsArray = polygons.ToArray();
            if (polygonsArray.Contains(null))
            {
                throw new ArgumentException("One of the polygons is null.", "polygons");
            }

            _polygons = new ReadOnlyCollection<GeoJsonPolygonCoordinates<TCoordinates>>(polygonsArray);
        }

        // public properties
        /// <summary>
        /// Gets the Polygons.
        /// </summary>
        /// <value>
        /// The Polygons.
        /// </value>
        public ReadOnlyCollection<GeoJsonPolygonCoordinates<TCoordinates>> Polygons
        {
            get { return _polygons; }
        }
    }
}
