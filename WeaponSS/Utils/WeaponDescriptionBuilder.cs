using GameData;
using Gear;
using Player;
using System.Text;
using System.Text.Json;
using WeaponStatShower.Utils.Language;
using WeaponStatShower.Utils.Language.Models;

namespace WeaponStatShower.Utils
{
    internal class WeaponDescriptionBuilder
    {
        private SleepersDatas sleepersDatas;
        private PlayerDataBlock _playerDataBlock;
        private GearIDRange idRange;
        private uint categoryID;
        private GearCategoryDataBlock gearCatBlock;
        private ItemDataBlock itemDataBlock;
        private LanguageDatas languageDatas;

        internal void Inizialize(GearIDRange idRange, PlayerDataBlock playerDataBlock, LanguageEnum language)
        {
            this.idRange = idRange;
            _playerDataBlock = playerDataBlock;
            categoryID = idRange.GetCompID(eGearComponent.Category);
            gearCatBlock = GameDataBlockBase<GearCategoryDataBlock>.GetBlock(categoryID);
            itemDataBlock = ItemDataBlock.GetBlock(gearCatBlock.BaseItem);

            languageDatas = desirializeLanguageJson(language);
        }

        public void UpdateSleepersDatas(string[] activatedSleepers, LanguageEnum language)
        {
            if (activatedSleepers[0].Trim().Length == 0)
            {
                WeaponStatShowerPlugin.LogWarning("Empty String in the config file, applying Default values");
                activatedSleepers = new string[] { "ALL" };
            }
            sleepersDatas = new SleepersDatas(activatedSleepers, desirializeLanguageJson(language).sleepers);
        }

        private LanguageDatas desirializeLanguageJson(LanguageEnum language)
        {
            LanguageDatasClass languageStrings = JsonSerializer.Deserialize<LanguageDatasClass>(LocalizedString.JsonString)!;
            return language.Equals(LanguageEnum.English) ? languageStrings.english : languageStrings.chinese;
        }







        public string DescriptionFormatter(string GearDescription)
        {

            if (itemDataBlock.inventorySlot == InventorySlot.GearMelee)
            {
                MeleeArchetypeDataBlock meleeArchetypeDataBlock = MeleeArchetypeDataBlock.GetBlock(GearBuilder.GetMeleeArchetypeID(gearCatBlock));

                GearDescription = VerboseDescriptionFormatter(meleeArchetypeDataBlock);

                return GearDescription + "\n\n" + GetFormatedWeaponStats(meleeArchetypeDataBlock, itemDataBlock);
            }
            else
            {
                eWeaponFireMode weaponFireMode = (eWeaponFireMode)idRange.GetCompID(eGearComponent.FireMode);

                bool isSentryGun = categoryID == 12; // => PersistentID for Sentry Gun

                ArchetypeDataBlock archetypeDataBlock = isSentryGun
                    ? SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(weaponFireMode)
                    : ArchetypeDataBlock.GetBlock(GearBuilder.GetArchetypeID(gearCatBlock, weaponFireMode));

                if (archetypeDataBlock == null) return GearDescription; //case of non-weapon tools such as BIO_TRACKER/MINE_DEPLOYER

                GearDescription = VerboseDescriptionFormatter(archetypeDataBlock, isSentryGun);

                return GearDescription + GetFormatedWeaponStats(archetypeDataBlock, isSentryGun);
            }
        }

        internal string FireRateFormatter(string gearPublicName)
        {
            if (itemDataBlock.inventorySlot == InventorySlot.GearMelee)
            {
                MeleeArchetypeDataBlock meleeArchetypeDataBlock = MeleeArchetypeDataBlock.GetBlock(GearBuilder.GetMeleeArchetypeID(gearCatBlock));
                return VerbosePublicNameMelee(meleeArchetypeDataBlock);
            }
            else
            {
                eWeaponFireMode weaponFireMode = (eWeaponFireMode)idRange.GetCompID(eGearComponent.FireMode);

                bool isSentryGun = categoryID == 12; // => PersistentID for Sentry Gun

                ArchetypeDataBlock archetypeDataBlock = isSentryGun
                    ? SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(weaponFireMode)
                    : ArchetypeDataBlock.GetBlock(GearBuilder.GetArchetypeID(gearCatBlock, weaponFireMode));

                if (archetypeDataBlock == null) return gearPublicName; //case of non-weapon tools such as BIO_TRACKER/MINE_DEPLOYER

                return VerbosePublicNameFireMode(archetypeDataBlock);
            }
        }

