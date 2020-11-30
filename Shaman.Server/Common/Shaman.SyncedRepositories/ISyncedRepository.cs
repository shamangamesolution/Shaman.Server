using Shaman.Messages.General.Entity;

namespace Shaman.SyncedRepositories
{
    public interface ISyncedRepository<T> where T: DataLightBase, new()
    {
        ChangesContainer<T> GetChanges();
        
        void FlushChanges();

        void TrackChange(int objectIndex, byte fieldIndex, int fieldValue);

        void TrackChange(int objectIndex, byte fieldIndex, int? fieldValue);

        void TrackChange(int objectIndex, byte fieldIndex, byte fieldValue);

        void TrackChange(int objectIndex, byte fieldIndex, byte? fieldValue);

        void TrackChange(int objectIndex, byte fieldIndex, float? fieldValue);

        void TrackInsert(T newItem);
        
        void TrackDelete(int id);
    }
}