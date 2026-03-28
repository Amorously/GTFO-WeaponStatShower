using GameData;
using Gear;

namespace WeaponStatShower.Utils
{
    internal static class ArchetypeUtil
    {
        internal static ArchetypeDataBlock? GetArchetypeDataBlock(GearIDRange idRange, uint categoryID, GearCategoryDataBlock gearCatBlock)
        {
            eWeaponFireMode val = (eWeaponFireMode)idRange.GetCompID(eGearComponent.FireMode);
            bool flag = categoryID == 12;
            return flag
                ? SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(val)
                : GameDataBlockBase<ArchetypeDataBlock>.GetBlock(GearBuilder.GetArchetypeID(gearCatBlock, val));
        }

        internal static ArchetypeDataBlock? GetMappedArchetypeDataBlock(GearIDRange idRange, uint categoryID, GearCategoryDataBlock gearCatBlock)
        {
            eWeaponFireMode val = (eWeaponFireMode)idRange.GetCompID(eGearComponent.FireMode);
            bool flag = categoryID == 12;
            if (!flag)
            {
                val = val switch
                {
                    eWeaponFireMode.SentryGunSemi => eWeaponFireMode.Semi,
                    eWeaponFireMode.SentryGunAuto => eWeaponFireMode.Auto,
                    eWeaponFireMode.SentryGunBurst => eWeaponFireMode.Burst,
                    eWeaponFireMode.SentryGunShotgunSemi => eWeaponFireMode.Semi,
                    _ => val
                };
            }
            return flag
                ? SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(val)
                : GameDataBlockBase<ArchetypeDataBlock>.GetBlock(GearBuilder.GetArchetypeID(gearCatBlock, val));
        }
    }
}