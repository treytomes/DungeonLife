namespace DungeonLife
{
    public static class LifeAppSettings
    {
        public static string WindowTitle { get; } = "Dungeon Life";
        public static int ScreenWidth { get; } = 200; // 80
        public static int ScreenHeight { get; } = 60; // 25
        public static float MinimumTemperature { get; } = 0.0f;
        public static float MaximumTemperature { get; } = 50.0f;
        public static float TemperatureRange { get; } = MaximumTemperature - MinimumTemperature;
    }
}
