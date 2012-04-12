﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radial.Cache
{
    /// <summary>
    /// Cache interface.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set(string key, object value);
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <typeparam name="T">The type of cache value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set<T>(string key, T value);
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="cacheSeconds">The cache holding seconds.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set(string key, object value, int cacheSeconds);
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="ts">The cache holding time.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set(string key, object value, TimeSpan ts);
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <typeparam name="T">The type of cache value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="cacheSeconds">The cache holding seconds.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set<T>(string key, T value, int cacheSeconds);
        /// <summary>
        /// Set cache data.
        /// </summary>
        /// <typeparam name="T">The type of cache value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="ts">The cache holding time.</param>
        /// <remarks>The cache will be discarding only if memory lack or explicitly removed.</remarks>
        void Set<T>(string key, T value, TimeSpan ts);
        /// <summary>
        /// Retrieve cached data.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>If there has matched data, return the cached object, otherwise return null.</returns>
        object Get(string key);
        /// <summary>
        /// Retrieve cached data.
        /// </summary>
        /// <typeparam name="T">The type of cache value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>If there has matched data, return the cached object, otherwise return null.</returns>
        T Get<T>(string key);
        /// <summary>
        /// Retrieve cached data.
        /// </summary>
        /// <param name="keys">The cache keys.</param>
        /// <returns>If there has matched data, return the cached objects, otherwise return an empty array.</returns>
        object[] Gets(string[] keys);
        /// <summary>
        /// Retrieve cached data.
        /// </summary>
        /// <typeparam name="T">The type of cache value.</typeparam>
        /// <param name="keys">The cache keys.</param>
        /// <returns>If there has matched data, return the cached objects, otherwise return an empty array.</returns>
        T[] Gets<T>(string[] keys);
        /// <summary>
        /// Remove cache key and its value.
        /// </summary>
        /// <param name="key">The cache key.</param>
        void Remove(string key);
        /// <summary>
        ///  Clear cache.
        /// </summary>
        void Clear();
    }
}
