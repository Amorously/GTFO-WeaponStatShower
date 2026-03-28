using BepInEx;
using BepInEx.Configuration;
using CellMenu;
using GameData;
using Gear;
using WeaponStatShower.Utils;
using WeaponStatShower.Utils.Language;

namespace WeaponStatShower.Patches
{
    internal class ShowStat : Patch
    {
        public override string Name { get; } = PatchName;
        
        public static Patch Instance { get; private set; }

        private static readonly ConfigDefinition ConfigEnabled = new(PatchName, "Enabled");
        private static readonly ConfigDefinition Language = new(PatchName, "Language");
        private static readonly ConfigDefinition ConfigSleepers = new(PatchName, "SleepersShown");
        private static readonly ConfigDefinition ShowStats = new(PatchName, "ShowStats");
        private static readonly ConfigDefinition ShowDescription = new(PatchName, "ShowDescription");

        private static WeaponDescriptionBuilder? DescriptionBuilder;
        private static LanguageEnum PrevLanguageEnum = LanguageEnum.English;
        private static string PrevShownSleepers = "PLACEHOLDER";
        
        private const string PatchName = nameof(ShowStat);
        private const PatchType patchType = PatchType.Postfix;

        public override void Initialize()
        {
            Instance = this;
            WeaponStatShowerPlugin.Instance.Config.Bind(ConfigEnabled, true, new ConfigDescription("Show the stats of a weapon."));
            WeaponStatShowerPlugin.Instance.Config.Bind(Language, LanguageEnum.English, new ConfigDescription("Select the mod language."));
            WeaponStatShowerPlugin.Instance.Config.Bind<string>(ConfigSleepers, "NONE",
                new ConfigDescription("Select which Sleepers are shown, seperated by a comma.\n" +
                "Acceptable values: ALL, NONE, STRIKER, SHOOTER, SCOUT, BIG_STRIKER, BIG_SHOOTER, CHARGER, CHARGER_SCOUT"));
            WeaponStatShowerPlugin.Instance.Config.Bind(ShowStats, true, new ConfigDescription("Show auto-generated weapon stats."));
            WeaponStatShowerPlugin.Instance.Config.Bind(ShowDescription, true, new ConfigDescription("Show gear descriptions alongside stats and enemy killpoints (if enabled)."));
            DescriptionBuilder = new WeaponDescriptionBuilder();
        }

        public override void Execute()
        {
            PatchMethod<CM_InventorySlotItem>(nameof(CM_InventorySlotItem.LoadData), patchType);
        }

        public static void CM_InventorySlotItem__LoadData__Postfix(CM_InventorySlotItem __instance, GearIDRange idRange, bool clickable, bool detailedInfo)
        {
            if (__instance == null || !detailedInfo) return;
            if (DescriptionBuilder == null)
            {
                WeaponStatShowerPlugin.LogError("Something went wrong with the DescriptionBuilder");
                return;
            }

            WeaponStatShowerPlugin.Instance.Config.Reload();
            var config = WeaponStatShowerPlugin.Instance.Config;
            string currShownSleepers = config.GetConfigEntry<string>(ConfigSleepers).Value.Trim().ToUpper();
            LanguageEnum currLanguageValue = config.GetConfigEntry<LanguageEnum>(Language).Value;
            bool showStats = config.GetConfigEntry<bool>(ShowStats).Value;
            bool showDescription = config.GetConfigEntry<bool>(ShowDescription).Value;

            if (!PrevShownSleepers.Equals(currShownSleepers) || !currLanguageValue.Equals(PrevLanguageEnum))
            {
                DescriptionBuilder.UpdateSleepersDatas(currShownSleepers.Split(','), currLanguageValue);
                PrevShownSleepers = currShownSleepers;
                PrevLanguageEnum = currLanguageValue;
            }

            DescriptionBuilder.Inizialize(idRange, PlayerDataBlock.GetBlock(1U), currLanguageValue, showStats, showDescription);
            if (showDescription)
            {
                string builtDescription = DescriptionBuilder.DescriptionFormatter(__instance.GearDescription);
                if (!builtDescription.IsNullOrWhiteSpace()) builtDescription += "\n";
                __instance.GearDescription = builtDescription + __instance.GearDescription;
            }
            __instance.GearPublicName = DescriptionBuilder.FireRateFormatter(__instance.GearPublicName);
        }
    }
}
