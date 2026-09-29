


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
        OnlineCursorProcess.isIgnorePVPInvsableCursor = ignorePVPhideCursor.Value;
    }
}