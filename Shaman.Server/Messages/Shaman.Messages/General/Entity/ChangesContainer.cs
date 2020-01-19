using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Shaman.Messages.General.Entity
{
    public class InsertedInfo<T> : EntityBase
        where T : DataLightBase, new()
    {
        public List<T> InsertedItems { get; set; }
        public int Revision { get; set; }

        public InsertedInfo()
        {
            
        }
        
        public InsertedInfo(List<T> insertedItems, int revision)
        {
            InsertedItems = insertedItems;
            Revision = revision;
        }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(InsertedItems);
            typeWriter.Write(Revision);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            InsertedItems = typeReader.ReadList<T>();
            Revision = typeReader.ReadInt();
        }
    }

    public class DeletedInfo : EntityBase
    {
        public HashSet<int> DeletedItems { get; set; }
        public int Revision { get; set; }

        public DeletedInfo()
        {
            
        }
        
        public DeletedInfo(HashSet<int> deletedItems, int revision)
        {
            DeletedItems = deletedItems;
            Revision = revision;
        }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(DeletedItems);
            typeWriter.Write(Revision);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            DeletedItems = typeReader.ReadIntHashSet();
            Revision = typeReader.ReadInt();
        }
    }
    
    public class UpdatedInfo : EntityBase
    {
        public ChangeSet ChangeSet { get; set; }
        public int Revision { get; set; }
        
        public UpdatedInfo(ChangeSet set, int revision)
        {
            ChangeSet = set;
            Revision = revision;
        }

        public UpdatedInfo()
        {
            
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntity(ChangeSet);
            typeWriter.Write(Revision);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            ChangeSet = typeReader.ReadEntity<ChangeSet>();
            Revision = typeReader.ReadInt();
        }
    }

    public class ChangeSet : EntityBase
    {
        public ConcurrentDictionary<int, ConcurrentDictionary<byte, int>> IntChanges =
            new ConcurrentDictionary<int, ConcurrentDictionary<byte, int>>();
        public ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>> NullableIntChanges =
            new ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>>();
        public ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>> ByteChanges =
            new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>>();
        public ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>> NullableByteChanges =
            new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>>();
        public ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>> NullableFloatChanges =
            new ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>>();

        public bool IsEmpty()
        {
            return IntChanges.IsEmpty && NullableIntChanges.IsEmpty && ByteChanges.IsEmpty &&
                   NullableByteChanges.IsEmpty && NullableFloatChanges.IsEmpty;
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int fieldValue)
        {
            IntChanges.TryAdd(objectIndex, new ConcurrentDictionary<byte, int>());
            IntChanges[objectIndex].AddOrUpdate(fieldIndex, fieldValue, (key, oldValue) => fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int? fieldValue)
        {
            NullableIntChanges.TryAdd(objectIndex, new ConcurrentDictionary<byte, int?>());
            NullableIntChanges[objectIndex].AddOrUpdate(fieldIndex, fieldValue, (key, oldValue) => fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte fieldValue)
        {
            ByteChanges.TryAdd(objectIndex, new ConcurrentDictionary<byte, byte>());
            ByteChanges[objectIndex].AddOrUpdate(fieldIndex, fieldValue, (key, oldValue) => fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte? fieldValue)
        {
            NullableByteChanges.TryAdd(objectIndex, new ConcurrentDictionary<byte, byte?>());
            NullableByteChanges[objectIndex].AddOrUpdate(fieldIndex, fieldValue, (key, oldValue) => fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, float? fieldValue)
        {
            NullableFloatChanges.TryAdd(objectIndex, new ConcurrentDictionary<byte, float?>());
            NullableFloatChanges[objectIndex].AddOrUpdate(fieldIndex, fieldValue, (key, oldValue) => fieldValue);
        }
        
        public void Clear()
        {
            IntChanges.Clear();
            NullableIntChanges.Clear();
            ByteChanges.Clear();
            NullableByteChanges.Clear();
            NullableFloatChanges.Clear();
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(IntChanges);
            typeWriter.Write(NullableIntChanges);
            typeWriter.Write(ByteChanges);
            typeWriter.Write(NullableByteChanges);
            typeWriter.Write(NullableFloatChanges);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            IntChanges = typeReader.ReadIntFieldDictionary();
            NullableIntChanges = typeReader.ReadNullableIntFieldDictionary();
            ByteChanges = typeReader.ReadByteFieldDictionary();
            NullableByteChanges = typeReader.ReadNullableByteFieldDictionary();
            NullableFloatChanges = typeReader.ReadNullableFloatFieldDictionary();
        }
    }

    public class ChangesContainerInfo<T> : EntityBase
        where T : DataLightBase, new()
    {
        public ChangesContainer<T> ChangesContainer { get; set; }
        public int Revision { get; set; }

        public ChangesContainerInfo(ChangesContainer<T> changesContainer, int revision)
        {
            ChangesContainer = changesContainer;
            Revision = revision;
        }
        
        public UpdatedInfo GetUpdatedInfo()
        {
            return ChangesContainer.GetUpdatedInfo(Revision);
        }

        public InsertedInfo<T> GetInsertedInfo()
        {
            return ChangesContainer.GetInsertedInfo(Revision);
        }

        public DeletedInfo GetDeletedInfo()
        {
            return ChangesContainer.GetDeletedInfo(Revision);
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            throw new System.NotImplementedException();
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class ChangesContainer<T> : EntityBase
        where T: DataLightBase, new()
    {
        public ChangeSet ChangeSet { get; set; }
        public List<T> InsertedValues;
        public HashSet<int> DeletedValues;
        private SortedSet<int> _insertedAndDeletedIds;

        private bool _isEmpty = true;

        public ChangesContainer()
        {
            _isEmpty = true;
            ChangeSet = new ChangeSet();
            InsertedValues = new List<T>();
            DeletedValues = new HashSet<int>();
            _insertedAndDeletedIds = new SortedSet<int>();
        }

        public UpdatedInfo GetUpdatedInfo(int revision)
        {
            return new UpdatedInfo(ChangeSet, revision);
        }

        public InsertedInfo<T> GetInsertedInfo(int revision)
        {
            return new InsertedInfo<T>(InsertedValues, revision);
        }

        public DeletedInfo GetDeletedInfo(int revision)
        {
            return new DeletedInfo(DeletedValues, revision);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int fieldValue)
        {
            if (_insertedAndDeletedIds.Contains(objectIndex))
                return;
            _isEmpty = false;
            ChangeSet.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int? fieldValue)
        {
            if (_insertedAndDeletedIds.Contains(objectIndex))
                return;
            _isEmpty = false;
            ChangeSet.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte fieldValue)
        {
            if (_insertedAndDeletedIds.Contains(objectIndex))
                return;
            _isEmpty = false;
            ChangeSet.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte? fieldValue)
        {
            if (_insertedAndDeletedIds.Contains(objectIndex))
                return;
            _isEmpty = false;
            ChangeSet.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, float? fieldValue)
        {
            if (_insertedAndDeletedIds.Contains(objectIndex))
                return;
            _isEmpty = false;
            ChangeSet.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackInsert(T newItem)
        {
            if (_insertedAndDeletedIds.Contains(newItem.Index))
                return;
            _isEmpty = false;
            InsertedValues.Add(newItem);
            _insertedAndDeletedIds.Add(newItem.Index);
        }

        public void TrackDelete(int id)
        {
            _isEmpty = false;
            InsertedValues.RemoveAll(i => i.Index == id);
            DeletedValues.Add(id);
            _insertedAndDeletedIds.Add(id);
        }

        public bool IsEmpty()
        {
            return _isEmpty;
        }
        
        public void Flush()
        {
            _isEmpty = true;
            ChangeSet.Clear();
            InsertedValues.Clear();
            DeletedValues.Clear();
            _insertedAndDeletedIds.Clear();
        }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntity(ChangeSet);
            typeWriter.Write(DeletedValues);
            typeWriter.WriteList(InsertedValues);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            ChangeSet = typeReader.ReadEntity<ChangeSet>();
            DeletedValues = typeReader.ReadIntHashSet();
            InsertedValues = typeReader.ReadList<T>();
        }
    }
}