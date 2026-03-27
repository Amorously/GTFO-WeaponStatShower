using GameData;
using System.Text;
using WeaponStatShower.Utils.Language.Models;

namespace WeaponStatShower.Utils
{
    internal class SleepersDatas
    {
        private readonly Dictionary<string, float[]> EnemyDatas = new Dictionary<string, float[]>();
        private SleepersLanguageModel sleepersLanguageDatas;

        public SleepersDatas(string[] activatedSleepers, SleepersLanguageModel sleepersLanguageDatas)
        {
            this.sleepersLanguageDatas = sleepersLanguageDatas;
            foreach (string monster in activatedSleepers)
            {
                switch (monster.Trim())
                {
                    case "DETAILS_ONLY":
                    case "NONE":
                        EnemyDatas.Clear(); //just to be sure
                        return;
                    case "ALL":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.striker, new float[] { 20, 3, 2, 0 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.shooter, new float[] { 30, 5, 2, 0 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.scout, new float[] { 42, 3, 2, 0 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.bigStriker, new float[] { 120, 1.5F, 2, 0 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.bigShooter, new float[] { 150, 2, 2, 0 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.charger, new float[] { 30, 1, 2, 1 });
                        EnemyDatas.TryAdd(sleepersLanguageDatas.chargerScout, new float[] { 60, 1, 2, 1 });
                        return;
                    case "STRIKER":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.striker, new float[] { 20, 3, 2, 0 });
                        break;
                    case "SHOOTER":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.shooter, new float[] { 30, 5, 2, 0 });
                        break;
                    case "SCOUT":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.scout, new float[] { 42, 3, 2, 0 });
                        break;
                    case "BIG_STRIKER":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.bigStriker, new float[] { 120, 1.5F, 2, 0 });
                        break;
                    case "BIG_SHOOTER":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.bigShooter, new float[] { 150, 2, 2, 0 });
                        break;
                    case "CHARGER":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.charger, new float[] { 30, 1, 2, 1 });
                        break;
                    case "CHARGER_SCOUT":
                        EnemyDatas.TryAdd(sleepersLanguageDatas.chargerScout, new float[] { 60, 1, 2, 1 });
                        break;
                    default:
                        WeaponStatShowerPlugin.LogWarning("You inserted an incorrect value in the config: " + monster.Trim());
                        break;
                }
            }
        }

        public string VerboseKill(ArchetypeDataBlock archetypeDataBlock)
        {
            StringBuilder builder = new StringBuilder();
            float damage = archetypeDataBlock.Damage * (archetypeDataBlock.ShotgunBulletCount > 0 ? archetypeDataBlock.ShotgunBulletCount : 1);
            float prcsnMultiplier = archetypeDataBlock.PrecisionDamageMulti;
            int count = 0;

            for (int i = 0; i < EnemyDatas.Count; i++)
            {
                string enemyName = EnemyDatas.Keys.ElementAt(i);
                List<string> killPlace = new List<string>();
                float[] currEnemyDatas = EnemyDatas[enemyName];

                if (canKillOnOccipit(damage, prcsnMultiplier, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.occipit);

                if (canKillOnHead(damage, prcsnMultiplier, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.head);

                if (canKillOnBack(damage, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.back);

                if (canKillOnChest(damage, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.chest);


                if (killPlace.Count > 0)
                {
                    if (count % 2 == 1)
                        builder.Append(" | ");

                    killPlace.Reverse();
                    builder.Append(enemyName + ": [" + string.Join(",", killPlace.ToArray()) + "]");

                    if (count++ % 2 == 1 && i != EnemyDatas.Count - 1)
                    {
                        builder.Append('\n');
                    }
                }
            }

            return builder.ToString();
        }

        internal string? VerboseKill(MeleeArchetypeDataBlock archeTypeDataBlock)
        {
            StringBuilder builder = new StringBuilder();
            float damage = archeTypeDataBlock.ChargedAttackDamage * archeTypeDataBlock.ChargedSleeperMulti;
            float prcsnMultiplier = archeTypeDataBlock.ChargedPrecisionMulti;
            int count = 0;

            for (int i = 0; i < EnemyDatas.Count; i++)
            {
                string enemyName = EnemyDatas.Keys.ElementAt(i);
                if (enemyName.Contains("SCOUT") || enemyName.Contains("哨兵") || enemyName.Contains("黑触"))
                    damage /= archeTypeDataBlock.ChargedSleeperMulti;

                List<string> killPlace = new List<string>();
                float[] currEnemyDatas = EnemyDatas[enemyName];

                if (canKillOnOccipit(damage, prcsnMultiplier, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.occipit);

                if (canKillOnHead(damage, prcsnMultiplier, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.head);

                if (canKillOnBack(damage, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.back);

                if (canKillOnChest(damage, currEnemyDatas))
                    killPlace.Add(sleepersLanguageDatas.chest);


                if (killPlace.Count > 0)
                {
                    if (count % 2 == 1)
                        builder.Append(" | ");

                    killPlace.Reverse();
                    builder.Append(enemyName + ": [" + string.Join(",", killPlace.ToArray()) + "]");

                    if (count++ % 2 == 1 && i != EnemyDatas.Count - 1)
                    {
                        builder.Append('\n');
                    }
                }
            }

            return builder.ToString();
        }

        private bool canKillOnChest(float damage, float[] currEnemyDatas)
        {
            return damage >= currEnemyDatas[0];
        }



        private bool canKillOnBack(float damage, float[] currEnemyDatas)
        {
            return damage * currEnemyDatas[2] >= currEnemyDatas[0];
        }

        private bool canKillOnHead(float damage, float prcsnMultiplier, float[] currEnemyDatas)
        {
            if (isArmored(currEnemyDatas))
            {
                return damage * currEnemyDatas[1] >= currEnemyDatas[0];
            }
            else
            {
                return damage * prcsnMultiplier * currEnemyDatas[1] >= currEnemyDatas[0];
            }
        }

        private bool canKillOnOccipit(float damage, float prcsnMultiplier, float[] currEnemyDatas)
        {
            if (isArmored(currEnemyDatas))
            {
                return damage * currEnemyDatas[2] * currEnemyDatas[1] >= currEnemyDatas[0];
            }
            else
            {
                return damage * prcsnMultiplier * currEnemyDatas[2] * currEnemyDatas[1] >= currEnemyDatas[0];

            }
        }

        private bool isArmored(float[] currEnemyDatas)
        {
            return currEnemyDatas[3] == 1f;
        }

        
    }
}
