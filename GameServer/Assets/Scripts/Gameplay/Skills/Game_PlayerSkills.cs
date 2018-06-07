using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Database.Static;
using Gameplay.Entities;
using Gameplay.Items;
using Gameplay.Skills.Events;
using Network;
using UnityEngine;

namespace Gameplay.Skills
{
    public class Game_PlayerSkills : Game_Skills
    {

        PlayerCharacter Owner;

        Dictionary<FSkill_Type, Item_SkillToken[]> Tokens = new Dictionary<FSkill_Type, Item_SkillToken[]>();

        public override void Init(Character character)
        {
            base.Init(character);
            Owner = character as PlayerCharacter;
            var cSkills = new List<FSkill_Type>();
            for (var i = 0; i < Owner.dbRef.Skills.Count; i++)
            {
                var dbs = Owner.dbRef.Skills[i];
                var s = GameData.Get.skillDB.GetSkill(dbs.ResourceId);
                if (s != null)
                {
                    for (var j = 0; j < dbs.SigilSlots; j++)
                    {
                        AddTokenSlot(s);
                    }
                    cSkills.Add(s);
                }
            }
            sv_SetSkills(cSkills);
            LoadDBSerializedDeck(Owner.dbRef.SerializedSkillDeck);
            var levelData = LevelProgression.Get.GetDataForLevel(Owner.Stats.GetFameLevel());
            mTiers = levelData.combatTierRows;
            mTierSlots = levelData.combatTierColumns;
        }

        public void sv2cl_SetSkills(List<FSkill_Type> aCharacterSkills, FSkill_Type[] aSkilldeckSkills)
        {
            sv_SetSkills(aCharacterSkills);
            SkilldeckSkills = aSkilldeckSkills;
            Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(Owner, aCharacterSkills, SkilldeckSkills));
        }

        public void sv_SetSkills(List<FSkill_Type> aCharacterSkills)
        {
            CharacterSkills = aCharacterSkills;
        }

        /// <summary>
        ///     Initiated by the client on Skilldeck assignments. Updates the serverside skilldeck
        /// </summary>
        public void SetSkillDeck(int[] newSkillDeck)
        {
            if (newSkillDeck.Length != 30)
            {
                Debug.LogWarning("SetSkillDeck length mismatch");
                return;
            }
            var newdeckSkills = new FSkill_Type[30];
            for (var i = 0; i < newSkillDeck.Length; i++)
            {
                if (newSkillDeck[i] != 0)
                {
                    newdeckSkills[i] = GetSkill(newSkillDeck[i]);
                }
            }
            sv2cl_SetSkills(CharacterSkills, newdeckSkills);
        }

        public override int GetTokenSlots(FSkill_Type skill)
        {
            Item_SkillToken[] slot;
            return Tokens.TryGetValue(skill, out slot) ? slot.Length : 0;
        }

        public override void AddTokenSlot(FSkill_Type skill)
        {
            Item_SkillToken[] slots;
            if (!Tokens.TryGetValue(skill, out slots))
            {
                if (HasSkill(skill.resourceID))
                {
                    Tokens.Add(skill, new Item_SkillToken[1]);
                }
                return;
            }
            if (slots.Length >= MAX_TOKEN_SLOTS) return;
            Array.Resize(ref slots, slots.Length + 1);
            Tokens[skill] = slots;
            Debug.Log("TODO notify client about skilltoken update");
        }

        public override ESkillStartFailure ExecuteIndex(int index, int targetID, Vector3 targetPosition, float time)
        {
            if (Mathf.Abs(Time.time - time) > 0.5f)
            {
                Owner.ResyncClientTime();
            }
            return base.ExecuteIndex(index, targetID, targetPosition, Time.time);
        }

        protected override void AddActiveSkill(FSkill_Type aSkill, Character target, float aStartTime, float aDuration, float aSkillSpeed, bool aFreezeMovement, bool aFreezeRotation, int aTokenItemID, int AnimVarNr, Vector3 aLocation, Quaternion aRotation)
        {
            base.AddActiveSkill(aSkill, target, aStartTime, aDuration, aSkillSpeed, aFreezeMovement, aFreezeRotation, aTokenItemID, AnimVarNr, aLocation, aRotation);
            Owner.SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(Owner, LastSkill, 0));
        }

        protected override void sv2cl_ClearLastSkill()
        {
            base.sv2cl_ClearLastSkill();
            Owner.SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL());
        }

        public override void sv2clrel_RunEvent(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            base.sv2clrel_RunEvent(skill, fxEvent, flags, skillPawn, triggerPawn, targetPawn);
            Owner.SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, fxEvent.resourceID, flags, skillPawn, triggerPawn, targetPawn,
                fxEvent.ElapsedTime));
        }

        public override void sv2clrel_RunEventL(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            base.sv2clrel_RunEventL(skill, fxEvent, flags, skillPawn, triggerPawn, location, targetPawn);
            Owner.SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, fxEvent.resourceID, flags, skillPawn, triggerPawn, targetPawn, location,
                fxEvent.ElapsedTime));
        }

        public string DBSerializeDeck()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < SkilldeckSkills.Length; i++)
            {
                if (SkilldeckSkills[i] == null)
                {
                    sb.Append("0");
                }
                else
                {
                    sb.Append(SkilldeckSkills[i].resourceID);
                }
                sb.Append("|");
            }
            return sb.ToString().TrimEnd('|');
        }

        void LoadDBSerializedDeck(string serializedData)
        {
            var skillIDs = serializedData.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (skillIDs.Length != 30)
            {
                Debug.LogWarning("serialized DBSkilldeck has wrong length");
            }
            var newDeck = new FSkill_Type[30];
            for (var i = 0; i < skillIDs.Length; i++)
            {
                if (skillIDs[i] == "0") continue;
                int id;
                if (int.TryParse(skillIDs[i], out id))
                {
                    newDeck[i] = GetSkill(id);
                }
            }
            SkilldeckSkills = newDeck;
        }

        /// <summary>
        ///     Adds a skill to this characters skill-list (if found by the given <see cref="id" />) TODO check requirements like level, class points etc
        /// </summary>
        /// <param name="id"></param>
        public void LearnSkill(int id)
        {
            var s = GameData.Get.skillDB.GetSkill(id);
            if (s != null)
            {
                LearnSkill(s);
            }
            else
            {
                Debug.Log("(LearnSkill) missing: " + id);
            }
        }

        public List<SkillDeckSkill> GetSkillDeckSkills()
        {
            var sds = new List<SkillDeckSkill>();
            for (var i = 0; i < SkilldeckSkills.Length; i++)
            {
                if (SkilldeckSkills[i] != null)
                {
                    sds.Add(new SkillDeckSkill {Type = SkilldeckSkills[i], AbsoluteDeckSlot = i});
                }
            }
            return sds;
        } 

        public class SkillDeckSkill
        {
            public FSkill_Type Type;
            public int AbsoluteDeckSlot;
        }

    }
}