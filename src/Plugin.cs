using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


namespace ModOnlineCursor
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "Seven.MultiPlayerCursor";
        public const string ModName = "MultiPlayerCursor";
        public const string ModVersion = "0.0.8";
        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            ConfigOnlineCursor.config(this.Config);
            OnlineCursorProcess.init();
            _harmony.PatchAll(typeof(Plugin).Assembly);
        }
    }
}