using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.Helpers
{
    public static class UpdateInfoHelper
    {
        public static List<List<T>> Split<T>(List<T> collection, int size)
        {
            var chunks = new List<List<T>>();
            var chunkCount = collection.Count() / size;
        
            if (collection.Count % size > 0)
                chunkCount++;
        
            for (var i = 0; i < chunkCount; i++)
                chunks.Add(collection.Skip(i * size).Take(size).ToList());
        
            return chunks;
        }
        
        public static List<ChangeSet> Split(ChangeSet changeSet, int parts)
        {
            var result = new List<ChangeSet>();
            int counter = 0;

            var byteDictKeysChunks = changeSet.ByteChanges.GroupBy(x => counter++ % parts)
                //.Select(g => g.ToDictionary(h => h.Key, h => h.Value))
                .ToList();
            var nullableByteDictKeys = changeSet.NullableByteChanges.GroupBy(x => counter++ % parts)
                .Select(g => g.ToDictionary(h => h.Key, h => h.Value))
                .ToList();
            var intDictKeys = changeSet.IntChanges.GroupBy(x => counter++ % parts)
                .Select(g => g.ToDictionary(h => h.Key, h => h.Value))
                .ToList();
            var nullableIntDictKeys = changeSet.NullableIntChanges.GroupBy(x => counter++ % parts)
                .Select(g => g.ToDictionary(h => h.Key, h => h.Value))
                .ToList();
            var nullableFloatDictKeys = changeSet.NullableFloatChanges.GroupBy(x => counter++ % parts)
                .Select(g => g.ToDictionary(h => h.Key, h => h.Value))
                .ToList();

            for(int i=0;i<parts;i++)
            {
                var newChangeSet = new ChangeSet();
                if (byteDictKeysChunks.Count() > i)
                    newChangeSet.ByteChanges = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>>(byteDictKeysChunks[i]);
                if (nullableByteDictKeys.Count() > i)
                    newChangeSet.NullableByteChanges = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>>(nullableByteDictKeys[i]);
                if (intDictKeys.Count() > i)
                    newChangeSet.IntChanges = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int>>(intDictKeys[i]);
                if (nullableIntDictKeys.Count() > i)
                    newChangeSet.NullableIntChanges = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>>(nullableIntDictKeys[i]);
                if (nullableFloatDictKeys.Count() > i)
                    newChangeSet.NullableFloatChanges = new ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>>(nullableFloatDictKeys[i]);
                result.Add(newChangeSet);
            }
            
            return result;
        }
        
        public static List<UpdatedInfo> Split(UpdatedInfo updateInfo, int minSize)
        {
            var result = new List<UpdatedInfo>();
            var currentSIze = updateInfo.GetSizeInBytes();
            var parts = (currentSIze / minSize) + 1;

            if (parts == 1)
            {
                result.Add(updateInfo);
            }
            else
            {
                var changeSets = Split(updateInfo.ChangeSet, parts);
                foreach(var changeSet in changeSets)
                    result.Add(new UpdatedInfo(changeSet, updateInfo.Revision));
            }
            
            return result;
        }
    }
}