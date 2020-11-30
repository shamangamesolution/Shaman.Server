using Shaman.Common.Utils.Exceptions;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Shopping
{
    public class Receipt : EntityBase
    {
        public bool Success { get; set; }
        public int PayedForId { get; set; }
        public StoreExceptionCode ErrorCode { get; set; }

        public static string BuyUnsuccessfulErrorMessage = "Buy unsuccessful, see receipt details"; 
        
        public Receipt()
        {
            ErrorCode = StoreExceptionCode.OK;
            Success = false;
        }
        
        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(this.Success);
            serializer.Write(this.PayedForId);
            serializer.Write((byte)this.ErrorCode);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            Success = serializer.ReadBool();
            PayedForId = serializer.ReadInt();
            ErrorCode = (StoreExceptionCode)serializer.ReadByte();
        }
    }
}