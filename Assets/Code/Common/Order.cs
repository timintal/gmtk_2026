namespace Code.Common
{
    public static class Order
    {
        public const short Init = -10000;
        public const short PostInit = -9500;

        public static readonly short Input = -9000;

        public const short PreUpdate = 1000;

        public const short Update = 2000;

        public const short LateUpdate = 3000;

        public const short PreCleanup = 9000;
        public const short Cleanup = 10000;
    }
}