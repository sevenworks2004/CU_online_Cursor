


using System;

class ConfigOnlineCursor
{
    public static void config(BepInEx.Configuration.ConfigFile config)
    {
        var ignorePVPhideCursor = config.Bind<bool>(
            "MultiPlayerCursor",
            "ignorePVPhideCursor",
            false,
            ":)"
        );
        if (Environment.GetEnvironmentVariable("IGNORPVP") == "1")
        {
            OnlineCursorProcess.isIgnorePVPInvsableCursor = true;
            return;
        }
        OnlineCursorProcess.isIgnorePVPInvsableCursor = ignorePVPhideCursor.Value;
    }
}