        private string VerbosePublicNameMelee(MeleeArchetypeDataBlock meleeArchetypeDataBlock)
        {
            MeleeLanguageModel meleeLanguage = languageDatas.melee;
            switch (meleeArchetypeDataBlock.persistentID)
            {
                case 1:
                    return meleeLanguage.hammer;
                case 2:
                    return meleeLanguage.knife;
                case 4:
                    return meleeLanguage.bat;
                case 3:
                    return meleeLanguage.spear;
                default:
                    return "";
            }
        }

        private string VerbosePublicNameFireMode(ArchetypeDataBlock archetypeDataBlock)
        {
            FiremodeLanguageModel firemodeLanguage = languageDatas.firemode;
            eWeaponFireMode fireMode = archetypeDataBlock.FireMode;
            StringBuilder sb = new StringBuilder();
            switch (fireMode)
            {
                case eWeaponFireMode.Auto:
                case eWeaponFireMode.SentryGunAuto:

                    sb.Append(firemodeLanguage.fullA + " (");
                    sb.Append("<#12FF50>");
                    sb.Append(languageDatas.rateOfFire + " ");
                    sb.Append(GetRateOfFire(archetypeDataBlock, fireMode));
                    sb.Append(CLOSE_COLOR_TAG);
                    sb.Append(")"); 
                    break;

                case eWeaponFireMode.Semi:
                case eWeaponFireMode.SentryGunSemi:

                    sb.Append(firemodeLanguage.semiA +" (");
                    sb.Append("<#12FF50>");
                    sb.Append(languageDatas.rateOfFire+" ");
                    sb.Append(GetRateOfFire(archetypeDataBlock, fireMode));
                    sb.Append(CLOSE_COLOR_TAG);
                    sb.Append(")");
                    break;

                case eWeaponFireMode.Burst:
                case eWeaponFireMode.SentryGunBurst:
                    if (archetypeDataBlock.BurstShotCount != 1)
                    {
                        sb.Append(firemodeLanguage.burst +" (");
                        sb.Append("<#704dfa>");
                        sb.Append("#" + archetypeDataBlock.BurstShotCount);
                        sb.Append(CLOSE_COLOR_TAG);
                        sb.Append(DIVIDER);
                        sb.Append("<#12FF50>");
                        sb.Append(languageDatas.rateOfFire +" ");
                        sb.Append(GetRateOfFire(archetypeDataBlock, fireMode));
                        sb.Append(CLOSE_COLOR_TAG);
                        sb.Append(")");
                    }
                    else
                    {
                        sb.Append(firemodeLanguage.semiA + " (");
                        sb.Append("<#12FF50>");
                        sb.Append(languageDatas.rateOfFire +" ");
                        sb.Append(GetRateOfFire(archetypeDataBlock, eWeaponFireMode.Semi));
                        sb.Append(CLOSE_COLOR_TAG);
                        sb.Append(")");
                        break;
                    }
                    break;
                case eWeaponFireMode.SemiBurst:
                    sb.Append("S-Burst (");
                    sb.Append("<#12FF50>");
                    sb.Append("#" + archetypeDataBlock.BurstShotCount);
                    sb.Append(" every " + archetypeDataBlock.SpecialSemiBurstCountTimeout + "\'");
                    sb.Append(CLOSE_COLOR_TAG);
                    sb.Append(")"); 
                    break;

                case eWeaponFireMode.SentryGunShotgunSemi:
                    sb.Append(firemodeLanguage.shotgunSentry + " (");
                    sb.Append("<#12FF50>");
                    sb.Append(languageDatas.rateOfFire + " ");
                    sb.Append(GetRateOfFire(archetypeDataBlock, fireMode));
                    sb.Append(CLOSE_COLOR_TAG);
                    sb.Append(")");
                    break;
                default:
                    WeaponStatShowerPlugin.LogError("FireMode not found");
                    break;
            }

            return sb.ToString();
        }

