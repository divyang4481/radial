﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radial.Serialization
{
    /// <summary>
    /// Json serializer.
    /// </summary>
    public static class JsonSerializer
    {

        /// <summary>
        /// Serialize to json string.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="o">The object.</param>
        /// <returns>
        /// Json string.
        /// </returns>
        public static string Serialize<T>(T o)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }

        /// <summary>
        /// Serialize to json string.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>Json string.</returns>
        public static string Serialize(object o)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }

        /// <summary>
        /// Deserialize from json string.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="json">The json string.</param>
        /// <returns>
        /// Object instance.
        /// </returns>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                json = string.Empty;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Deserialize from json string.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>Object instance.</returns>
        public static object Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                json = string.Empty;
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }
    }
}
