using System;
using System.Collections.Generic;
using System.Text;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills
{
    [Serializable]
    public class SkillDeck : ScriptableObject
    {
        [SerializeField, ReadOnly] int activeTier;

        [SerializeField, HideInInspector] int lastselectedSkillSlot;

        [SerializeField, ReadOnly] List<SkillDeckTier> tiers = new List<SkillDeckTier>();

        public FSkill this[int index]
        {
            get
            {
                var tier = index/5;
                if (index < 0 || tier >= tiers.Count)
                {
                    return null;
                }
                var tierIndex = index%5;
                return tiers[tier][tierIndex];
            }
            set
            {
                var tier = index/5;
                if (index < 0 || tier >= tiers.Count)
                {
                    return;
                }
                var tierIndex = index%5;
                var previous = tiers[tier][tierIndex];
                if (previous != null)
                {
                    previous.DeckSlot = -1;
                }
                tiers[tier][tierIndex] = value;
                if (value != null)
                {
                    value.DeckSlot = index;
                }
            }
        }

        public List<SkillDeckTier> Tiers
        {
            get { return tiers; }
        }

        public int Length
        {
            get { return tiers.Count*5; }
        }

        public FSkill[] ToArray()
        {
            var arr = new FSkill[tiers.Count*5];
            for (var i = 0; i < tiers.Count; i++)
            {
                for (var index = 0; index < 5; index++)
                {
                    arr[i*5 + index] = tiers[i][index];
                }
            }
            return arr;
        }

        public void SetActiveSlot(int index)
        {
            lastselectedSkillSlot = index;
        }

        public FSkill GetSkillFromLastActiveSlot()
        {
            return tiers[activeTier][lastselectedSkillSlot];
            //return activeTier[lastselectedSkillSlot];
        }

        public FSkill GetSkillFromActiveTier(int slotIndex)
        {
            return tiers[activeTier][slotIndex];
            //return activeTier[slotIndex];
        }

        public int GetActiveTierIndex()
        {
            //return tiers.IndexOf(activeTier);
            return activeTier;
        }

        public void RollDeck() //TODO: dont roll, but reset on: skill miss, same skill twice, combo of 9
        {
            var currentTierIndex = GetActiveTierIndex();
            currentTierIndex++;
            if (currentTierIndex >= tiers.Count)
            {
                currentTierIndex = 0;
            }
            else
            {
                while (!tiers[currentTierIndex].HasSkills)
                {
                    currentTierIndex++;
                    if (currentTierIndex >= tiers.Count)
                    {
                        currentTierIndex = 0;
                        break;
                    }
                }
            }
            activeTier = currentTierIndex;
        }

        public void ResetRoll()
        {
            if (tiers.Count == 0)
            {
                return;
            }
            activeTier = 0;
        }

        public void Reset()
        {
            for (var i = 0; i < tiers.Count; i++)
            {
                for (var s = 0; s < tiers[i].skills.Length; s++)
                {
                    if (tiers[i].skills[s] != null)
                    {
                        tiers[i].skills[s].DeckSlot = -1;
                    }
                    tiers[i].skills[s] = null;
                }
            }
            ResetRoll();
        }

        public string DBSerialize()
        {
            var sb = new StringBuilder();
            sb.Append(tiers.Count);
            sb.Append('#');
            for (var i = 0; i < tiers.Count; i++)
            {
                for (var t = 0; t < tiers[i].skills.Length; t++)
                {
                    if (tiers[i].skills[t] == null)
                    {
                        sb.Append('0');
                    }
                    else
                    {
                        sb.Append(tiers[i].skills[t].resourceID);
                    }
                    if (t != tiers[i].skills.Length - 1)
                    {
                        sb.Append(',');
                    }
                }
                if (i != tiers.Count - 1)
                {
                    sb.Append('|');
                }
            }
            return sb.ToString();
        }

        public void LoadForPlayer(string serializedData, PlayerCharacter playerCharacter)
        {
            var count = serializedData.Split('#');
            if (count.Length == 2)
            {
                var numCount = 0;
                if (int.TryParse(count[0], out numCount))
                {
                    var stiers = count[1].Split('|');
                    if (stiers.Length == numCount)
                    {
                        for (var i = 0; i < stiers.Length; i++)
                        {
                            tiers.Add(new SkillDeckTier());
                            var skillids = stiers[i].Split(',');
                            for (var s = 0; s < skillids.Length; s++)
                            {
                                int skillID;
                                if (int.TryParse(skillids[s], out skillID))
                                {
                                    var sk = playerCharacter.GetSkill(skillID);
                                    if (sk != null)
                                    {
                                        tiers[i].skills[s] = sk;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadForNPC(NpcCharacter npc)
        {
            for (var i = 0; i < tiers.Count; i++)
            {
                for (var t = 0; t < tiers[i].skills.Length; t++)
                {
                    if (tiers[i].skills[t] != null)
                    {
                        if (!tiers[i].skills[t].name.Contains("(Clone)"))
                        {
                            tiers[i].skills[t] = Instantiate(tiers[i].skills[t]);
                            tiers[i].skills[t].DeepClone();
                            if (!npc.Skills.Contains(tiers[i].skills[t]))
                            {
                                npc.Skills.Add(tiers[i].skills[t]);
                            }
                        }
                    }
                }
            }
        }

        public List<SkillDeckSkill> GetSkillDeckSkills()
        {
            var sds = new List<SkillDeckSkill>();
            for (var t = 0; t < tiers.Count; t++)
            {
                for (var s = 0; s < tiers[t].skills.Length; s++)
                {
                    if (tiers[t].skills[s] != null)
                    {
                        sds.Add(new SkillDeckSkill(t, tiers[t].skills[s].resourceID, s, t*tiers[t].skills.Length + s));
                    }
                }
            }
            return sds;
        }

        [Serializable]
        public class SkillDeckTier
        {
            public FSkill[] skills = new FSkill[5];

            public FSkill this[int index]
            {
                get
                {
                    if (index < 0 || index > 4)
                    {
                        return null;
                    }
                    return skills[index];
                }
                set
                {
                    if (index < 0 || index > 4)
                    {
                        return;
                    }
                    skills[index] = value;
                }
            }

            public bool HasSkills
            {
                get
                {
                    for (var i = 0; i < skills.Length; i++)
                    {
                        if (skills[i] != null)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        public struct SkillDeckSkill
        {
            public int tier;
            public int skillID;
            public int deckSlot;
            public int totalDeckSlot;

            public SkillDeckSkill(int tier, int skillID, int deckSlot, int totalDeckSlot)
            {
                this.tier = tier;
                this.skillID = skillID;
                this.deckSlot = deckSlot;
                this.totalDeckSlot = totalDeckSlot;
            }
        }
    }
}