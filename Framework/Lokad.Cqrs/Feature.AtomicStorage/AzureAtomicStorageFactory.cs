﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Linq;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicStorageFactory : IAtomicStorageFactory
    {
        public IAtomicEntityWriter<TKey, TEntity> GetEntityWriter<TKey, TEntity>()
        {
            return new AzureAtomicEntityWriter<TKey, TEntity>(_client, _strategy);
        }

        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
            return new AzureAtomicEntityReader<TKey, TEntity>(_client, _strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new AzureAtomicSingletonReader<TSingleton>(_client, _strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new AzureAtomicSingletonWriter<TSingleton>(_client, _strategy);
        }

        readonly object _initializationLock = new object();
        bool _initialized = false;

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        public void Initialize()
        {
            lock (_initializationLock)
            {
                if (_initialized)
                    return;
                DoInitialize();

                _initialized = true;
            }
        }

        void DoInitialize()
        {
            var types = _strategy.GetEntityTypes();

            var folders = types
                .Select(t => _strategy.GetFolderForEntity(t)).ToSet();

            folders.Add(_strategy.GetFolderForSingleton());

            folders
                .AsParallel()
                .WithDegreeOfParallelism(folders.Count)
                .ForAll(t => _client.GetContainerReference(t).CreateIfNotExist());
        }

        readonly IAzureAtomicStorageStrategy _strategy;
        readonly IAzureClientConfiguration _client;


        public AzureAtomicStorageFactory(IAzureAtomicStorageStrategy strategy, IAzureClientConfiguration client)
        {
            _strategy = strategy;
            _client = client;
        }

        public NuclearStorage CreateSimplifiedStorage(bool dontInitialize = false)
        {
            var storage = new NuclearStorage(this);
            if (!dontInitialize)
            {
                Initialize();
            }
            return storage;
        }
    }
}