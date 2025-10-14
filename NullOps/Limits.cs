namespace NullOps;

public static class Limits
{
    public static class Settings
    {
        public const int MaxKeyLength = 128;
        public const int MaxValueLength = 256;
    }

    public static class Users
    {
        public const int MinUsernameLength = 4;
        public const int MaxUsernameLength = 128;
        
        public const int MinPasswordLength = 4;
        public const int MaxPasswordLength = 256;
    }

    public static class Agents
    {
        public const int MinNameLength = 4;
        public const int MaxNameLength = 128;
        public const int MaxAddressLength = 256;
        public const int MaxTokenLength = 64;
    }
}