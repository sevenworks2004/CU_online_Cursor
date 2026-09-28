using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


namespace ModOnlineCursor
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "Seven.OnlineCursor";
        public const string ModName = "Online Cursor";
        public const string ModVersion = "0.0.1";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            OnlineCursorProcess.init();
            _harmony.PatchAll(typeof(Plugin).Assembly);
        }
    }
}