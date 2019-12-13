namespace Shaman.Messages.General.DTO.Requests
{
    public struct BackEndEndpoints
    {
        public const string ValidateSessionId = "Region/ValidateSessionId";
        public const string GetAuthToken = "Region/GetAuthToken";
        public const string IsOnService = "Region/IsOnService";
        
        #region router
        public const string GetBundleUri = "Server/GetBundleUri";
        public const string ActualizeServer = "Server/ActualizeServer";
        public const string GetServerInfoList = "Server/GetServerInfoList";
        #endregion
    }
}
