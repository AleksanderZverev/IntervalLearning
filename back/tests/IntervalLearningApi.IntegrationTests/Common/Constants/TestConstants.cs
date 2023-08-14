namespace IntervalLearningApi.IntegrationTests.Common.Constants;

public static class TestConstants
{
    public static class User
    {
        public static long Id { get; set; } = 1;
        public static string Email => "test@mail.ru";
        public static string Password => "test123";
        public static string FirstName => "Иван";
        public static string LastName => "Тестировщик";
    }
    
    public static class Collection
    {
        public static short Id { get; set; } = 1;
        
        public static class Other
        {
            public static short Id { get; set; } = 2;
        }
    }

    public static class Language
    {
        public static short TestId { get; set; } = 1;
    }
    
    public static class Theme
    {
        public static short TestId { get; set; } = 1;
    }
}