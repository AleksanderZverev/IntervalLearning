using System.Diagnostics.CodeAnalysis;

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
        public const string SearchPublic = "search";
        public const string SearchPrivate = "search/private";
        
        public const string GetPublicCollection = "public/{userId:long}-{collectionId}";
        public static string GetPublicCollectionPath(long userId, long collectionId)
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
        public static string AddCardsToMyCollectionPath(long userId, int collectionId)  
            => $"{userId}-{collectionId}/add";
        
    }
}