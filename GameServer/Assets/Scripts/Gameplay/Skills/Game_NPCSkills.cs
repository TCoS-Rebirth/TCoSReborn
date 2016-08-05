using System;
using Common.UnrealTypes;
using Gameplay.Items;
using UnityEngine;
using Database.Static;
using Gameplay.Entities;

namespace Gameplay.Skills
{
    public class Game_NPCSkills : Game_Skills
    {

        NpcCharacter Owner;

        public NPC_SkillDeck CurrentNPCSkillDeck;
        float LongestAttack;
        float LongestDebuff;
        float LongestBuff;
        bool CanHeal;
        bool mQueueSkillAnimation;
        Vector3 mSkillLocation;
        Quaternion mSkillRotation;
        int mQueueAnimVariation;
        Item_Type mQueueTracerItem;
        float mQueueTime;

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as NpcCharacter;
        }

        protected override void cl_AddActiveSkill(int aSkillID, float aStartTime, float aDuration, float aSkillSpeed, bool aFreezeMovement, bool aFreezeRotation, int aTokenItemID, int AnimVarNr, Vector3 aLocation, Quaternion aRotation)
        {
            base.cl_AddActiveSkill(aSkillID, aStartTime, aDuration, aSkillSpeed, aFreezeMovement, aFreezeRotation, aTokenItemID, AnimVarNr, aLocation, aRotation);
            mQueueSkillAnimation = true;                                        
            mSkillLocation = aLocation;                                                
            mSkillRotation = aRotation;                                                
            mQueueAnimVariation = AnimVarNr;                                       
            mQueueTracerItem = Database.Static.GameData.Get.itemDB.GetItemType(aTokenItemID);
            mQueueTime = Time.time;                                    
        }

        public void sv_SetSkills(NPC_SkillDeck aSkilldeck /*, NPC_Equipment aEquipment*/)
        {
            CurrentNPCSkillDeck = aSkilldeck;
            for (var i = 0; i < aSkilldeck.Tiers.Length; i++)
            {
                for (var j = 0; j < aSkilldeck.Tiers[i].Skills.Length; j++)
                {
                    var s = aSkilldeck.Tiers[i].Skills[j];
                    if (s == null) continue;
                    CharacterSkills.Add(s);
                    SkilldeckSkills[i*aSkilldeck.Tiers.Length + j] = s;
                }
            }
        }
    }
}