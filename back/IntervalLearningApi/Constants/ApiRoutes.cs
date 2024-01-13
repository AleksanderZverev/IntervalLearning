using Domain.User.ValueObjects;

namespace IntervalLearningApi.Constants;

public static class ApiRoutes
{
    public static class Accounts
    {
        public const string BasePath = "api/accounts";
        
        public const string Register = "register";
        public const string Authenticate = "authenticate";
        public const string RefreshToken = "refresh-token";
        public const string RevokeToken = "revoke-token";
    }
    
    public static class Collections
    {
        public const string BasePath = "api/collections";

        public const string Create = "";
        
        public const string Delete_DeleteCollection = "{collectionId}";
        public static string GetDeleteCollectionPath(string collectionId)
            => $"{collectionId}";
        
        public const string SearchPublic = "search";
        public const string SearchPrivate = "search/private";
        
        public const string GetPublicCollection = "public/{userId:long}-{collectionId}";
        public static string GetPublicCollectionPath(UserId userId, long collectionId)
            => $"public/{userId}-{collectionId}";

        public const string GetAll = "";
        public const string GetRandomWords = "words/random";
        public const string GetRepeatCollections = "repeat";
        public const string GetNotFinished = "not-finished";
        
        public const string GetCollection = "{collectionId}";
        public static string GetCollectionPath(int collectionId) 
            => $"{collectionId}";
            
        public const string MakePublic = "{collectionId}/public";
        public static string GetMakePublicPath(int collectionId) 
            => $"{collectionId}/public";
        
        public const string AddCardsToMyCollection = "{collectionUserId}-{collectionId}/add";
        public static string AddCardsToMyCollectionPath(UserId userId, int collectionId)  
            => $"{userId}-{collectionId}/add";
    }
    
    public static class Cards
    {
        public const string BasePath = "api/collections/{collectionId}/cards";
        public static string GetBasePath(short collectionId)
            => $"api/collections/{collectionId}/cards";
        
        public const string Get_Card = "{cardId}";
        public static string GetCardPath(string cardId)
            => cardId;
        
        public const string Get_GetAll = "";
        public const string Get_GetCardsQueue = "repeat";
        public const string Get_GetNotStartedCards = "not-started";
        public const string Get_GetAllRelearningCards = "relearn";
        public const string Patch_RelearnCard = "relearn";
        public const string Post_CreateCard = "";
        
        public const string Delete_DeleteCard = "{cardId}";
        public static string GetDeleteCardPath(short cardId) 
            => $"{cardId}";

        public const string Delete_StopRepeatingCard = "{cardId}/learn";
        public static string GetStopRepeatingCardPath(string cardId) 
            => $"{cardId}/learn";

        public const string Patch_PostponeRepeatingCard = "{cardId}/learn/postpone";
        public static string GetPostponeRepeatingCardPath(string cardId) 
            => $"{cardId}/learn/postpone";
        
        
        public const string Post_MoveCard = "move";
        public const string Get_SearchCard = "search";
        public const string Post_StartCards = "start";  
        public const string Path_RememberCard = "remember";
    }
    
    public static class Schedule
    {
        public const string BasePath = "api/schedules";
        
        public const string Get_GetAll = "";
        
        public const string Get_GetUserSchedule = "{userId}/{scheduleId}";
        public static string GetGetUserSchedulePath(UserId userId, string scheduleId)
            => $"{userId}/{scheduleId}";
        
        public const string Get_GetMySchedule = "my/{scheduleId}";
        public static string GetGetMySchedulePath(string scheduleId)
            => $"my/{scheduleId}";
        
        public const string Patch_EditSchedule = "{scheduleId}";
        public static string GetEditSchedulePath(string scheduleId)
            => $"{scheduleId}";
        
        public const string Post_CreateSchedule = "";
    }

    public class Statistics
    {
        public const string BasePath = "api/statistics";

        public const string Get_LearningStatistic = "learning";
        public const string Get_DetailedCalendarStatistic = "calendar/detailed";
    }
    
    public class Themes
    {
        public const string BasePath = "api/themes";

        public const string Get_GetAll = "";
    }

    public class Dictionary
    {
        public const string BasePath = "api/dictionary";

        public const string Get_GenLanguages = "languages";
        public const string Post_AddTranslations = "translations";
        public const string Get_GetTranslation = "translations";
        public const string Get_SearchWords = "words/search";
    }
}