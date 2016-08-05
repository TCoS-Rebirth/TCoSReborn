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
            throw new NotImplementedException("TODO Load skilldeck from DB");
        }

        public void sv2cl_SetSkills(List<FSkill_Type> aCharacterSkills, FSkill_Type[] aSkilldeckSkills)
        {
            sv_SetSkills(aCharacterSkills, aSkilldeckSkills);
        }

        public void sv_SetSkills(List<FSkill_Type> aCharacterSkills, FSkill_Type[] aSkilldeckSkills)
        {
            CharacterSkills = aCharacterSkills;
            SkilldeckSkills = aSkilldeckSkills;
        }

        void OnSkillDeckChanged()
        {
            Owner.SendToClient(PacketCreator.S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(Owner, CharacterSkills, SkilldeckSkills));
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
            OnSkillDeckChanged();
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

        //protected override void AddActiveSkill(RunningSkillData skillData)
        //{
        //    base.AddActiveSkill(skillData);
        //    Owner.SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(Owner, skillData, 0));
        //}

        //protected override void ClearLastSkill(RunningSkillData s)
        //{
        //    base.ClearLastSkill(s);
        //    Owner.SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL(Owner, s.Skill));
        //}

        public override void RunEvent(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            base.RunEvent(skill, fxEvent, flags, skillPawn, triggerPawn, targetPawn);
            Owner.SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, fxEvent.resourceID, flags, skillPawn, triggerPawn, targetPawn,
                fxEvent.ElapsedTime));
        }

        public override void RunEventL(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            base.RunEventL(skill, fxEvent, flags, skillPawn, triggerPawn, location, targetPawn);
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

        public void LoadDBSerializedDeck(string serializedData)
        {
            var skillIDs = serializedData.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (skillIDs.Length != 30)
            {
                Debug.LogError("serialized DBSkilldeck has wrong length");
                return;
            }
            var newDeck = new FSkill_Type[30];
            for (int i = 0; i < skillIDs.Length; i++)
            {
                if (skillIDs[i] != "0")
                {
                    int id;
                    if (int.TryParse(skillIDs[i], out id))
                    {
                        newDeck[i] = GetSkill(id);
                    }
                }
            }
            sv_SetSkills(CharacterSkills, newDeck);
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

        #region FromOld_FixMe
        /*  

        /// <summary>
        ///     Initiated by the client, indicates a skill use request. Executes the requested skill (if available) and handles
        ///     errors with skillcasting
        /// </summary>
        public void ClientUseSkill(int slotIndex, int targetID, Vector3 camPos, Vector3 targetPosition, float clientTime)
        {
            if (Mathf.Abs(clientTime - Time.time) > 0.5f)
            {
                SendToClient(PacketCreator.S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME(Time.time));
            }
            var s = ActiveSkillDeck.GetSkillFromActiveTier(slotIndex);
            var result = ESkillStartFailure.SSF_INVALID_SKILL;
            if (s != null)
            {
                result = UseSkill(s.resourceID, targetID, targetPosition, clientTime, camPos);
            }
            if (result == ESkillStartFailure.SSF_ALLOWED)
            {
                ActiveSkillDeck.SetActiveSlot(slotIndex);
            }
            else
            {
                DebugChatMessage("Skill result: " + result);
            }
        }

        void OnLearnedSkill(FSkill_Type s)
        {
            var m = PacketCreator.S2C_GAME_SKILLS_SV2CL_LEARNSKILL(s);
            SendToClient(m);
        }

        public override void OnDamageCaused(SkillApplyResult sap)
        {
            var m = PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTDAMAGE(sap.skillTarget.RelevanceID,
                sap.appliedSkill.resourceID, sap.damageCaused, sap.damageResisted);
            SendToClient(m);
        }

        public override void OnHealingCaused(SkillApplyResult sap)
        {
            if (sap.healCaused > 0)
            {
                var m = PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTHEAL(sap.skillTarget.RelevanceID,
                    sap.appliedSkill.resourceID, sap.healCaused);
                SendToClient(m);
            }
        }

        public override void OnStatChangeCaused(SkillApplyResult sap)
        {
            SendToClient(
                PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTSTATE(sap.skillTarget.RelevanceID,
                    sap.appliedSkill.resourceID, sap.appliedEffect.resourceID, sap.statChange));
        }
        */
        #endregion

    }
}