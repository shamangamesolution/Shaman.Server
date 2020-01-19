using Shaman.Messages.General.Entity;

namespace Shaman.Game.Repositories
{
    public abstract class SyncedRepositoryBase<T> : ISyncedRepository<T> 
        where T: DataLightBase, new()
    {
        private int _currentRevision = 0;
        private object _mutex = new object();
        private ChangesContainer<T> _changesContainer = new ChangesContainer<T>();
        
        public ChangesContainer<T> GetChanges()
        {
            return _changesContainer;
        }

        public void FlushChanges()
        {
            _changesContainer.Flush();
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int fieldValue)
        {
            _changesContainer.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, int? fieldValue)
        {
            _changesContainer.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte? fieldValue)
        {
            _changesContainer.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, byte fieldValue)
        {
            _changesContainer.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackChange(int objectIndex, byte fieldIndex, float? fieldValue)
        {
            _changesContainer.TrackChange(objectIndex, fieldIndex, fieldValue);
        }
        
        public void TrackInsert(T newItem)
        {
            _changesContainer.TrackInsert(newItem);
        }

        public void TrackDelete(int id)
        {
            _changesContainer.TrackDelete(id);
        }
    }
}