        private string VerboseDescriptionFormatter(MeleeArchetypeDataBlock meleeArchetypeDataBlock)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(meleeArchetypeDataBlock.CameraDamageRayLength < 1.76 ? languageDatas.melee.shortRange : meleeArchetypeDataBlock.CameraDamageRayLength < 2.5 ? languageDatas.melee.mediumRange : languageDatas.melee.longRange);

            sb.Append(meleeArchetypeDataBlock.CanHitMultipleEnemies? languageDatas.melee.canPierce+ "\n": "");

            return "".Equals(sb.ToString()) ? "" : sb.ToString() + "\n";
        }

        private string VerboseDescriptionFormatter(ArchetypeDataBlock archetypeDataBlock, bool isSentryGun)
        {
            StringBuilder sb = new StringBuilder();
            SpreadLanguageModel spread = languageDatas.spread;
            if (isSentryGun)
                sb.AppendLine(languageDatas.deployable);

            switch (archetypeDataBlock.ShotgunBulletSpread + archetypeDataBlock.ShotgunConeSize)
            {
                case 0: break;
                case 1:
                    sb.AppendLine(spread.chocked);
                    break;
                case 4:
                    sb.AppendLine(spread.small);
                    break;
                case 5:
                    sb.AppendLine(spread.medium);
                    break;
                case 7:
                    sb.AppendLine(spread.medium);
                    break;
                case 9:
                    sb.AppendLine(spread.huge);
                    break;
                default:
                    WeaponStatShowerPlugin.LogError(archetypeDataBlock.PublicName + ": spread not considered{" + archetypeDataBlock.ShotgunBulletSpread + "/" + archetypeDataBlock.ShotgunConeSize +"}");
                    break;
            }

            if (archetypeDataBlock.SpecialChargetupTime > 0)
                sb.AppendLine(archetypeDataBlock.SpecialChargetupTime > 0.4 ?
                    $"{languageDatas.longChargeUp} ({FormatFloat(archetypeDataBlock.SpecialChargetupTime, 2)})" :
                    $"{languageDatas.shortChargeUp} ({FormatFloat(archetypeDataBlock.SpecialChargetupTime, 2)})"
                );

            return "".Equals(sb.ToString())? "" : sb.ToString() + "\n";
        }

