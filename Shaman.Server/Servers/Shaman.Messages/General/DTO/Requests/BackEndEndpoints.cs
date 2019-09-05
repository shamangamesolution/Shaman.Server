namespace Shaman.Messages.General.DTO.Requests
{
    public struct BackEndEndpoints
    {
        public const string GetMatchmakers = "Server/GetMatchmakers";
        public const string ValidateSessionId = "Region/ValidateSessionId";
        public const string GetAuthToken = "Region/GetAuthToken";
        public const string IsOnService = "Region/IsOnService";
        public const string Initialization = "LoadingScreen/Initialization";
        public const string ActualizeMatchMaker = "Server/ActualizeMatchMaker";
        public const string GetBackendsList = "Server/GetBackendsList";
        public const string GetStorageVersion = "Region/GetStorageVersion";
        public const string GetStorage = "Storage/GetStorage";
        public const string GetNotCompressedStorage = "Storage/GetNotCompressedStorage";
    }
}
