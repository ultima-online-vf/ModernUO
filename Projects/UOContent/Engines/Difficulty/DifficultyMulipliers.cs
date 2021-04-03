namespace Server.Engines.Difficulty
{
    public static class DifficultyMulipliers
    {
        public static int HardMultiplier;
        public static int NightmareMultiplier;

        public static void Configure()
        {
            HardMultiplier = ServerConfiguration.GetOrUpdateSetting("difficulty.multiplier.hard", 2);
            NightmareMultiplier = ServerConfiguration.GetOrUpdateSetting("difficulty.multiplier.nightmare", 5);
        }
    }
}
