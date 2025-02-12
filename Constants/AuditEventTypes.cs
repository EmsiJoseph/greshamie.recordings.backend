namespace backend.Constants
{
    public static class AuditEventTypes
    {
        public const int UserLoggedIn = 1;
        public const int UserLoggedOut = 2;
        public const int RecordPlayed = 3;
        public const int RecordExported = 4;
        public const int RecordDeleted = 5;
        public const int TokenRefreshed = 6;
        public const int ManualSync = 7;
        public const int AutoSync = 8;
    }
}