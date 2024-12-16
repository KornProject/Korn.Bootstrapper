using Korn;
using Korn.Utils;

static class Env
{
    public static readonly string KornPath = SystemVariablesUtils.GetKornPath()??throw new KornError("BootstrapperEnv->.cctor: KornPath is null.");
    public static LocalLogger Logger;
}