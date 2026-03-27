using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using WeaponStatShower.Patches;

namespace WeaponStatShower
{
    [BepInPlugin(GUID, ModName, "1.5.2")]
    [BepInProcess("GTFO.exe")]
    public class WeaponStatShowerPlugin : BasePlugin
    {

        internal const string ModName = "Weapon Stat Shower";

        internal const string GUID = "WeaponStatShower";

        private const string SectionMain = "Config";
        private static readonly ConfigDefinition configDefinition = new(SectionMain, "Version");
        private static readonly ConfigDefinition ConfigGameVersion = new(SectionMain, "GameVersion");

        private static Harmony HarmonyInstance;
        private static readonly Dictionary<Type, Patches.Patch> RegisteredPatches = new();

        public static WeaponStatShowerPlugin Instance { get; private set; }

        public WeaponStatShowerPlugin()
        {
            this.Config.SaveOnConfigSet = false;
        }
        public override void Load()
        {
            Instance = this;

            this.Config.SaveOnConfigSet = true;

            LogInfo("STARTED");

            RegisterPatch<ShowStat>();

            this.Config.Save();
        }

        public static void RegisterPatch<T>() where T : Patches.Patch, new()
        {
            if (HarmonyInstance == null)
            {
                HarmonyInstance = new Harmony(GUID);
            }

            if (RegisteredPatches.ContainsKey(typeof(T)))
            {
                LogDebug($"Ignoring duplicate patch: {typeof(T).Name}");
                return;
            }

            var patch = new T
            {
                Harmony = HarmonyInstance,
            };

            patch.Initialize();

            if (patch.Enabled)
            {
                LogInfo($"Applying patch: {patch.Name}");
                patch.Execute();
            }

            RegisteredPatches[typeof(T)] = patch;
        }

        public static void LogDebug(object data) => Instance.Log.LogDebug(data);

        public static void LogError(object data) => Instance.Log.LogError(data);

        public static void LogFatal(object data) => Instance.Log.LogFatal(data);

        public static void LogInfo(object data) => Instance.Log.LogInfo(data);

        public static void LogMessage(object data) => Instance.Log.LogMessage(data);

        public static void LogWarning(object data) => Instance.Log.LogWarning(data);
    }
}
