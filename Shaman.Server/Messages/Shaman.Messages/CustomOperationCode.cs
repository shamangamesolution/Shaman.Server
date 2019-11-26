namespace Shaman.Messages
{
    public struct CustomOperationCode
    {
        public const ushort Authorization = 10001;
        public const ushort Ping = 10002;
        public const ushort JoinRoom = 10003;        
        public const ushort Test = 10006;
        public const ushort LeaveRoom = 10009;
        public const ushort Disconnect = 10010;
        public const ushort Connect = 10011;
        public const ushort PingRequest = 10012;
        public const ushort Error = 10000;


        //matchmaking.BEGIN
        public const ushort EnterMatchMaking = 11012;
        public const ushort LeaveMatchMaking = 11013;
        public const ushort CreateRoom = 11014;
        public const ushort JoinInfo = 11015;
        public const ushort ServerActualization = 11017;
        public const ushort UpdateRoom = 11018;
        
        //matchmaking.END
        
        //router.BEGIN
        public const ushort ActualizeMatchmaker = 12002;
        public const ushort GetServerInfoList = 12003;
        //router.END
        
        //backend.BEGIN
        public const ushort GetSessionId = 13001;
        public const ushort Initialization = 15001;
        public const ushort GetCurrentStorageVersion = 15003;
        public const ushort GetStorageHttp = 15056;
        public const ushort IsOnServiceHttp = 15057;
        public const ushort GetAuthToken = 15060;


        public const ushort ValidateSessionId = 15109;
        public const ushort GetMatchmakers = 15110;
        public const ushort GetNotCompressedStorage = 15111;
        //backend.END
        
        //RW.operations.BEGIN
        public const ushort GetRobotsCollection = 16001;
        public const ushort GetOrCreatePlayer = 16002;
        public const ushort GetStorage = 16003;
        public const ushort StartUpgrade = 16005;
        public const ushort Buy = 16006;
        public const ushort ClaimUpgadeComplete = 16007;
        public const ushort EndSession = 16008;
        public const ushort PlaceRobotToSlot = 16009;
        public const ushort GetGameParameters = 16010;
        public const ushort UpdatePlayerBeforeMatch = 16011;
        public const ushort InstantFinishUpgrade = 16012;
        public const ushort GiveFightRewards = 16013;
        public const ushort GetAvailableAdWatches = 16014;
        public const ushort AdWatched = 16015;    
        public const ushort UpdateBalance = 16016;
        public const ushort GiftRobotToPlayer = 16017;
        public const ushort IncrementAndGetVersion = 16018;
        public const ushort DecrementFightsForRentedRobot = 16019;
        public const ushort GiveDailyGifts = 16020;
        public const ushort RemoveConsumable = 16021;
        public const ushort StatisticsRequest = 16022;
        public const ushort TutorialRequest = 16023;
        public const ushort RentFromFight = 16024;
        public const ushort GetCurrentTournamentHttp = 16026;
        public const ushort GetPrevTournamentHttp = 16027;
        public const ushort GetPrevTournamentIdHttp = 16028;
        public const ushort GetPlayerHttp = 16030;
		public const ushort OpenBox = 16031;
        public const ushort GetOpenedCards = 16032;
        public const ushort OpenCard = 16033;
        public const ushort GetBox = 16034;
        public const ushort AdWatchedToReduceUpgradeTime = 16035;
        public const ushort UpdateDailyGoal = 16036;
        public const ushort ClaimDailyGoalGift = 16037;
        public const ushort GetMyTournamentPlaceHttp = 16038;
        public const ushort IsPlayerProfileExistsHttp = 16039;
        public const ushort GetServerConfigurationsHttp = 16040;
        public const ushort GetPlayerStatus = 16041;
        public const ushort GetRoomToken = 16042;
        public const ushort GetAccounts = 16045;
        public const ushort LinkAccount = 16046;
        public const ushort UnlinkAccount = 16047;
        public const ushort GiveLoot = 16048;
        public const ushort ChangeName = 16049;
        public const ushort GetDailyGoals = 16050;
        public const ushort GetTimeToEndOfTheDay = 16051;
        public const ushort ApplyGoalContract = 16052;
        public const ushort PayForGoal = 16053;        
        public const ushort GetClan = 16054;
        public const ushort CreateClan = 16055;
        public const ushort GetClans = 16056;
        public const ushort GetMyRequestsToClans = 16057;
        public const ushort DisposeClan = 16058;
        public const ushort LeaveClan = 16059;
        public const ushort KickFromClan = 16060;
        public const ushort PromoteToHelper = 16061;
        public const ushort SendInvite = 16062;
        public const ushort UpdateClan = 16063;        
        public const ushort GetRequestsToMyClan = 16064;
        public const ushort GetChatHttp = 16065;
        public const ushort CreateChatMessageHttp = 16066;
        public const ushort SendRequest = 16067;
        public const ushort ApproveRequest = 16068;
        public const ushort RejectRequest = 16069;
        public const ushort MarkAsCheater =  16070;
        public const ushort UpgradeConsumable =  16071;
        public const ushort GetMyInvites =  16072;
        public const ushort AcceptInvite =  16073;
        public const ushort AdWatchedForDaiyGoal = 16074;
        public const ushort GetPlayerRatingHttp =  16075;
        public const ushort BuyConsumable =  16076;
        public const ushort PlaceConsumableToSlot =  16077;
        public const ushort GetBots =  16078;
        public const ushort WeaponUpgrade =  16079;
        public const ushort InstantCompleteWeaponUpgrade =  16080;
        public const ushort PlaceWeaponToSlot =  16081;
        public const ushort ShardsBuy = 16082;
        public const ushort RemoveWeaponFromSlot =  16083;
        public const ushort UpdateConfigurationHttp =  16084;
        public const ushort GetPlayerIdHttp =  16085;
        public const ushort PingRegionHttp =  16086;
        public const ushort GetBackendHttp =  16087;
        public const ushort UpdatePlayersCount =  16088;
        public const ushort GetExternalAccountsFromMaster =  16089;
        public const ushort GetGuestIdFromMaster =  16090;
        public const ushort LinkAccountOnMaster =  16091;
        public const ushort UnlinkAccountOnMaster =  16092;
        public const ushort ClaimWeaponUpgradeComplete =  16093;
        public const ushort GetMinerPrize =  16094;
        public const ushort JoinMarketingEvent =  16095;
        public const ushort ActualizeServer =  16096;
        public const ushort CompleteAndUpgrade =  16097;
        public const ushort GetBattleRecord =  16098;
        public const ushort GetOnline =  16099;
        public const ushort PlaceWorkshopProjectToSlot =  16100;
        public const ushort ClaimWorkshopProjectResult =  16101;
        public const ushort InstantCompleteWorkshopProject =  16102;
        public const ushort UpdateWorkshopStatus =  16103;
        public const ushort BuyMarketItem =  16104;
        public const ushort ExchangeCurrency =  16105;
        public const ushort OpenBoxNew =  16106;
        public const ushort GetBoxOpenRecords =  16107;
        public const ushort GetWinners =  16108;
        public const ushort OpenFreeBoxNew =  16109;
        public const ushort GetRoomStats =  16110;
        public const ushort UpdateAdWatch =  16111;
        public const ushort ChangeBalance =  16112;
        public const ushort GetGameDataEvent = 16113;
        public const ushort CountdownEvent = 16114;
        public const ushort PlayerIsReadyEvent = 16115;
        public const ushort DamageEvent = 16116;
        public const ushort DestroyEvent = 16117;
        public const ushort GetRoomPlayers = 16118;
        public const ushort RobotDataEvent = 16119;
        public const ushort StartFireEvent = 16120;
        public const ushort StopFireEvent = 16121;
        public const ushort GetSpawnRequest = 16122;
        public const ushort SpawnedEvent = 16123;
        public const ushort RoundTimerEvent = 16124;
        public const ushort PlayerIsReadyToSpawnEvent = 16125;
        public const ushort AnimationStateEvent = 16126;
        public const ushort RewardEarnedEvent = 16127;
        public const ushort FreezeCountdownEvent = 16128;
        public const ushort LevelGainedEvent = 16129;
        public const ushort GetMyRobots = 16130;
        public const ushort InactivityDisconnectEvent = 16131;
        public const ushort ConsumableActivatedEvent = 16132;
        public const ushort HealEvent = 16133;
        public const ushort PointsEarnedEvent = 16134;
        public const ushort UpdateDailyGoalEvent = 16135;
        public const ushort MatchEndedEvent = 16136;
        public const ushort LootSpawned = 16138;
        public const ushort LootRemoved = 16139;
        public const ushort PostEffectEvent =  16140;
        public const ushort LinkBotEvent =  16141;
        public const ushort SpawnBotEvent =  16142;
        public const ushort LinkBotCompletedEvent =  16143;
        public const ushort PositionSyncEvent =  16144;
        public const ushort CpProgress =  16145;
        public const ushort CpStatus =  16146;
        public const ushort CpScore =  16147;
        public const ushort DropPodLanded =  16148;
        public const ushort PhysicShieldActivated =  16149;
        public const ushort PhysicShieldDestroyed =  16150;
        public const ushort PhysicShieldDamaged =  16151;
        public const ushort AbilityActivated =  16152;
        public const ushort AbilityDeactivated =  16153;
        public const ushort MatchResultEvent =  16154;
        public const ushort BarrierDamageEvent =  16155;
        public const ushort SlackBotMessageRequest = 16156;
        //RW.operations.END

    }
}