using GameData;
using System.Text;
using WeaponStatShower.Utils.Language.Models;

namespace WeaponStatShower.Utils
{
    internal class SleepersDatas
    {
        private readonly struct EnemyData
        {
            public readonly float Health;
            public readonly float HeadMultiplier;
            public readonly float BackMultiplier;
            public readonly bool IsArmored;

            public EnemyData(float health, float headMultiplier, float backMultiplier, bool isArmored)
            {
                Health = health;
                HeadMultiplier = headMultiplier;
                BackMultiplier = backMultiplier;
                IsArmored = isArmored;
            }
        }

        private readonly Dictionary<string, EnemyData> _enemyDatas = new();
        private readonly SleepersLanguageModel sleepersLanguageDatas;

        public SleepersDatas(string[] activatedSleepers, SleepersLanguageModel sleepersLanguageDatas)
        {
            this.sleepersLanguageDatas = sleepersLanguageDatas;
            foreach (string monsterRaw in activatedSleepers)
            {
                string monster = monsterRaw.Trim();
                switch (monster)
                {
                    case "DETAILS_ONLY":
                    case "DESCRIPTION_ONLY":
                    case "NONE":
                        _enemyDatas.Clear();
                        return;

                    case "ALL":
                        _enemyDatas.Clear();
                        _enemyDatas.TryAdd(sleepersLanguageDatas.striker, new EnemyData(20, 3, 2, false));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.shooter, new EnemyData(30, 5, 2, false));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.scout, new EnemyData(42, 3, 2, false));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.bigStriker, new EnemyData(120, 1.5f, 2, false));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.bigShooter, new EnemyData(150, 2, 2, false));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.charger, new EnemyData(30, 1, 2, true));
                        _enemyDatas.TryAdd(sleepersLanguageDatas.chargerScout, new EnemyData(60, 1, 2, true));
                        return;

                    case "STRIKER":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.striker, new EnemyData(20, 3, 2, false));
                        break;

                    case "SHOOTER":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.shooter, new EnemyData(30, 5, 2, false));
                        break;

                    case "SCOUT":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.scout, new EnemyData(42, 3, 2, false));
                        break;

                    case "BIG_STRIKER":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.bigStriker, new EnemyData(120, 1.5f, 2, false));
                        break;

                    case "BIG_SHOOTER":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.bigShooter, new EnemyData(150, 2, 2, false));
                        break;

                    case "CHARGER":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.charger, new EnemyData(30, 1, 2, true));
                        break;

                    case "CHARGER_SCOUT":
                        _enemyDatas.TryAdd(sleepersLanguageDatas.chargerScout, new EnemyData(60, 1, 2, true));
                        break;

                    default:
                        WeaponStatShowerPlugin.LogWarning("You inserted an incorrect value in the config: " + monster);
                        break;
                }
            }
        }

        public string VerboseKill(ArchetypeDataBlock archetypeDB)
        {
            float damage = archetypeDB.Damage * (archetypeDB.ShotgunBulletCount > 0 ? archetypeDB.ShotgunBulletCount : 1);
            float prcnMult = archetypeDB.PrecisionDamageMulti;
            return BuildKillString(prcnMult, (_, __) => damage);
        }

        internal string? VerboseKill(MeleeArchetypeDataBlock meleeArchetypeDB)
        {
            float baseDamage = meleeArchetypeDB.ChargedAttackDamage * meleeArchetypeDB.ChargedSleeperMulti;
            float prcnMult = meleeArchetypeDB.ChargedPrecisionMulti;
            return BuildKillString(prcnMult, (enemyName, _) =>
            {
                float damage = baseDamage;
                if (enemyName.Contains("SCOUT") || enemyName.Contains("哨兵") || enemyName.Contains("黑触"))
                    damage /= meleeArchetypeDB.ChargedSleeperMulti;
                return damage;
            });
        }

        private string BuildKillString(float prcnMult, Func<string, EnemyData, float> getDamage)
        {
            StringBuilder sb = new();
            int count = 0;
            int index = 0;
            int total = _enemyDatas.Count - 1;

            foreach (var (enemyName, data) in _enemyDatas)
            {
                float damage = getDamage(enemyName, data);
                List<string> killPlace = new(4);

                if (canKillOnOccipit(damage, prcnMult, data))
                    killPlace.Add(sleepersLanguageDatas.occipit);

                if (canKillOnHead(damage, prcnMult, data))
                    killPlace.Add(sleepersLanguageDatas.head);

                if (canKillOnBack(damage, data))
                    killPlace.Add(sleepersLanguageDatas.back);

                if (canKillOnChest(damage, data))
                    killPlace.Add(sleepersLanguageDatas.chest);

                if (killPlace.Count > 0)
                {
                    if (count % 2 == 1)
                        sb.Append(" | ");
                    killPlace.Reverse();
                    sb.Append(enemyName + ": [" + string.Join(",", killPlace) + "]");
                    if (count++ % 2 == 1 && index != total)
                        sb.AppendLine();
                }
                index++;
            }

            if (count > 1 && sb.Length > 0 && sb[^1] != '\n')
            {
                sb.AppendLine(); 
            }
            return sb.ToString();
        }

        private bool canKillOnChest(float damage, EnemyData data) => damage >= data.Health;

        private bool canKillOnBack(float damage, EnemyData data) => damage * data.BackMultiplier >= data.Health;

        private bool canKillOnHead(float damage, float prcnMultiplier, EnemyData data)
        {
            if (isArmored(data))
                return damage * data.HeadMultiplier >= data.Health;
            return damage * prcnMultiplier * data.HeadMultiplier >= data.Health;
        }

        private bool canKillOnOccipit(float damage, float prcnMultiplier, EnemyData data)
        {
            if (isArmored(data))
                return damage * data.BackMultiplier * data.HeadMultiplier >= data.Health;
            return damage * prcnMultiplier * data.BackMultiplier * data.HeadMultiplier >= data.Health;
        }

        private bool isArmored(EnemyData data) => data.IsArmored;
    }
}
