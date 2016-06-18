using Common;
using Database.Static;
using Gameplay.Skills;
using Network;
using UnityEngine;

namespace Gameplay.Entities
{
    public sealed partial class PlayerCharacter
    {

        #region Skills

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
            ActiveSkillDeck.Reset();
            for (var i = 0; i < newSkillDeck.Length; i++)
            {
                if (newSkillDeck[i] <= 0)
                {
                    continue;
                }
                ActiveSkillDeck[i] = GetSkill(newSkillDeck[i]);
            }
            SendToClient(PacketCreator.S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(this, Skills,
                ActiveSkillDeck.ToArray()));
        }

        /// <summary>
        ///     Adds a skill to this characters skill-list (if found by the given <see cref="id" />) TODO check requirements like
        ///     level, class points etc
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

        protected override void OnLearnedSkill(FSkill s)
        {
            var m = PacketCreator.S2C_GAME_SKILLS_SV2CL_LEARNSKILL(s);
            SendToClient(m);
        }

        protected override void OnStartCastSkill(RunningSkillContext s)
        {
            base.OnStartCastSkill(s);
            SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(this, s));
        }

        protected override void OnEndCastSkill(RunningSkillContext s)
        {
            base.OnEndCastSkill(s);
            SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL(this, s.ExecutingSkill));
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

        #endregion

    }
}
