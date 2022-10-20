﻿using System;
using System.Threading;

namespace KeyedSemaphores
{
    /// <summary>
    ///     Represents a locked keyed semaphore that has already acquired its inner semaphore.
    ///     It is safe to perform any multi-threaded operations while in possession of this object.
    /// </summary>
    public class LockedKeyedSemaphore<TKey> : IDisposable
    {
        private readonly KeyedSemaphoresCollection<TKey> _collection;
        private readonly KeyedSemaphore<TKey> _keyedSemaphore;

        internal LockedKeyedSemaphore(
            KeyedSemaphoresCollection<TKey> collection,
            KeyedSemaphore<TKey> keyedSemaphore)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _keyedSemaphore = keyedSemaphore ?? throw new ArgumentNullException(nameof(keyedSemaphore));
        }

        /// <summary>
        ///     Releases and disposes of the inner <see cref="KeyedSemaphore{TKey}" />
        /// </summary>
        public void Dispose()
        {
            var key = _keyedSemaphore.Key;
            
            while (true)
            {
                if (!Monitor.TryEnter(_keyedSemaphore))
                {
                    continue;
                }
                
                var remainingConsumers = --_keyedSemaphore.Consumers;

                if (remainingConsumers == 0)
                {
                    _collection.Index.TryRemove(key, out _);
                }

                Monitor.Exit(_keyedSemaphore);

                break;
            }

            _keyedSemaphore.SemaphoreSlim.Release();
        }
    }
}
