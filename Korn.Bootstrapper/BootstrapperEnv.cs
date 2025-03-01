namespace Korn.Bootstrapper
{
    public static class BootstrapperEnv
    {
        static BootstrapperEnv()
        {
#if NET8_0
            TargetVersion = ".net8";
#elif NET472
            TargetVersion = ".net472";
#endif
        }

        public static readonly string TargetVersion;

        public static KornLogger Logger;
        public static PluginLoader PluginLoader;
    }
}