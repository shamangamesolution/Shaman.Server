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
        
        //rw.BEGIN
        public const string CreateStorage = "Storage/CreateStorage";
        public const string GetCurrentTournament = "Rating/GetCurrentTournament";
        public const string GetPrevTournament = "Rating/GetPrevTournament";
        public const string GetPrevTournamentId = "Rating/GetPrevTournamentId";
        public const string GetPlayer = "Player/GetPlayer";
        public const string GetClan = "Clan/GetClan";
        public const string GetClans = "Clan/GetClans";
        public const string GetMyRequestsToClans = "Clan/GetMyRequestsToClans";
        public const string GetRequestsToMyClan = "Clan/GetRequestsToMyClan";
        public const string GetPlayerStatus = "Player/GetPlayerStatus2";
        public const string GetMyTournamentPlace = "Rating/GetMyPlace";
        public const string IsPlayerProfileExists = "Player/IsPlayerProfileExists";
        public const string GetServerConfigurations = "Server/GetConfigurations";
        public const string UpdateConfiguration = "Server/UpdateConfiguration";
        public const string GetVersion = "Version/GetVersion";
        public const string GetInvites = "Clan/GetMyInvites";
        public const string CreateChatMessage = "Chat/CreateMessage";
        public const string GetChat = "Chat/GetChat";
        public const string GetPlayerRating = "Player/GetPlayerRating";
        public const string GetPlayerId = "Player/GetPlayerId";
        public const string PingRegion = "Region/PingRegion";
        public const string GetBackEnd = "Region/GetBackends";
        public const string UpdatePlayersCount = "Region/UpdatePlayersCount";
        public const string SlackBotMessageMaster = "Region/SlackBotMessage";
        public const string SlackBotMessageBackend = "Tools/SlackBotMessage";
        public const string GetExternalAccounts = "Region/GetExternalAccounts";
        public const string GetGuestId = "Region/GetGuestId";
        public const string UnlinkAccount = "Region/UnlinkAccount";
        public const string LinkAccount = "Region/LinkAccount";
        public const string MigrateGuestIds = "Player/MigrateGuestIds";
        public const string ChangeBalance = "Tools/ChangeBalance";
        
        #region router
        public const string ActualizeServer = "Server/ActualizeServer";
        public const string GetOnline = "Server/GetOnline";
        public const string GetServerInfoList = "Server/GetServerInfoList";
        #endregion
        
        #region Loading screen
        public const string ChangeName = "LoadingScreen/ChangeName";
        public const string ClaimDailyGoalGift = "LoadingScreen/ClaimDailyGoalGift";
        public const string ClaimUpgradeComplete = "LoadingScreen/ClaimUpgradeComplete";
        public const string ClaimWeaponUpgradeComplete = "LoadingScreen/ClaimWeaponUpgradeComplete";
        public const string GetAccounts = "LoadingScreen/GetAccounts";
        public const string GetBox = "LoadingScreen/GetBox";
        public const string GetDailyGoals = "LoadingScreen/GetDailyGoals";
        public const string InstantUpgradeComplete = "LoadingScreen/InstantUpgradeComplete";
        public const string InstantWeaponUpgradeComplete = "LoadingScreen/InstantWeaponUpgradeComplete";
        public const string IsAdWatchAvailable = "LoadingScreen/IsAdWatchAvailable";
        public const string LinkAccountRequest = "LoadingScreen/LinkAccount";
        public const string OpenBox = "LoadingScreen/OpenBox";
        public const string OpenBoxNew = "LoadingScreen/OpenBoxNew";
        public const string OpenFreeBoxNew = "LoadingScreen/OpenFreeBoxNew";
        public const string PlaceConsumableToSlot = "LoadingScreen/PlaceConsumableToSlot";
        public const string PlaceWeaponToSlot = "LoadingScreen/PlaceWeaponToSlot";
        public const string RemoveWeaponFromSlot = "LoadingScreen/RemoveWeaponFromSlot";
        public const string StartUpgrade = "LoadingScreen/StartUpgrade";
        public const string WeaponUpgrade = "LoadingScreen/WeaponUpgrade";
        public const string TutorialUpdate = "LoadingScreen/TutorialUpdate";
        public const string WatchAd = "LoadingScreen/WatchAd";
        public const string GetTime = "LoadingScreen/GetTimeToEndOfTheDay";
        public const string GiveDailyGifts = "LoadingScreen/GiveDailyGifts";
        public const string WatchAdForUpgrade = "LoadingScreen/WatchAdForUpgrade";
        public const string Buy = "LoadingScreen/Buy";
        public const string GetMinerPrize = "LoadingScreen/GetMinerPrize";
        public const string JoinMarketingEvent = "LoadingScreen/JoinMarketingEvent";
        public const string CompleteAndUpgrade = "LoadingScreen/CompleteAndUpgrade";
        public const string GetBattleRecord = "LoadingScreen/GetBattleRecord";
        public const string StartWorkshopProject = "LoadingScreen/StartWorkshopProject";
        public const string ClaimWorkshopProjectResult = "LoadingScreen/ClaimWorkshopProjectResult";
        public const string InstantCompleteWorkshopProject = "LoadingScreen/InstantCompleteWorkshopProject";
        public const string UpdateWorkshopStatus = "LoadingScreen/UpdateWorkshopStatus";
        public const string BuyMarketItem = "LoadingScreen/BuyMarketItem";
        public const string ExchangeCurrency = "LoadingScreen/ExchangeCurrency";
        public const string GetBoxOpenRecords = "LoadingScreen/GetBoxOpenRecords";
        public const string GetWinners = "LoadingScreen/GetWinners";
        #endregion
        
        #region gameplay screen
        public const string GetGameParameters = "GamePlay/GetGameParameters";
        public const string UpdatePlayerBeforeMatch = "GamePlay/UpdatePlayerBeforeMatch";
        public const string GetBots = "GamePlay/GetBots";
        public const string GiveFightRewards = "GamePlay/GiveFightRewards";
        public const string UpdateDailyGoals = "GamePlay/UpdateDailyGoals";
        public const string GiveLoot = "GamePlay/GiveLoot";
        public const string MarkAsCheater = "GamePlay/MarkAsCheater";
        public const string GetRoomStats = "LoadingScreen/GetRoomStats";
        public const string UpdateAdWatch = "GamePlay/UpdateAdWatch";
        #endregion
        //rw.END
    }
}
