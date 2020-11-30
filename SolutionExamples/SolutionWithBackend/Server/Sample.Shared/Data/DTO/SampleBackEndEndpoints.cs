namespace Sample.Shared.Data.DTO
{
    public struct SampleBackEndEndpoints
    {
        public const string ValidateSessionId = "Region/ValidateSessionId";
       
        //master backend
        public const string GetAuthToken = "Region/GetAuthToken";
        public const string IsOnService = "Region/IsOnService";
        
        //loading screen handlers
        public const string Initialization = "LoadingScreen/Initialization";

        //storage
        public const string GetStorageVersion = "Region/GetStorageVersion";
        public const string GetStorage = "Storage/GetStorage";
        public const string LinkExternalAccount = "Main/LinkExternalAccount";
        
        //gameplay
        public const string GetPlayerGameData = "Gameplay/GetPlayerGameData";
    }
}