        private string GetFormatedWeaponStats(ArchetypeDataBlock archeTypeDataBlock, bool isSentryGun = false)
        {
            if (archeTypeDataBlock == null) return string.Empty;

            bool isChinese = !languageDatas.damage.Equals("Dmg");

            StringBuilder builder = new StringBuilder();

            void Divider(ref int count, StringBuilder builder)
            {
                if (count >= 4 || (isChinese && count >= 3))
                {
                    builder.Append("\n");
                    count = 0;
                }
                else if (count > 0)
                    builder.Append(DIVIDER);
            }

            int count = 0;

            builder.Append("<#9D2929>");
            builder.Append($"{languageDatas.damage} ");
            builder.Append(FormatFloat(archeTypeDataBlock.Damage, 2));
            builder.Append(archeTypeDataBlock.ShotgunBulletCount > 0? "(x"+ archeTypeDataBlock.ShotgunBulletCount +")" : "");
            builder.Append(CLOSE_COLOR_TAG);
            count++;

            if (!isSentryGun)
            {
                builder.Append(DIVIDER);

                builder.Append("<color=orange>");
                builder.Append($"{languageDatas.clip} ");
                builder.Append(archeTypeDataBlock.DefaultClipSize);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            builder.Append(DIVIDER);

            builder.Append("<#FFD306>");
            builder.Append($"{languageDatas.maxAmmo} ");
            builder.Append(GetTotalAmmo(archeTypeDataBlock, itemDataBlock, isSentryGun));
            builder.Append(CLOSE_COLOR_TAG);
            count++;

            if (!isSentryGun)
            {
                Divider(ref count, builder);
                builder.Append("<#C0FF00>");
                builder.Append($"{languageDatas.reload} ");
                builder.Append(FormatFloat(archeTypeDataBlock.DefaultReloadTime, 2));
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }


            if (archeTypeDataBlock.PrecisionDamageMulti != 1f)
            {
                Divider(ref count, builder);
                builder.Append("<#18A4A9>");
                builder.Append($"{languageDatas.precision} ");
                builder.Append(FormatFloat(archeTypeDataBlock.PrecisionDamageMulti, 2));
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            Divider(ref count, builder);
            builder.Append("<#6764de>");
            builder.Append($"{languageDatas.falloff} ");
            builder.Append(((int)archeTypeDataBlock.DamageFalloff.x).ToString() + "m");
            builder.Append(CLOSE_COLOR_TAG);
            count++;

            bool nonStandardStagger = archeTypeDataBlock.StaggerDamageMulti != 1f;

            if (nonStandardStagger)
            {
                Divider(ref count, builder);
                builder.Append("<color=green>");
                builder.Append($"{languageDatas.stagger} ");
                builder.Append(FormatFloat(archeTypeDataBlock.StaggerDamageMulti, 2));
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            bool nonStandardHip = archeTypeDataBlock.HipFireSpread != 0f;

            if (nonStandardHip && archeTypeDataBlock.ShotgunBulletCount == 0)
            {
                Divider(ref count, builder);
                builder.Append("<#cc9347>");
                builder.Append($"{languageDatas.hipSpread} ");
                builder.Append(FormatFloat(archeTypeDataBlock.HipFireSpread, 2));
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            bool nonStandardADS = archeTypeDataBlock.AimSpread != 0f;

            if (nonStandardADS && archeTypeDataBlock.ShotgunBulletCount == 0)
            {
                Divider(ref count, builder);
                builder.Append("<#e6583c>");
                builder.Append($"{languageDatas.aimDSpread} ");
                builder.Append(FormatFloat(archeTypeDataBlock.AimSpread, 2));
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }


            bool pierce = archeTypeDataBlock.PiercingBullets;

            if (pierce)
            {
                Divider(ref count, builder);
                builder.Append("<#097345>");
                builder.Append($"{languageDatas.pierceCount} ");
                builder.Append(archeTypeDataBlock.PiercingDamageCountLimit);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            builder.AppendLine("\n");

            builder.Append(sleepersDatas.VerboseKill(archeTypeDataBlock));

            return builder.ToString();
        }

        private string GetFormatedWeaponStats(MeleeArchetypeDataBlock archeTypeDataBlock, ItemDataBlock itemDataBlock)
        {
            MeleeLanguageModel meleeLanguage = languageDatas.melee;

            if (archeTypeDataBlock == null) return string.Empty;

            StringBuilder builder = new StringBuilder();

            void Divider(ref int count, StringBuilder builder)
            {
                if (count >= 3)
                {
                    builder.Append("\n");
                    count = 0;
                }
                else if (count > 0)
                    builder.Append(DIVIDER);
            }

            builder.Append("<#9D2929>");
            builder.Append($"{languageDatas.damage}.{meleeLanguage.light} ");
            builder.Append(archeTypeDataBlock.LightAttackDamage);
            builder.Append(CLOSE_COLOR_TAG);

            builder.Append(DIVIDER);

            builder.Append("<color=orange>");
            builder.Append($"{languageDatas.damage}.{meleeLanguage.heavy} ");
            builder.Append(archeTypeDataBlock.ChargedAttackDamage);
            builder.Append(CLOSE_COLOR_TAG);

            int count = 2;

            if (!archeTypeDataBlock.AllowRunningWhenCharging)
            {
                Divider(ref count, builder);

                builder.Append("<#FFD306>");
                builder.Append($"{meleeLanguage.canRunWhileCharging}");
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.LightStaggerMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#C0FF00>");
                builder.Append($"{languageDatas.stagger}.{meleeLanguage.light} ");
                builder.Append(archeTypeDataBlock.LightStaggerMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.ChargedStaggerMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<color=green>");
                builder.Append($"{languageDatas.stagger}.{meleeLanguage.heavy} ");
                builder.Append(archeTypeDataBlock.ChargedStaggerMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.LightPrecisionMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#004E2C>");
                builder.Append($"{languageDatas.precision}.{meleeLanguage.light} ");
                builder.Append(archeTypeDataBlock.LightPrecisionMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.ChargedPrecisionMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#55022B>");
                builder.Append($"{languageDatas.precision}.{meleeLanguage.heavy} ");
                builder.Append(archeTypeDataBlock.ChargedPrecisionMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.LightSleeperMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#A918A7>");
                builder.Append($"{meleeLanguage.sleepingEnemiesMultiplier}.{meleeLanguage.light} ");
                builder.Append(archeTypeDataBlock.LightSleeperMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.ChargedSleeperMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#025531>");
                builder.Append($"{meleeLanguage.sleepingEnemiesMultiplier}.{meleeLanguage.heavy} ");
                builder.Append(archeTypeDataBlock.ChargedSleeperMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.LightEnvironmentMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#18A4A9>");
                builder.Append($"{meleeLanguage.environmentMultiplier}.{meleeLanguage.light} ");
                builder.Append(archeTypeDataBlock.LightEnvironmentMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            if (archeTypeDataBlock.ChargedEnvironmentMulti != 1f)
            {
                Divider(ref count, builder);

                builder.Append("<#75A2AA>");
                builder.Append($"{meleeLanguage.environmentMultiplier}.{meleeLanguage.heavy} ");
                builder.Append(archeTypeDataBlock.ChargedEnvironmentMulti);
                builder.Append(CLOSE_COLOR_TAG);
                count++;
            }

            builder.AppendLine("\n");

            builder.Append(sleepersDatas.VerboseKill(archeTypeDataBlock));

            return builder.ToString();
        }

        private static float FormatFloat(float value, int v)
        {
            return (float)Math.Round((decimal)value, v);
        }



        private int GetAmmoMax(ItemDataBlock itemDataBlock)
        {
            var ammoType = PlayerAmmoStorage.GetAmmoTypeFromSlot(itemDataBlock.inventorySlot);
            switch (ammoType)
            {
                case AmmoType.Standard:
                    return _playerDataBlock.AmmoStandardMaxCap;
                case AmmoType.Special:
                    return _playerDataBlock.AmmoSpecialMaxCap;
                case AmmoType.Class:
                    return _playerDataBlock.AmmoClassMaxCap;
                case AmmoType.CurrentConsumable:
                    return itemDataBlock.ConsumableAmmoMax;
            }
            return -1;
        }


        private int GetTotalAmmo(ArchetypeDataBlock archetypeDataBlock, ItemDataBlock itemDataBlock, bool isSentryGun = false)
        {
            var max = GetAmmoMax(itemDataBlock);

            var costOfBullet = archetypeDataBlock.CostOfBullet;

            if (isSentryGun)
            {
                costOfBullet = costOfBullet * itemDataBlock.ClassAmmoCostFactor;

                if (archetypeDataBlock.ShotgunBulletCount > 0f)
                {
                    costOfBullet *= archetypeDataBlock.ShotgunBulletCount;
                }
            }

            var maxBullets = (int)(max / costOfBullet);

            if (isSentryGun)
                return maxBullets;

            return maxBullets + archetypeDataBlock.DefaultClipSize;
        }

        private string GetRateOfFire(ArchetypeDataBlock archetypeDataBlock, eWeaponFireMode fireMode)
        {
            float value = -1F;

            switch (fireMode)
            {
                case eWeaponFireMode.Auto:
                case eWeaponFireMode.SentryGunAuto:
                    value = 1 / archetypeDataBlock.ShotDelay;
                    break;
                case eWeaponFireMode.SentryGunShotgunSemi:
                case eWeaponFireMode.Semi:
                    value = 1 / (archetypeDataBlock.ShotDelay + archetypeDataBlock.SpecialChargetupTime);
                    break;
                case eWeaponFireMode.SentryGunBurst:
                case eWeaponFireMode.Burst:
                    float shootsPerSecondSB = 1 / (archetypeDataBlock.BurstDelay + archetypeDataBlock.SpecialChargetupTime + (archetypeDataBlock.ShotDelay * (archetypeDataBlock.BurstShotCount -1)));
                    value = (shootsPerSecondSB) * archetypeDataBlock.BurstShotCount;
                    break;
            }

            return FormatFloat(value, 1).ToString();
        }



        private const string DIVIDER = " | ";
        private const string CLOSE_COLOR_TAG = "</color>";
    }
}
