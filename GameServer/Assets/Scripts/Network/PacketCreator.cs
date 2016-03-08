using System.Collections.Generic;
using Common;
using Common.UnrealTypes;
using Database.Dynamic.Internal;
using Gameplay.Conversations;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using Gameplay.Entities.Players;
using Gameplay.Items;
using Gameplay.Skills;
using UnityEngine;
using Utility;
using World;
using Gameplay.Quests;
using Database.Static;

namespace Network
{
    public static class PacketCreator
    {
        static void WriteSv2cl_Item(Game_Item i, Message m)
        {
            m.WriteInt32(i.DBID); //korrekt
            m.WriteInt32(i.Type.resourceID); //korrekt
            m.WriteInt32((int) i.LocationType); //korrekt
            m.WriteByte(0);
            m.WriteInt32(i.LocationSlot); // korrekt
            m.WriteInt32(i.CharacterID);
            m.WriteInt32(i.StackSize);
            m.WriteByte(i.Attuned); //korrekt
            m.WriteByte(i.Color1); //korrekt
            m.WriteByte(i.Color2); //korrekt
            m.WriteInt32(0); //serial?
        }


        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON);
            m.WriteByte((byte) ch.CombatMode);
            return m;
        }

        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON()
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON);
            return m;
        }

        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON);
            m.WriteByte((byte) ch.equippedWeaponType);
            return m;
        }

        public static Message S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte) ch.CombatMode);
            m.WriteInt32(0); //possibly mainhand and offhand
            m.WriteInt32(0);
            return m;
        }

        public static Message S2R_GAME_EMOTES_SV2REL_EMOTE(Character ch, EContentEmote emote)
        {
            var m = new Message(GameHeader.S2R_GAME_EMOTES_SV2REL_EMOTE);
            m.WriteInt32(ch.RelevanceID);
            if (ch is NpcCharacter)
            {
                emote = (EContentEmote) ((int) emote - 1);
            }
            m.WriteByte((byte) emote);
            return m;
        }

        public static Message S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON);
            m.WriteInt32(ch.RelevanceID);
            return m;
        }

        public static Message S2C_PLAYER_ADD(PlayerCharacter pc)
        {
            var m = new Message(GameHeader.S2C_PLAYER_ADD);
            m.WriteInt32(pc.RelevanceID);
            m.WriteInt32(UnitConversion.ToUnreal(pc.Rotation).Yaw);
            m.WriteInt32(pc.MaxHealth);
            m.WriteFloat(pc.Physique);
            m.WriteFloat(pc.Morale);
            m.WriteFloat(pc.Concentration);
            m.WriteInt32(pc.FameLevel);
            m.WriteInt32(pc.PepRank);
            //pawn add stream
            m.WriteVector3(UnitConversion.ToUnreal(pc.Velocity));
            m.WriteVector3(UnitConversion.ToUnreal(pc.Position));
            m.WriteByte((byte) pc.Physics);
            m.WriteByte(pc.MoveFrame);
            m.WriteFloat(0); //PVPTimer
            m.WriteByte((byte) pc.PawnState);
            m.WriteInt32(0); //BitfieldInvulnerability SBGameClasses:4819 AGame_Pawn
            m.WriteFloat(pc.GroundSpeedModifier); //GroundSpeedModifier
            m.WriteInt32(0); //DebugFilter
            m.WriteInt32(0); //BitFieldHasPet_Invisible_JumpedFromLadder;
            m.WriteInt32(pc.ShiftableAppearance); //shiftableAppearance;
            m.WriteByteArray(pc.GetPackedLOD(0));
            m.WriteByteArray(pc.GetPackedLOD(1)); //<-----------fix length for all, or better implement it!
            m.WriteByteArray(pc.GetPackedLOD(2));
            m.WriteByteArray(pc.GetPackedLOD(3));
            m.WriteString(pc.Name);
            m.WriteString(pc.Guild != null ? pc.Guild.Name : "");
            m.WriteInt32(pc.Faction.ID);
            m.WriteFloat(pc.Health);
            m.WriteByte(0); //frozenFlags
            m.WriteInt32(pc.GetEffectiveMoveSpeed());
            m.WriteInt32(pc.StateRank);
            m.WriteByte((byte) pc.CombatMode); //HACK: following fields may have to be changed by this mode accordingly
            var it = pc.ItemManager.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
            m.WriteInt32(it != null ? it.Type.resourceID : 0);
            it = pc.ItemManager.GetEquippedItem(EquipmentSlot.ES_SHIELD);
            m.WriteInt32(it != null ? it.Type.resourceID : 0); //Offhandweapon! (does this even exist?)
            m.WriteInt32(pc.Effects.Count);
            for (var i = 0; i < pc.Effects.Count; i++)
            {
                m.WriteInt32(pc.Effects[i].resourceID);
            }
            return m;
        }

        public static Message S2C_NPC_ADD(NpcCharacter npc)
        {
            var m = new Message(GameHeader.S2C_NPC_ADD);
            m.WriteInt32(npc.RelevanceID);
            m.WriteInt32(npc.typeRef.resourceID);
            m.WriteInt32(-1); //ownerID?
            m.WriteVector3(UnitConversion.ToUnreal(npc.Position));
            m.WriteInt32(npc.MaxHealth);
            m.WriteFloat(npc.Physique);
            m.WriteFloat(npc.Morale);
            m.WriteFloat(npc.Concentration);
            m.WriteInt32(npc.FameLevel);
            m.WriteInt32(npc.PepRank);
            //NpcPawnStream
            m.WriteInt32(npc.typeRef.resourceID); //same as resourceID

            /*
            m.WriteInt32(npc.RelatedQuestIDs.Count);
            for (var i = 0; i < npc.RelatedQuestIDs.Count; i++)
            {
                Debug.Log("Dialog: " + npc.RelatedQuestIDs[i]);
                m.WriteInt32(npc.RelatedQuestIDs[i]);
            }
            */
            var relatedQuestIDs = npc.getRelatedQuestIDs();

            m.WriteInt32(relatedQuestIDs.Count);    //Number of related quest
            foreach(var questID in relatedQuestIDs)
            {
                m.WriteInt32(questID);              //Related quest ID array
            }           

            m.WriteVector3(UnitConversion.ToUnreal(npc.Destination)); //struct NetMovment [dest, flag]
            m.WriteByte((byte) npc.MovementFlags); //ENpcMovementFlag
            m.WriteInt32(-1); //target-RelID
            m.WriteVector3(UnitConversion.ToUnreal(npc.FocusLocation));
            m.WriteRotator(UnitConversion.ToUnreal(npc.Rotation));
            m.WriteByte((byte) npc.PawnState);
            m.WriteInt32((int) npc.ClassType); //unsure
            m.WriteInt32(0);
            m.WriteInt32(0); //DebugFilter?
            m.WriteInt32(npc.Invisible ? 1 : 0); //invisibility
            //NpcPawnStream end
            m.WriteInt32(npc.ShiftableAppearance); //shiftableAppearance
            m.WriteInt32(npc.Faction.ID); //Faction
            //NpcStatsStream
            m.WriteFloat(npc.Health); //health
            m.WriteByte(0); //frozenflags?
            m.WriteInt32(0); //movespeed?
            m.WriteInt32(npc.StateRank);
            //NpcStatsStream end
            //NpcCombatStateStream
            m.WriteByte((byte) npc.CombatMode);
            m.WriteInt32(0); //mainHandweapon?
            m.WriteInt32(0); //offhandweapon?
            //NpcCombatStateStream end
            m.WriteInt32(npc.Effects.Count);
            for (var i = 0; i < npc.Effects.Count; i++)
            {
                m.WriteInt32(npc.Effects[i].resourceID);
            }
            return m;
        }

        public static Message S2C_INTERACTIVELEVELELEMENT_ADD(InteractiveLevelElement ie)
        {
            var m = new Message(GameHeader.S2C_INTERACTIVELEVELELEMENT_ADD);
            m.WriteInt32(ie.RelevanceID); //relevanceID
            m.WriteInt32(ie.LevelObjectID); //leveObjectID
            m.WriteInt32(ie.IsEnabled ? 1 : 0); //isEnabled
            m.WriteInt32(ie.Invisible ? 1 : 0); //isHidden
            m.WriteRotator(UnitConversion.ToUnreal(ie.Rotation)); //NetRotation
            m.WriteVector3(UnitConversion.ToUnreal(ie.Position)); //NetLocation
            m.WriteByte((byte) ie.CollisionType); //collisionType
            m.WriteInt32(0);
            return m;
        }

        public static Message S2C_BASE_PAWN_REMOVE(Entity ro)
        {
            var m = new Message(GameHeader.S2C_BASE_PAWN_REMOVE);
            m.WriteInt32(ro.RelevanceID);
            return m;
        }

        public static Message S2C_LEVELOBJECT_REMOVE(Entity ro)
        {
            var m = new Message(GameHeader.S2C_LEVELOBJECT_REMOVE);
            m.WriteInt32(ro.RelevanceID);
            m.WriteInt32(1);
            return m;
        }

        public static Message S2R_GAME_ACTOR_SV2CLREL_SETENABLED(Entity ro)
        {
            var m = new Message(GameHeader.S2R_GAME_ACTOR_SV2CLREL_SETENABLED);
            m.WriteInt32(ro.RelevanceID);
            m.WriteInt32(0);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_TELEPORTTO(Entity ro)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_TELEPORTTO);
            m.WriteInt32(ro.RelevanceID);
            m.WriteVector3(UnitConversion.ToUnreal(ro.Position));
            m.WriteRotator(UnitConversion.ToUnreal(ro.Rotation));
            return m;
        }

        public static Message S2R_GAME_PLAYERPAWN_UPDATEROTATION(Character pc)
        {
            var m = new Message(GameHeader.S2R_GAME_PLAYERPAWN_UPDATEROTATION);
            m.WriteInt32(pc.RelevanceID);
            m.WriteInt32(UnitConversion.ToUnreal(pc.Rotation).Yaw);
            return m;
        }

        public static Message S2R_PLAYERPAWN_MOVE(PlayerCharacter pc)
        {
            var m = new Message(GameHeader.S2R_PLAYERPAWN_MOVE);
            m.WriteInt32(pc.RelevanceID);
            m.WriteInt32(pc.MoveFrame);
            m.WriteVector3(UnitConversion.ToUnreal(pc.Position));
            m.WriteVector3(UnitConversion.ToUnreal(pc.Velocity));
            m.WriteByte((byte) pc.Physics);
            return m;
        }

        public static Message S2R_GAME_NPCPAWN_SV2REL_MOVE(NpcCharacter npc)
        {
            var m = new Message(GameHeader.S2R_GAME_NPCPAWN_SV2REL_MOVE);
            m.WriteInt32(npc.RelevanceID);
            m.WriteByte((byte) npc.MovementFlags);
            m.WriteVector3(UnitConversion.ToUnreal(npc.Destination));
            return m;
        }

        public static Message S2R_GAME_NPCPAWN_SV2REL_STOPMOVEMENT(NpcCharacter npc)
        {
            var m = new Message(GameHeader.S2R_GAME_NPCPAWN_SV2REL_STOPMOVEMENT);
            m.WriteInt32(npc.RelevanceID);
            m.WriteVector3(UnitConversion.ToUnreal(npc.Position));
            return m;
        }

        public static Message S2C_GAME_CHAT_SV2CL_ONMESSAGE(string sender, string message, EGameChatRanges channel)
        {
            var m = new Message(GameHeader.S2C_GAME_CHAT_SV2CL_ONMESSAGE);
            m.WriteString(sender);
            m.WriteString(message);
            m.WriteInt32((int) channel);
            return m;
        }

        public static Message S2R_GAME_NPCPAWN_SV2REL_FOCUSLOCATION(NpcCharacter character)
        {
            var m = new Message(GameHeader.S2R_GAME_NPCPAWN_SV2REL_FOCUSLOCATION);
            m.WriteInt32(character.RelevanceID);
            m.WriteVector3(UnitConversion.ToUnreal(character.FocusLocation));
            return m;
        }

        public static Message S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(PlayerCharacter pc, SkillContext s)
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL);
            m.WriteInt32(s.ExecutingSkill.resourceID); //skillID
            m.WriteFloat(s.StartTime); //startTime
            m.WriteFloat(s.ExecutingSkill.GetSkillDuration(pc)); //(animation duration has to be manually collected)
            m.WriteFloat(s.ExecutingSkill.attackSpeed); //skillSpeed
            m.WriteInt32(s.ExecutingSkill.freezePawnMovement ? 1 : 0); //freezeMovement
            m.WriteInt32(s.ExecutingSkill.freezePawnRotation ? 1 : 0); //freezeRotation
            m.WriteInt32(s.SourceItemID); //aTokenItemID
            m.WriteInt32(s.ExecutingSkill.animationVariation); //animVarNr
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL(Character ch, SkillContext s)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(s.ExecutingSkill.resourceID);
            m.WriteFloat(s.StartTime);
            m.WriteFloat(s.ExecutingSkill.GetSkillDuration(ch)); // duration of skill
            m.WriteFloat(s.ExecutingSkill.attackSpeed); //skillSpeed, AnimationSpeed?
            m.WriteInt32(s.ExecutingSkill.freezePawnMovement ? 1 : 0); //freezemovement
            m.WriteInt32(s.ExecutingSkill.freezePawnRotation ? 1 : 0); //freezerotation
            m.WriteInt32(s.SourceItemID); //tokenItemID
            m.WriteInt32(s.ExecutingSkill.animationVariation); //animvarNr
            m.WriteVector3(UnitConversion.ToUnreal(s.TargetPosition));
            m.WriteRotator(UnitConversion.ToUnreal(ch.Rotation)); //TODO viewRotation?
            return m;
        }

        public static Message S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL(PlayerCharacter pc, FSkill s)
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL);
            return m;
        }

        public static Message S2R_GAME_EFFECTS_SV2CLREL_STARTREPLICATED(Character ch, int effectResourceID, int effectHandleID)
        {
            var m = new Message(GameHeader.S2R_GAME_EFFECTS_SV2CLREL_STARTREPLICATED);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(effectResourceID);
            m.WriteInt32(effectHandleID);
            return m;
        }

        public static Message S2R_GAME_EFFECTS_SV2CLREL_STOPREPLICATED(Character ch, int effectHandleID)
        {
            var m = new Message(GameHeader.S2R_GAME_EFFECTS_SV2CLREL_STOPREPLICATED);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(effectHandleID);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT(Character ch, EPawnEffectType effectType)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte) effectType);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECTDIRECT(Character ch, int effectID)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECTDIRECT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(effectID);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_STATICPLAYSOUND(Character ch, EPawnSound soundEffect, float volume)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_STATICPLAYSOUND);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte) soundEffect);
            m.WriteFloat(1f); //volume?
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Character ch, int skillID, int eventID, int flags, Character skillPawn, Character triggerPawn,
            Character targetPawn, float time)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(skillID);
            m.WriteInt32(eventID);
            m.WriteInt32(flags); //?
            m.WriteInt32(skillPawn != null ? skillPawn.RelevanceID : -1);
            m.WriteInt32(triggerPawn != null ? triggerPawn.RelevanceID : -1);
            m.WriteInt32(targetPawn != null ? targetPawn.RelevanceID : -1);
            m.WriteFloat(time);
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Character ch, int skillID, int eventID, int flags, Character skillPawn, Character triggerPawn,
            Character targetPawn, Vector3 location, float time)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(skillID);
            m.WriteInt32(eventID);
            m.WriteInt32(flags); //?
            m.WriteInt32(skillPawn != null ? skillPawn.RelevanceID : -1);
            m.WriteInt32(triggerPawn != null ? triggerPawn.RelevanceID : -1);
            m.WriteInt32(targetPawn != null ? targetPawn.RelevanceID : -1);
            m.WriteVector3(location);
            m.WriteFloat(time);
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2CLREL_UPDATEDUFFS(Character ch, List<DuffInfoData> duffs)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2CLREL_UPDATEDUFFS);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(duffs.Count);
            for (var i = 0; i < duffs.Count; i++)
            {
                m.WriteInt32(duffs[i].duff.resourceID);
                m.WriteInt32(duffs[i].stackCount); //stackCount
                m.WriteFloat(duffs[i].applyTime);
                m.WriteFloat(duffs[i].duration);
            }
            return m;
        }

        public static Message S2R_BASE_PAWN_SV2CLREL_DAMAGEACTIONS(Character ch, float factor)
        {
            var m = new Message(GameHeader.S2R_BASE_PAWN_SV2CLREL_DAMAGEACTIONS);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(1f); //damagefactor?
            return m;
        }

        public static Message S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTDAMAGE(int targetID, int skillID, int damageCaused, int damageResisted)
        {
            var m = new Message(GameHeader.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTDAMAGE);
            m.WriteInt32(skillID);
            m.WriteInt32(targetID);
            m.WriteInt32(damageCaused);
            m.WriteInt32(damageResisted);
            return m;
        }

        public static Message S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTHEAL(int targetID, int skillID, int healingCaused)
        {
            var m = new Message(GameHeader.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTHEAL);
            m.WriteInt32(skillID);
            m.WriteInt32(targetID);
            m.WriteInt32(healingCaused);
            return m;
        }

        public static Message S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTSTATE(int targetID, int skillID, int effectID, int amount)
        {
            var m = new Message(GameHeader.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTSTATE);
            m.WriteInt32(skillID);
            m.WriteInt32(targetID);
            m.WriteInt32(amount);
            m.WriteInt32(effectID);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2REL_COMBATMESSAGEDEATH(Character dead, Character killer)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2REL_COMBATMESSAGEDEATH);
            m.WriteInt32(killer.RelevanceID);
            m.WriteInt32(killer.RelevanceID);
            m.WriteInt32(dead.RelevanceID);
            return m;
        }

        public static Message S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH(Character killer)
        {
            var m = new Message(GameHeader.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH);
            m.WriteInt32(killer.RelevanceID);
            return m;
        }

        public static Message S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME(float serverTime)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME);
            m.WriteFloat(serverTime);
            return m;
        }

        public static Message S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN);
            m.WriteInt32(p.IsResting ? 0 : 0xff);
            return m;
        }

        public static Message S2C_GAME_SKILLS_SV2CL_LEARNSKILL(FSkill s)
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_LEARNSKILL);
            m.WriteInt32(s.resourceID);
            return m;
        }

        public static Message S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(PlayerCharacter p, List<FSkill> skills, FSkill[] deck)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS);
            m.WriteInt32(skills.Count);
            for (var i = 0; i < skills.Count; i++)
            {
                m.WriteInt32(skills[i].resourceID);
            }
            m.WriteInt32(deck.Length);
            for (var i = 0; i < deck.Length; i++)
            {
                m.WriteInt32(deck[i] == null ? 0 : deck[i].resourceID);
            }
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Health);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.MaxHealth);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Physique);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Morale);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Concentration);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATESTATERANKSHIFT(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATESTATERANKSHIFT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.StateRank);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS);
            m.WriteInt32(p.FamePoints);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEPEPPOINTS(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEPEPPOINTS);
            m.WriteInt32(p.PepPoints);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte) ch.PawnState);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEBODYDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEBODYDELTA);
            m.WriteInt32(p.Body);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMINDDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMINDDELTA);
            m.WriteInt32(p.Mind);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEFOCUSDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEFOCUSDELTA);
            m.WriteInt32(p.Focus);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMAGICRESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMAGICRESISTANCE);
            m.WriteFloat(p.MagicResistance);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMELEERESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMELEERESISTANCE);
            m.WriteFloat(p.MeleeResistance);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATERANGEDRESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATERANGEDRESISTANCE);
            m.WriteFloat(p.RangedResistance);
            return m;
        }

        public static Message S2C_GAME_COMBATSTATS_SV2CL_UPDATEINCOMBAT(PlayerCharacter p, bool inCombat)
        {
            var m = new Message(GameHeader.S2C_GAME_COMBATSTATS_SV2CL_UPDATEINCOMBAT);
            m.WriteInt32(inCombat ? 1 : 0);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEBODYANDRUNEAFFINITY(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEBODYANDRUNEAFFINITY);
            m.WriteInt32(p.Body);
            m.WriteFloat(p.RuneAffinity);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEMINDANDSPIRITAFFINITY(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEMINDANDSPIRITAFFINITY);
            m.WriteInt32(p.Mind);
            m.WriteFloat(p.SpiritAffinity);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFOCUSANDSOULAFFINITY(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFOCUSANDSOULAFFINITY);
            m.WriteInt32(p.Focus);
            m.WriteFloat(p.SoulAffinity);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(ch.GetEffectiveMoveSpeed());
            return m;
        }

        public static Message S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM(Game_Item item, EItemChangeNotification notification)
        {
            //Fsv2cl_item
            var m = new Message(GameHeader.S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM);
            m.WriteInt32(item.DBID);
            m.WriteInt32(item.Type.resourceID);
            m.WriteInt32(item.CharacterID);
            m.WriteByte((byte) item.LocationType);
            m.WriteInt32(item.LocationSlot);
            m.WriteInt32(item.LocationID);
            m.WriteInt32(item.StackSize);
            m.WriteByte(item.Color1);
            m.WriteByte(item.Color2);
            m.WriteInt32(item.Attuned);
            m.WriteByte(0); //dummy
            m.WriteByte((byte) notification);
            return m;
        }

        public static Message S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM(EItemLocationType locationType, int locationSlot, int locationID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM);
            m.WriteByte((byte) locationType);
            m.WriteInt32(locationSlot);
            m.WriteInt32(locationID);
            return m;
        }

        #region CharacterSelect

        public static Message S2C_CREATE_CHARACTER_ACK(DBPlayerCharacter character)
        {
            var msg = new Message(GameHeader.S2C_CS_CREATE_CHARACTER_ACK);
            msg.WriteInt32((int) MessageStatusCode.NO_ERROR);
            msg.WriteInt32(character.DBID); //characterID
            //SD_CHARACTER_DATA
            msg.WriteByte((byte) character.PawnState); //not dead
            msg.WriteInt32(character.AccountID);
            msg.WriteString(character.Name);
            msg.WriteVector3(UnitConversion.ToUnreal(character.Position));
            msg.WriteInt32((int) GameConfiguration.Get.player.StartZone);
            msg.WriteInt32(character.Money);
            msg.WriteInt32(character.Appearance.AppearancePart1); //appearance1
            msg.WriteInt32(character.Appearance.AppearancePart2); //appearance2
            msg.WriteRotator(UnitConversion.ToUnreal(Quaternion.Euler(character.Rotation)));
            msg.WriteInt32(character.Faction); //factionID
            msg.WriteInt32(0); //lastUsedTimeStamp
            //SD_CHARACTER_SHEET_DATA
            msg.WriteInt32(character.ArcheType);
            msg.WriteFloat(character.FamePep[0]);
            msg.WriteFloat(character.FamePep[1]);
            msg.WriteFloat(character.HealthMaxHealth[0]);
            msg.WriteInt32(0);
            msg.WriteByte((byte) character.ExtraBodyMindFocusAttributePoints[0]);
            msg.WriteByte((byte) character.ExtraBodyMindFocusAttributePoints[1]);
            msg.WriteByte((byte) character.ExtraBodyMindFocusAttributePoints[2]);
            msg.WriteByte((byte) character.ExtraBodyMindFocusAttributePoints[3]); //unknown
            var equipment = character.GetEquipmentList();
            msg.WriteInt32(equipment.Count);
            for (var i = 0; i < equipment.Count; i++)
            {
                WriteSv2cl_Item(equipment[i], msg);
            }
            return msg;
        }

        public static Message S2C_CS_DELETE_CHARACTER_ACK(int charID, bool success)
        {
            var m = new Message(GameHeader.S2C_CS_DELETE_CHARACTER_ACK);
            if (success)
            {
                m.WriteInt32(0); //0 = success, 1 = YouDoNotOwnThisCharacter, 2=passwordIncorrect, 3=internalError
                m.WriteInt32(charID); //characterID
            }
            else
            {
                m.WriteInt32(3);
                m.WriteInt32(-1);
            }
            return m;
        }

        #endregion

        #region WorldLogin

        public static Message S2C_WORLD_PRE_LOGIN(MapIDs newMap)
        {
            var m = new Message(GameHeader.S2C_WORLD_PRE_LOGIN);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            m.WriteInt32((int) newMap);
            return m;
        }

        public static Message S2C_WORLD_LOGIN(PlayerInfo player)
        {
            var p = player.ActiveCharacter;
            var m = new Message(GameHeader.S2C_WORLD_LOGIN);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            m.WriteInt32(p.RelevanceID);
            m.WriteFloat(Time.time); //ServerTime
            m.WriteByte(0); //NetState
            //pawnStream
            m.WriteVector3(UnitConversion.ToUnreal(p.Velocity)); //velocity
            m.WriteVector3(UnitConversion.ToUnreal(p.Position)); //location
            m.WriteByte((byte) p.Physics); //physics
            m.WriteByte(p.MoveFrame); //moveFrame
            m.WriteByte((byte) EPawnStates.PS_ALIVE); //pawnState
            m.WriteInt32(0); //Bitfield Invulnerable
            m.WriteFloat(p.GroundSpeedModifier); //baseMoveSpeed
            m.WriteInt32(0); //DebugFilters
            m.WriteInt32(p.Invisible ? 1 : 0); //visibility
            //statsStream
            m.WriteFloat(p.FameLevel); //fame
            m.WriteFloat(p.PepRank); //pep
            m.WriteInt32(0); //MayChoseClassBitfield
            m.WriteByte(p.RemainingAttributePoints); //remainingAttributePoints
            m.WriteFloat(p.Health); //currentHealth
            m.WriteByte(0); //cameraMode
            m.WriteInt32(p.GetEffectiveMoveSpeed()); //moveSpeed

            //Stats
            m.WriteInt32(p.Body);
            m.WriteInt32(p.Mind);
            m.WriteInt32(p.Focus);
            m.WriteFloat(p.Physique);
            m.WriteFloat(p.Morale);
            m.WriteFloat(p.Concentration);
            m.WriteInt32(p.FameLevel);
            m.WriteInt32(p.PepRank);
            m.WriteFloat(p.RuneAffinity);
            m.WriteFloat(p.SpiritAffinity);
            m.WriteFloat(p.SoulAffinity);
            m.WriteFloat(p.MeleeResistance);
            m.WriteFloat(p.RangedResistance);
            m.WriteFloat(p.MagicResistance);
            m.WriteInt32(p.MaxHealth);
            m.WriteFloat(p.PhysiqueRegeneration);
            m.WriteFloat(p.PhysiqueDegeneration);
            m.WriteFloat(p.MoraleRegeneration);
            m.WriteFloat(p.MoraleDegeneration);
            m.WriteFloat(p.ConcentrationRegeneration);
            m.WriteFloat(p.ConcentrationDegeneration);
            m.WriteFloat(p.HealthRegeneration);
            m.WriteFloat(p.AttackSpeedBonus);
            m.WriteFloat(p.MovementSpeedBonus);
            m.WriteFloat(p.DamageBonus);
            m.WriteFloat(p.Health);
            //stats end

            m.WriteInt32(p.StateRank); //slider
            m.WriteInt32(0); //something with stats buildup in character screen
            m.WriteInt32(0); //same
            m.WriteInt32(0); //same
            //effectsStream
            m.WriteInt32(p.Effects.Count); //count
            for (var i = 0; i < p.Effects.Count; i++)
            {
                m.WriteInt32(p.Effects[i].resourceID);
            }
            //characterInfo
            m.WriteInt32(p.RelevanceID); //id
            //characterdata
            m.WriteByte((byte) p.PawnState); //pawnState
            m.WriteInt32(player.Account.UID); //accountID
            m.WriteString(p.Name); //name
            m.WriteVector3(UnitConversion.ToUnreal(p.Position)); //position
            m.WriteInt32((int) p.LastZoneID); //worldID
            m.WriteInt32(p.Money); //money
            m.WriteInt32(p.Appearance.AppearancePart1); //apearance 1 maybe LOd3, 2 ?
            m.WriteInt32(p.Appearance.AppearancePart2); //appearance 2
            m.WriteRotator(UnitConversion.ToUnreal(p.Rotation)); //rotation
            m.WriteInt32(p.Faction.ID); //faction/taxonomy
            m.WriteInt32(0); //lastUsedTimeStamp
            //characterSheetData
            m.WriteInt32((int) p.ArcheType); //Archetype
            m.WriteFloat(p.FamePoints); //famepoints
            m.WriteFloat(p.PepPoints); //pepPoints
            m.WriteFloat(p.Health); //health
            m.WriteInt32(0); //selectedSkilldeck (?)
            m.WriteByte(p.ExtraBodyPoints); //extraBodyPoints
            m.WriteByte(p.ExtraMindPoints); //extraMindPoints
            m.WriteByte(p.ExtraFocusPoints); //extraFocusPoints
            m.WriteByte(0);

            var playerItems = p.ItemManager.GetItems(EItemLocationType.ILT_Unknown);
            m.WriteInt32(playerItems.Count);
            for (var i = 0; i < playerItems.Count; i++)
            {
                WriteSv2cl_Item(playerItems[i], m);
            }
            //characterinfo end

            var unknown1 = 1;
            m.WriteInt32(unknown1);
            for (var i = 0; i < unknown1; i++)
            {
                m.WriteInt32(0); //something with DBSkillDecks
            }

            m.WriteInt32(p.Skills.Count); //learnedskills
            for (var i = 0; i < p.Skills.Count; i++)
            {
                m.WriteInt32(p.Skills[i].resourceID); //skillID
                m.WriteByte((byte) p.Skills[i].SigilSlots); //sigilSlots
            }

            var sds = p.ActiveSkillDeck.GetSkillDeckSkills();
            m.WriteInt32(sds.Count); //skilldeckSkills
            for (var i = 0; i < sds.Count; i++)
            {
                m.WriteInt32(0);
                m.WriteInt32(sds[i].skillID);
                m.WriteByte((byte) sds[i].totalDeckSlot);
            }

            /*
            var numFinishedQuests = 0;
            m.WriteInt32(numFinishedQuests);
            for (var i = 0; i < numFinishedQuests; i++)
            {
                m.WriteInt32(i); //finishedQuest ID
            }
            var numActiveQuests = 0;
            m.WriteInt32(numActiveQuests);
            for (var i = 0; i < numActiveQuests; i++)
            {
                m.WriteInt32(0);
                m.WriteInt32(0);
                m.WriteInt32(0); //QuestID
                m.WriteInt32(0); //objectiveState
            }
            */

            //Valshaaran : player quest data

            //Finished quests

            //Array size
            m.WriteInt32(p.QuestData.completedQuestIDs.Count);
            //Array content
            foreach(var questID in p.QuestData.completedQuestIDs)
            {
                m.WriteInt32(questID);
            }

            //Active quest targets

            //Array size - cumulative sum of no. of targets
            int totalTargets = 0;
            foreach (var quest in p.QuestData.curQuests)
            {
                totalTargets += quest.targetProgress.Count;
            }
            m.WriteInt32(totalTargets);

            //Array contents
                foreach (var pqp in p.QuestData.curQuests)
                {
                Quest_Type quest = GameData.Get.questDB.GetQuest(pqp.questID);

                for (int n = 0; n < quest.targets.Count; n++) {
                    
                    m.WriteInt32(0);                            //Unknown
                    m.WriteInt32(0);                            //Unknown?
                    m.WriteInt32(quest.targets[n].resource.ID); //Objective resource ID?
                    m.WriteInt32(pqp.targetProgress[n]);        //Trying target's progress value?     
                    
                }
            }

            var numPersistentVariables = 1;
            m.WriteInt32(numPersistentVariables);
            for (var i = 0; i < numPersistentVariables; i++)
            {
                m.WriteInt32(100); //contextID 100=MapExplored
                m.WriteInt32(120); // (100) sectionID (GED files)
                m.WriteInt32(1); //(100) 1=discovered
            }


            m.WriteInt32((int) player.Account.Level); //0=player,1=error, 2 = gamemaster, 2+ = increasing Gamemaster levels (more rights like console CS-Commands)
            return m;
        }

        public static Message S2C_CS_LOGIN(PlayerInfo p, List<DBPlayerCharacter> characters)
        {
            var m = new Message(GameHeader.S2C_CS_LOGIN);
            m.WriteInt32(characters.Count);
            for (var i = 0; i < characters.Count; i++)
            {
                var pc = characters[i];
                m.WriteInt32(pc.DBID);
                m.WriteByte((byte) pc.PawnState);
                m.WriteInt32(pc.AccountID);
                m.WriteString(pc.Name);
                m.WriteVector3(UnitConversion.ToUnreal(pc.Position));
                m.WriteInt32(pc.LastZoneID); //worldID
                m.WriteInt32(pc.Money);
                m.WriteInt32(pc.Appearance.AppearancePart1); //appearance1
                m.WriteInt32(pc.Appearance.AppearancePart2); //appearance2
                m.WriteRotator(Rotator.Zero);
                m.WriteInt32(pc.Faction);
                m.WriteInt32(0); //lastusedTimeStamp
                m.WriteInt32(pc.ArcheType);
                m.WriteFloat(pc.FamePep[0]); //fame
                m.WriteFloat(pc.FamePep[1]); //pep
                m.WriteFloat(pc.HealthMaxHealth[0]);
                m.WriteInt32(0); //selectedSkilldeckID
                m.WriteByte((byte) pc.ExtraBodyMindFocusAttributePoints[0]);
                m.WriteByte((byte) pc.ExtraBodyMindFocusAttributePoints[1]);
                m.WriteByte((byte) pc.ExtraBodyMindFocusAttributePoints[2]);
                m.WriteByte((byte) pc.ExtraBodyMindFocusAttributePoints[3]);
                var playerItems = pc.GetEquipmentList();
                m.WriteInt32(playerItems.Count);
                for (var j = 0; j < playerItems.Count; j++)
                {
                    //playerItems[j].WriteTo(m);
                    WriteSv2cl_Item(playerItems[j], m);
                }
            }
            m.WriteInt32(characters.Count); //famemapCount
            for (var f = 0; f < characters.Count; f++)
            {
                m.WriteInt32(characters[f].DBID);
                m.WriteInt32(characters[f].FamePep[0]); //famevalue
            }
            return m;
        }

        #endregion

        #region WorldLogout

        public static Message S2C_WORLD_LOGOUT_ACK()
        {
            return new Message(GameHeader.S2C_WORLD_LOGOUT_ACK);
        }

        public static Message S2C_USER_ON_LOGOUT(int accountID, int charID)
        {
            var m = new Message(GameHeader.S2C_USER_ON_LOGOUT);
            m.WriteInt32(accountID);
            if (charID != -1)
                m.WriteInt32(charID);
            return m;
        }

        #endregion

        #region PlayerTeams

        public static Message S2C_REMOVED_FROM_TEAM(int teamID, PlayerCharacter removed, eTeamRemoveMemberCode reason)
        {
            var m = new Message(GameHeader.S2C_REMOVED_FROM_TEAM);
            m.WriteInt32(teamID);
            m.WriteInt32(removed.RelevanceID);
            m.WriteInt32(0);
            m.WriteInt32((int) reason);
            return m;
        }

        public static Message S2C_TEAM_LEAVE_ACK(int teamID, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LEAVE_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32((int) eTeamRequestResult.TRR_ACCEPT);
            return m;
        }

        public static Message S2C_TEAM_KICK_ACK(int teamID, PlayerCharacter kicked, eTeamRequestResult result, PlayerCharacter kicker)
        {
            var m = new Message(GameHeader.S2C_TEAM_KICK_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32(kicked.RelevanceID);
            m.WriteInt32((int) result);
            return m;
        }

        public static Message S2C_TEAM_LEADER_ACK(int teamID, PlayerCharacter newLead, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LEADER_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32(newLead.RelevanceID);
            m.WriteInt32((int) result);
            return m;
        }

        public static Message S2C_TEAM_LOOTMODE_ACK(int teamID, ELootMode lootMode, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LOOTMODE_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32((int) lootMode);
            m.WriteInt32((int) result);
            return m;
        }

        public static Message S2C_TEAM_REMOVE_MEMBER(int teamID, PlayerCharacter leaver, eTeamRemoveMemberCode reason)
        {
            var m = new Message(GameHeader.S2C_TEAM_REMOVE_MEMBER);
            m.WriteInt32(teamID); //Team ID?
            m.WriteInt32(0); //unknown
            m.WriteInt32(leaver.RelevanceID); //leaver rid?
            m.WriteInt32((int) reason); //eTeamRemoveMemberCode
            return m;
        }

        public static Message S2C_TEAM_DISBAND_ACK()
        {
            var m = new Message(GameHeader.S2C_TEAM_DISBAND_ACK);
            m.WriteInt32(0); //unknown
            m.WriteInt32(0); //unknown
            m.WriteInt32(0); //unknown
            return m;
        }

        public static Message S2C_TEAM_ADD_MEMBER(int teamID, PlayerCharacter newMember, bool isLeader)
        {
            var m = new Message(GameHeader.S2C_TEAM_ADD_MEMBER);

            m.WriteInt32(teamID); //eTeamRequestResult?
            m.WriteInt32(0); //Probably teamID

            //memberData
            m.WriteByte((byte) (isLeader ? 1 : 0)); //+isLeader
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteInt32((int) newMember.LastZoneID); //+memberworld
            m.WriteInt32(0);
            m.WriteInt32(newMember.RelevanceID); //+memberRID
            m.WriteString(newMember.Name); //+membername
            //memberData end
            return m;
        }

        public static Message S2C_GET_TEAM_INFO_ACK(PlayerTeam team, eTeamRequestResult result, PlayerCharacter target)
        {
            var m = new Message(GameHeader.S2C_GET_TEAM_INFO_ACK);

            m.WriteInt32((int) result); //Unknown, 0 = party frame but with no stats

            if (team != null)
                m.WriteInt32(team.TeamID); //teamID
            else
                m.WriteInt32(0);

            m.WriteInt32(0); //Unknown

            if (result != eTeamRequestResult.TRR_GET_TEAM_INFO_FAILED)
            {
                //Debug.Log ("Compiling team info");
                //teamMembersVec
                var numOtherMembers = team.memberCount - 1;
                if (numOtherMembers < 0)
                {
                    numOtherMembers = 0;
                }
                m.WriteInt32(numOtherMembers); // Size of memberdata array, teamMembersVec

                foreach (var p in team.Members)
                {
                    if (p != target)
                    {
                        //memberData
                        m.WriteByte((byte) (p == team.Leader ? 1 : 0)); //+isLeader
                        m.WriteByte(0);
                        m.WriteByte(0);
                        m.WriteInt32((int) p.LastZoneID); //+memberworld
                        m.WriteInt32(0);
                        m.WriteInt32(p.RelevanceID); //+memberRID
                        m.WriteString(p.Name); //+membername
                        //memberData end
                    }
                }
                //teamMembersVec end
            }

            return m;
        }

        public static Message S2C_TEAM_CHARACTER_STATS_BASE(int teamID, PlayerCharacter statsOwner)
        {
            var m = new Message(GameHeader.S2C_TEAM_CHARACTER_STATS_BASE);
            m.WriteInt32(teamID); //Unknown?
            m.WriteInt32(statsOwner.RelevanceID);
            m.WriteInt32((int) statsOwner.LastZoneID);
            var pos = UnitConversion.ToUnreal(statsOwner.Position);
            m.WriteFloat(pos.x);
            m.WriteFloat(pos.z);
            m.WriteByte((byte) (statsOwner.Appearance.Gender == CharacterGender.Female ? 1 : 0));
            m.WriteInt32((int) (statsOwner.ArcheType + 1));
            m.WriteInt32(0); //discipline
            m.WriteFloat(statsOwner.MaxHealth);
            m.WriteInt32(statsOwner.PepRank);
            m.WriteInt32(statsOwner.FameLevel);
            return m;
        }

        public static Message S2C_TEAM_CHARACTER_STATS_UPDATE(int teamID, PlayerCharacter statsOwner)
        {
            var m = new Message(GameHeader.S2C_TEAM_CHARACTER_STATS_UPDATE);

            m.WriteInt32(teamID); //TeamID?

            //StatsUpdateData start

            m.WriteInt32(statsOwner.RelevanceID); //Outer.CharacterID
            m.WriteFloat(statsOwner.Health); //Stats.mHealth
            m.WriteFloat(statsOwner.Physique); //Stats.mPhysiqueLevel
            m.WriteFloat(statsOwner.Morale); //Stats.mMoraleLevel
            m.WriteFloat(statsOwner.Concentration); //Stats.mConcentrationLevel
            m.WriteInt32(statsOwner.StateRank); //Stats.mStateRankShift
            m.WriteInt32(0); //TODO:Skills.mLastDuffUpdateTime

            //TODO: TeamDuffList, array of ints?
            m.WriteInt32(0);

            //LODData3, array of bytes?
            m.WriteByteArray(statsOwner.GetPackedLOD(3));

            //StatsUpdateData end
            return m;
        }

        #endregion

        #region PlayerConversations

        public static Message S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE(NpcCharacter partner, ConversationTopic curTopic, ConversationNode curNode,
            List<ConversationTopic> topics)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE);

            //Conversation NPC resource ID?
            //Single topic parameter - current topic ID?
            //Conversation state resource ID?
            //Response flags
            var responseFlags = curTopic.getResponseFlags(curNode);


            Debug.Log("PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE : ");
            Debug.Log("partner typeref ID = " + partner.typeRef.resourceID);
            Debug.Log("curTopic ID = " + curTopic.resource.ID);

            if (curNode == null)
            {
                Debug.Log("No node, setting ID and responseflags to 0");
            }
            else
            {
                Debug.Log("curNode ID = " + curNode.resource.ID);
                Debug.Log("responseFlags = " + responseFlags);
            }

            m.WriteInt32(partner.typeRef.resourceID); //partner resource ID
            m.WriteInt32(curTopic.resource.ID); //current topic resource ID

            if (curNode == null)
            {
                m.WriteInt32(0); //node
                m.WriteInt32(0); //responseflags
            }
            else
            {
                m.WriteInt32(curNode.resource.ID); //current node-within-topic ID
                m.WriteInt32(responseFlags); //response flags
            }


            //List of topics
            m.WriteInt32(topics.Count); //Size of array
            foreach (var ct in topics)
            {
                //Conversation topic ID?
                m.WriteInt32(ct.resource.ID);
            }
            return m;
        }

        public static Message S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE(NpcCharacter partner)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE);
            m.WriteInt32(partner.typeRef.resourceID);
            return m;
        }

        #endregion

        #region QuestLog

        //Valshaaran : 

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_ACCEPTQUEST(int questID, List<int> progressArray)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_ADDQUEST);

            //probably quest resource ID
            m.WriteInt32(questID);

            //Write progress array
            //TODO : Experimental
            if (progressArray == null)
            {
                m.WriteInt32(1);    //Progress array size
                m.WriteInt32(0);    //aProgress value
            }
            else {
                m.WriteInt32(progressArray.Count);    //Progress array size

                foreach (var aProgress in progressArray)
                {
                    m.WriteInt32(aProgress);        //Progress array items
                }
            }


            return m;
        }

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_ADDQUEST(int questID, List<int> progressArray)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_ADDQUEST);

            //probably quest resource ID
            m.WriteInt32(questID);

            //Write progress array
            //TODO : Experimental
            if (progressArray == null)
            {
                m.WriteInt32(1);    //Progress array size
                m.WriteInt32(0);    //aProgress value
            }
            else {
                m.WriteInt32(progressArray.Count);    //Progress array size

                foreach (var aProgress in progressArray)
                {
                    m.WriteInt32(aProgress);        //Progress array items
                }
            }


            return m;
        }

       

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST(int questID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST);

            m.WriteInt32(questID); //Probable
                    

            return m;
        }

        #endregion
    }
}