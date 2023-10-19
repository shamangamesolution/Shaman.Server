namespace Shaman.Serialization.Messages.Http
{
    public abstract class HttpResponseBase : ISerializable
    {
        public ResultCode ResultCode { get; set; }

        public string Message { get; set; }

        public HttpResponseBase()
        {
            this.ResultCode = ResultCode.OK;
        }

        public bool Success => this.ResultCode == ResultCode.OK;

        protected abstract void SerializeResponseBody(ITypeWriter typeWriter);

        protected abstract void DeserializeResponseBody(ITypeReader typeReader);

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte) this.ResultCode);
            typeWriter.Write(this.Message);
            if (this.ResultCode != ResultCode.OK)
                return;
            this.SerializeResponseBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            ResultCode = (ResultCode) typeReader.ReadByte();
            Message = typeReader.ReadString();
            if (ResultCode != ResultCode.OK)
                return;
            DeserializeResponseBody(typeReader);
        }

        public void SetError(string message)
        {
            ResultCode = ResultCode.RequestProcessingError;
            Message = message;
        }
    }
}