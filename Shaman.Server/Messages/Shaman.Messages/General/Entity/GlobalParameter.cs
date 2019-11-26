using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity
{
    public struct ParameterNames
    {
        public const string DefaultFirstRobotId = "DefaultFirstRobotId";
        public const string ServerTimeDelta = "ServerTimeDelta";
        public const string MaxAdWatchesPerDay = "MaxAdWatchesPerDay";
        public const string GoldForWatch = "GoldForWatch";
        public const string DefaultSilver = "DefaultSilver";
        public const string DefaultGold = "DefaultGold";
        public const string DefaultPoints = "DefaultPoints";
        public const string IsOnService = "IsOnService";
        public const string OnServiceMessageKey = "OnServiceMessageKey";
        public const string IgnoreTokenParam = "IgnoreTokenParam";
        public const string DefaultCountry = "DefaultCountry";
        public const string TournamentTopCount = "TournamentTopCount";
        public const string RatingMultiplier = "RatingMultiplier";
        public const string DayBeforeInfiniteDailyGifts = "DayBeforeInfiniteDailyGifts";
        public const string UpgradeStationMaxLevel = "UpgradeStationMaxLevel";
        public const string UpgradeStationLevel2Cap = "UpgradeStationLevel2Cap";
        public const string UpgradeStationLevel3Cap = "UpgradeStationLevel3Cap";

        public const string ClanMaxRequestsCount = "ClanMaxRequestsCount";
        public const string ClanJoinCooldown = "ClanJoinCooldown";
        public const string ClanCreateCooldown = "ClanCreateCooldown";
        public const string MaxClanMembers = "MaxClanMembers";
        public const string LevelMinToCreateClan = "LevelMinToCreateClan";
        public const string LevelMinToJoinClan = "LevelMinToJoinClan";

        public const string MaxDaysDeletedExists = "MaxDaysDeletedExists";
        public const string InviteLiveTime = "InviteLiveTime";

        public const string MaxAdWatchesForUpgradePerDay = "MaxAdWatchesForUpgradePerDay";
        public const string UpgradeReducedSeconds = "UpgradeReducedSeconds";
        public const string DailyGoalsCount = "DailyGoalsCount";
        public const string BattlesBeforeAdShow = "BattlesBeforeAdShow";
        public const string PointsForWatch = "PointsForWatch";

        public const string FireteamChatLimit = "FireteamChatLimit";
        public const string ClanChatLimit = "ClanChatLimit";
        public const string GlobalChatLimit = "GlobalChatLimit";
        public const string ChatMessageSymbolsLimit = "ChatMessageSymbolsLimit";

        public const string ClanListLimit = "ClanListLimit";
        public const string MaxAdWatchesForGoalsPerDay = "MaxAdWatchesForGoalsPerDay";

        public const string PlayerBuffLong = "PlayerBuffLong";
        public const string PlayerBuffCooldown = "PlayerBuffCooldown";

        public const string TutorialMapId = "TutorialMapId";

        public const string KillsMatchmakingCoefficient = "KillsMatchmakingCoefficient";
        public const string MaxSpawns = "MaxSpawns";
        public const string PrizeWinnersLimit = "PrizeWinnersLimit";
        public const string WinnerRotationRatio = "WinnerRotationRatio";
        public const string BackEndId = "BackEndId";
        public const string SocialFeaturesEnabled = "SocialFeaturesEnabled";
        public const string GoogleAuthFileName = "GoogleAuthFileName";
        public const string GoogleApplicationName = "GoogleApplicationName";
        public const string GooglePackageName = "GooglePackageName";


        //rate us
        public const string RateUsWinningPlaces = "RateUsWinningPlaces";
        public const string RateUsStrongPositiveLimitShow = "RateUsStrongPositiveLimitShow";
        public const string RateUsWindowRespawnRate = "RateUsWindowRespawnRate";
        public const string RateUsMinPlayerLevelShow = "RateUsMinPlayerLevelShow";

        //matchmaking
        public const string RobotNumberForMatchMaking = "RobotNumberForMatchMaking";

        //gameplay
        public const string MinDamageOnRadiusBorder = "MinDamageOnRadiusBorder";
        public const string FreezingPowerNumberOfBattles = "FreezingPowerNumberOfBattles";

        //social
        public const string JoinSocialNetworkEventId = "JoinSocialNetworkEventId";

        //ad
        public const string AdDailyAvailableBase = "AdDailyAvailableBase";
        public const string AdMaximumAvailable = "AdMaximumAvailable";
        public const string AdAddedAfterBattle = "AdAddedAfterBattle";
        public const string AdAdditionalPrizePercent = "AdAdditionalPrizePercent";
        public const string AdForcedShowChance = "AdForcedShowChance";
        public const string AdForcedShowMinPlayerRegisterDate = "AdMinPlayerRegisterDate";
        public const string AdForcedShowMaxPlayerRegisterDate = "AdMaxPlayerRegisterDate";
        public const string AdResetPeriodSec = "AdResetPeriodSec";
        public const string AdMinPlayerLevel = "AdMinPlayerLevel";
        public const string AdMaxPlayerLevel = "AdMaxPlayerLevel";
        public const string ShowAdPercent = "ShowAdPercent";

        //gameplay
        public const string TutorialLevelAfterBattleTutorialStarts = "TutorialLevelAfterBattleTutorialStarts";

        //robot rent
        public const string MaxRentCount = "MaxRentCount";

        //temp db
        public const string IsProd = "IsProd";

        //workshop
        public const string WorkshopLevel = "WorkshopLevel";
        public const string RentSeconds = "RentSeconds";
        public const string WorkshopWatchTimeReduce = "WorkshopWatchTimeReduce";

        //market
        public const string MarketRefreshTimeSeconds = "MarketRefreshTimeSeconds";

        //stat
        public const string StatScreenParameterName = "StatScreenParameterName";

        //bots
        public const string GetBotsNamesUrl = "GetBotsNamesUrl"; // https://***REMOVED***.***REMOVED***.com:5002/getnames
        public const string RealPlayersNamesPercent = "RealPlayersNamesPercent"; // 30
        public const string BotChangePathChance = "BotChangePathChance";
        public const string BotCombatChoosePlayerChance = "BotCombatChoosePlayerChance";
        public const string BotCombatChooseOffenderChance = "BotCombatChooseOffenderChance";
        public const string BotSandboxChoosePlayerChance = "BotSandboxChoosePlayerChance";
        public const string BotSandboxChooseOffenderChance = "BotSandboxChooseOffenderChance";
        public const string BotVisibilityInInvisibleMode = "BotVisibilityInInvisibleMode";
        public const string BotMinSolutionInterval = "BotMinSolutionInterval";
        public const string BotMaxSolutionInterval = "BotMaxSolutionInterval";
        public const string BotMinFireInterval = "BotMinFireInterval";
        public const string BotMaxFireInterval = "BotMaxFireInterval";
        public const string BotStopFireChance = "BotStopFireChance";
        
        public const string LogBuyingFlow = "LogBuyingFlow";

        //new boxes
        public const string IsLogBoxes = "IsLogBoxes";
        public const string ShowPrizeHistory = "ShowPrizeHistory";
        public const string GachaLevel = "GachaLevel";
        public const string SuperBoxProgressBar = "SuperBoxProgressBar";
        public const string SuperBoxCurrencyId = "SuperBoxCurrencyId";
        public const string MaxBoxItemRarity = "MaxBoxItemRarity";
        public const string BoxItemHistoryMax = "BoxItemHistoryMax";
        public const string DailyFreeBoxIntervalSec = "DailyFreeBoxIntervalSec";
        public const string DailyFreeBoxType = "DailyFreeBoxType";

        public const string LogFightRewards = "LogFightRewards";
        public const string IsFreeAndAdBoxAffectSuperBoxProgress = "IsFreeAndAdBoxAffectSuperBoxProgress";

        public const string SandboxFinish = "SandboxFinish";
        public const string DailyGoalsStartOnLevel = "DailyGoalsStartOnLevel";
        public const string DailyGiftsStartOnLevel = "DailyGiftsStartOnLevel";
        public const string ArePushNotificationsActive = "ArePushNotificationsActive";
        public const string IsDamageReliable = "IsDamageReliable";

        public const string GetMyCountryUrl = "GetMyCountryUrl";
        public const string TimeSkipFree = "TimeSkipFree";
        public const string AutoQualityMinFPS = "AutoQualityMinFPS";
        public const string AutoQualityMidFPS = "AutoQualityMidFPS";
        public const string AutoQualityMaxFPS = "AutoQualityMaxFPS";
        
        public const string FirstAnticheatPeriodParam = "FirstAnticheatPeriodParam";
        public const string SecondAnticheatPeriodParam = "SecondAnticheatPeriodParam";
        public const string ThirdAnticheatPeriodParam = "ThirdAnticheatPeriodParam";
    }

    [Serializable]
    public class GlobalParameter : EntityBase
    {
        public string Name { get; set; }
        public string StringValue { get; set; } = "";
        public int? IntValue { get; set; }
        public float? FloatValue { get; set; }
        public bool? BoolValue { get; set; }
        public DateTime? DateTimeValue { get; set; }

        public int GetIntValue()
        {
            if (IntValue != null)
                return IntValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        public string GetStringValue()
        {
            if (StringValue != null)
                return StringValue;
            else
                throw new Exception("Parameter has null value");
        }
        public float GetFloatValue()
        {
            if (FloatValue != null)
                return FloatValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        public bool GetBoolValue()
        {
            if (BoolValue != null)
                return BoolValue.Value;
            else
                throw new Exception("Parameter has null value");
        }
        
        public DateTime? GetNullableDateTimeValue()
        {
            return DateTimeValue;
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(this.Name);
            typeWriter.Write(this.StringValue);
            if (IntValue != null)
            {
                typeWriter.Write((byte) 1);
                typeWriter.Write(this.IntValue.Value);
            }
            else
            {
                typeWriter.Write((byte) 0);
            }
            if (FloatValue != null)
            {
                typeWriter.Write((byte) 1);
                typeWriter.Write(this.FloatValue.Value);
            }
            else
            {
                typeWriter.Write((byte) 0);
            }
            if (BoolValue != null)
            {
                typeWriter.Write((byte) 1);
                typeWriter.Write(this.BoolValue.Value);
            }
            else
            {
                typeWriter.Write((byte) 0);
            }
            if (DateTimeValue != null)
            {
                typeWriter.Write((byte) 1);
                typeWriter.Write(this.DateTimeValue.Value.ToBinary());
            }
            else
            {
                typeWriter.Write((byte) 0);
            }
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Name = typeReader.ReadString();
            StringValue = typeReader.ReadString();
            var intValueSet = typeReader.ReadByte();
            if (intValueSet == 1)
                IntValue = typeReader.ReadInt();
            var floatValueSet = typeReader.ReadByte();
            if (floatValueSet == 1)
                FloatValue = typeReader.ReadFloat();
            var boolValueSet = typeReader.ReadByte();
            if (boolValueSet == 1)
                BoolValue = typeReader.ReadBool();
            var dtValueSet = typeReader.ReadByte();
            if (dtValueSet == 1)
                DateTimeValue = new DateTime(typeReader.ReadLong());
        }
        
    }
}
