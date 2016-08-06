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
        public float LongestAttack;
        public float LongestDebuff;
        public float LongestBuff;
        public bool CanHeal;
        //bool mQueueSkillAnimation;
        //Vector3 mSkillLocation;
        //Quaternion mSkillRotation;
        //int mQueueAnimVariation;
        //Item_Type mQueueTracerItem;
        //float mQueueTime;

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as NpcCharacter;
            if (Owner.Type.SkillDeck != null)
            {
                sv_SetSkills(Owner.Type.SkillDeck);
            }
        }

        public override int GetSkilldeckRowCount()
        {
            return CurrentNPCSkillDeck == null ? 0 : CurrentNPCSkillDeck.Tiers.Count;
        }

        public override int GetSkilldeckColumnCount()
        {
            return CurrentNPCSkillDeck == null ? 0 : CurrentNPCSkillDeck.Tiers[0].Skills.Length;
        }

        //protected override void AddActiveSkill(FSkill_Type aSkill, Character target, float aStartTime, float aDuration, float aSkillSpeed, bool aFreezeMovement, bool aFreezeRotation, int aTokenItemID, int AnimVarNr, Vector3 aLocation, Quaternion aRotation)
        //{
            //base.AddActiveSkill(aSkill, target, aStartTime, aDuration, aSkillSpeed, aFreezeMovement, aFreezeRotation, aTokenItemID, AnimVarNr, aLocation, aRotation);
            //mQueueSkillAnimation = true;                                        
            //mSkillLocation = aLocation;                                                
            //mSkillRotation = aRotation;                                                
            //mQueueAnimVariation = AnimVarNr;                                       
            //mQueueTracerItem = GameData.Get.itemDB.GetItemType(aTokenItemID);
            //mQueueTime = Time.time;                                    
        //}

        public void sv_SetSkills(NPC_SkillDeck aSkilldeck /*, NPC_Equipment aEquipment*/)
        {
            CurrentNPCSkillDeck = aSkilldeck;
            for (var i = 0; i < aSkilldeck.Tiers.Count; i++)
            {
                for (var j = 0; j < aSkilldeck.Tiers[i].Skills.Length; j++)
                {
                    var s = aSkilldeck.Tiers[i].Skills[j];
                    if (s == null) continue;
                    CharacterSkills.Add(s);
                    SkilldeckSkills[i*aSkilldeck.Tiers.Count + j] = s;
                }
            }
        }
    }
}