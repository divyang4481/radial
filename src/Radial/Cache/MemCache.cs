﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching;
using System.Threading;
using Radial.Serialization;

namespace Radial.Cache
{
    /// <summary>
    /// Memcached cache implement class.
    /// </summary>
    public class MemCache : ICache
    {
        static object SyncRoot = new object();
        static MemcachedClient Client=null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemCache"/> class.
        /// </summary>
        public MemCache()
        {
            lock (SyncRoot)
            {
                if (Client == null)
                    Client = new MemcachedClient();
            }
        }

        /// <summary>
        /// Complex type serialization method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        }

        /// <summary>
        /// Complex type deserialization method.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        private static object Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize(Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Retrieve cached data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// If there has matched key, return the cached value, otherwise return null.
        /// </returns>
        public object Get(string key)
        {
            return Deserialize(Client.Get<byte[]>(CacheStatic.NormalizeKey(key)));
        }

        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="cacheSeconds">The cache holding seconds.</param>
        public void Set(string key, object value, int? cacheSeconds = null)
        {
            if (value == null)
                return;


            if (cacheSeconds.HasValue)
                Client.Store(Enyim.Caching.Memcached.StoreMode.Set, CacheStatic.NormalizeKey(key), Serialize(value), TimeSpan.FromSeconds(cacheSeconds.Value));
            else
                Client.Store(Enyim.Caching.Memcached.StoreMode.Set, CacheStatic.NormalizeKey(key), Serialize(value));
        }


        /// <summary>
        /// Remove cache data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        public void Remove(string key)
        {
            Client.Remove(CacheStatic.NormalizeKey(key));
        }
    }
}
