using System.Collections.Generic;
using Common;
using Common.UnrealTypes;
using Database.Dynamic.Internal;
using Gameplay.Conversations;
using Gameplay.Entities;
using Gameplay.Entities.Players;
using Gameplay.Items;
using Gameplay.Skills;
using UnityEngine;
using Utility;
using World;
using Gameplay.Quests;
using Database.Static;
using Gameplay;
using Gameplay.Entities.Interactives;
using Gameplay.Loot;

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

        public static Message S2C_PLAYER_ADD(PlayerCharacter pc)
        {
            var m = new Message(GameHeader.S2C_PLAYER_ADD);
            m.WriteInt32(pc.RelevanceID);
            m.WriteInt32(UnitConversion.ToUnreal(pc.Rotation).Yaw);
            m.WriteInt32(pc.Stats.mRecord.MaxHealth);
            m.WriteFloat(pc.Stats.mRecord.Physique);
            m.WriteFloat(pc.Stats.mRecord.Morale);
            m.WriteFloat(pc.Stats.mRecord.Concentration);
            m.WriteInt32(pc.Stats.GetFameLevel());
            m.WriteInt32(pc.Stats.GetPePRank());
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
            var app = pc.Appearance as Game_PlayerAppearance;
            m.WriteByteArray(app.GetPackedLOD(0));
            m.WriteByteArray(app.GetPackedLOD(1)); //<-----------fix length for all, or better implement it!
            m.WriteByteArray(app.GetPackedLOD(2));
            m.WriteByteArray(app.GetPackedLOD(3));
            m.WriteString(pc.Name);
            m.WriteString(pc.Guild != null ? pc.Guild.Name : "");
            m.WriteInt32(pc.Faction.ID);
            m.WriteFloat(pc.Stats.mRecord.CopyHealth);
            m.WriteByte(0); //frozenFlags
            m.WriteInt32(pc.GetEffectiveMoveSpeed());
            m.WriteInt32(pc.Stats.StateRank);
            m.WriteByte((byte) pc.CombatState.CombatMode); //HACK: following fields may have to be changed by this mode accordingly
            //var it = pc.Items.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
            //m.WriteInt32(it != null ? it.Type.resourceID : 0);
            //it = pc.Items.GetEquippedItem(EquipmentSlot.ES_SHIELD);
            //m.WriteInt32(it != null ? it.Type.resourceID : 0); //Offhandweapon! (does this even exist?)
            m.WriteInt32(pc.CombatState.MainWeapon);
            m.WriteInt32(pc.CombatState.OffhandWeapon);
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
            m.WriteInt32(npc.Type.resourceID);
            m.WriteInt32(-1); //ownerID?
            m.WriteVector3(UnitConversion.ToUnreal(npc.Position));
            m.WriteInt32(npc.Stats.mRecord.MaxHealth);
            m.WriteFloat(npc.Stats.mRecord.Physique);
            m.WriteFloat(npc.Stats.mRecord.Morale);
            m.WriteFloat(npc.Stats.mRecord.Concentration);
            m.WriteInt32(npc.Stats.GetFameLevel());
            m.WriteInt32(npc.Stats.GetPePRank());
            //NpcPawnStream
            m.WriteInt32(npc.Type.resourceID); //same as resourceID

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
            m.WriteInt32(-1); //target-RelID mNetFocus
            m.WriteVector3(UnitConversion.ToUnreal(npc.FocusLocation)); //mNetFocusLocation
            m.WriteRotator(UnitConversion.ToUnreal(npc.Rotation)); //mDefaultRotation
            m.WriteByte((byte) npc.PawnState);
            m.WriteInt32((int) npc.Stats.GetCharacterClass()); //unsure
            m.WriteInt32(0);
            m.WriteInt32(0); //DebugFilter?
            m.WriteInt32(npc.Invisible ? 1 : 0); //invisibility
            //NpcPawnStream end
            m.WriteInt32(npc.ShiftableAppearance); //shiftableAppearance
            m.WriteInt32(npc.Faction.ID); //Faction
            //NpcStatsStream
            m.WriteFloat(npc.Stats.mRecord.CopyHealth); //health
            m.WriteByte(npc.Stats.FrozenFlags); //frozenflags?
            m.WriteInt32(0); //movespeed?
            m.WriteInt32(npc.Stats.StateRank);
            //NpcStatsStream end
            //NpcCombatStateStream
            m.WriteByte((byte) npc.CombatState.CombatMode);
            m.WriteInt32(npc.CombatState.MainWeapon); //mainHandweapon?
            m.WriteInt32(npc.CombatState.OffhandWeapon); //offhandweapon?
            //NpcCombatStateStream end
            m.WriteInt32(npc.Effects.Count);
            for (var i = 0; i < npc.Effects.Count; i++)
            {
                m.WriteInt32(npc.Effects[i].resourceID);
            }
            return m;
        }       

        public static Message S2C_LEVELOBJECT_REMOVE(Entity ro)
        {
            var m = new Message(GameHeader.S2C_LEVELOBJECT_REMOVE);
            m.WriteInt32(ro.RelevanceID);
            m.WriteInt32(1);
            return m;
        }

        public static Message S2C_USER_LEVELUP()
        {
            //TODO
            Message m = new Message(GameHeader.S2C_USER_LEVELUP);

            m.WriteInt32(0);    //Unknown
            m.WriteInt32(0);    //Unknown
            m.WriteInt32(0);    //Unknown

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

        #region CharacterSelect

        public static Message S2C_CREATE_CHARACTER_ACK(DBPlayerCharacter character)
        {
            var msg = new Message(GameHeader.S2C_CS_CREATE_CHARACTER_ACK);
            msg.WriteInt32((int)MessageStatusCode.NO_ERROR);
            msg.WriteInt32(character.DBID); //characterID
            //SD_CHARACTER_DATA
            msg.WriteByte((byte)character.PawnState); //not dead
            msg.WriteInt32(character.AccountID);
            msg.WriteString(character.Name);
            msg.WriteVector3(UnitConversion.ToUnreal(character.Position));
            msg.WriteInt32((int)GameConfiguration.Get.player.StartZone);
            msg.WriteInt32(character.Money);
            msg.WriteInt32(character.Appearance.AppearanceCachePart1); //appearance1
            msg.WriteInt32(character.Appearance.AppearanceCachePart2); //appearance2
            msg.WriteRotator(UnitConversion.ToUnreal(Quaternion.Euler(character.Rotation)));
            msg.WriteInt32(character.Faction); //factionID
            msg.WriteInt32(0); //lastUsedTimeStamp
            //SD_CHARACTER_SHEET_DATA
            msg.WriteInt32(character.ArcheType);
            msg.WriteFloat(character.FamePep[0]);
            msg.WriteFloat(character.FamePep[1]);
            msg.WriteFloat(character.Health);
            msg.WriteInt32(0);
            msg.WriteByte((byte)character.ExtraBodyMindFocusAttributePoints[0]);
            msg.WriteByte((byte)character.ExtraBodyMindFocusAttributePoints[1]);
            msg.WriteByte((byte)character.ExtraBodyMindFocusAttributePoints[2]);
            msg.WriteByte((byte)character.ExtraBodyMindFocusAttributePoints[3]); //unknown
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

        #region Combat state
        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON(ECombatMode mode)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON);
            m.WriteByte((byte)mode);
            return m;
        }

        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON()
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON);
            return m;
        }

        public static Message S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(ECombatMode mode)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON);
            m.WriteByte((byte)mode);
            return m;
        }

        public static Message S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_COMBATSTATE_SV2REL_DRAWWEAPON);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte)ch.CombatState.CombatMode); //ch.CombatMode
            m.WriteInt32(ch.CombatState.MainWeapon); //mainhand 
            m.WriteInt32(ch.CombatState.OffhandWeapon); //and offhand
            return m;
        }

        public static Message S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_COMBATSTATE_SV2REL_SHEATHEWEAPON);
            m.WriteInt32(ch.RelevanceID);
            return m;
        }

        #endregion

        #region GUI 

        public static Message S2C_GAME_GUI_SV2CL_SHOWTUTORIAL(SBResource articleRes)
        {
            var m = new Message(GameHeader.S2C_GAME_GUI_SV2CL_SHOWTUTORIAL);
            m.WriteInt32(articleRes.ID);
            return m;
        }

        #endregion

        #region Actors

        public static Message S2R_GAME_ACTOR_SV2CLREL_SETENABLED(Entity ro, bool enabled)
        {
            var m = new Message(GameHeader.S2R_GAME_ACTOR_SV2CLREL_SETENABLED);
            m.WriteInt32(ro.RelevanceID);
            m.WriteInt32(enabled ? 1 : 0);
            return m;
        }

        public static Message S2R_GAME_ACTOR_SV2CLREL_SHOW(Entity ro, bool show, float fadeTimer = 0.0f)
        {
            var m = new Message(GameHeader.S2R_GAME_ACTOR_SV2CLREL_SHOW);
            m.WriteInt32(ro.RelevanceID);
            m.WriteInt32(show ? 1 : 0);
            m.WriteFloat(fadeTimer);
            return m;
        }

        public static Message S2R_GAME_ACTOR_SV2CLREL_SETCOLLISIONTYPE(Entity ro, ECollisionType col)
        {
            var m = new Message(GameHeader.S2R_GAME_ACTOR_SV2CLREL_SETCOLLISIONTYPE);
            m.WriteInt32(ro.RelevanceID);
            m.WriteByte((byte)col);
            return m;
        }

        public static Message S2C_GAME_ACTOR_MOVE(Entity actor, Vector3 newPos, Quaternion newRot)
        {
            Message m = new Message(GameHeader.S2C_GAME_ACTOR_MOVE);
            m.WriteInt32(actor.RelevanceID);
            m.WriteInt32(0); //unknown!
            m.WriteVector3(UnitConversion.ToUnreal(newPos));
            m.WriteRotator(UnitConversion.ToUnreal(newRot));
            return m;
        }

        public static Message S2C_LEVELOBJECT_REMOVE(InteractiveLevelElement ie)
        {
            var m = new Message(GameHeader.S2C_LEVELOBJECT_REMOVE);
            m.WriteInt32(ie.RelevanceID);
            m.WriteInt32(ie.LevelObjectID); //Experimental?
            return m;
        }

        #endregion

        #region Interactives

        public static Message S2C_INTERACTIVELEVELELEMENT_ADD(InteractiveLevelElement ie)
        {
            var m = new Message(GameHeader.S2C_INTERACTIVELEVELELEMENT_ADD);
            m.WriteInt32(ie.RelevanceID); //element relevanceID
            m.WriteInt32(ie.LevelObjectID); //leveObjectID
            m.WriteInt32(ie.Enabled ? 1 : 0); //isEnabled
            m.WriteInt32(ie.Invisible ? 1 : 0); //isHidden
            m.WriteRotator(UnitConversion.ToUnreal(ie.Rotation)); //NetRotation
            m.WriteVector3(UnitConversion.ToUnreal(ie.Position)); //NetLocation
            m.WriteByte((byte)ie.CollisionType); //collisionType
            m.WriteInt32(0);
            return m;
        }

        public static Message S2R_INTERACTIVELEVELELEMENT_SV2CLREL_STARTCLIENTSUBACTION(InteractiveLevelElement ie, int menuOptInd, int subActInd, bool reverse, Character c)
        {
            var m = new Message(GameHeader.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_STARTCLIENTSUBACTION);

            m.WriteInt32(ie.RelevanceID);   //element relevance ID?
            m.WriteInt32(menuOptInd);     //option index?
            m.WriteInt32(subActInd);        //suboption index?
            m.WriteInt32(reverse ? 1 : 0);          //bool aReverse?
            m.WriteInt32(c.RelevanceID);    //interacting pawn relevance ID?                              
            return m;
        }

        public static Message S2R_INTERACTIVELEVELELEMENT_SV2CLREL_CANCELCLIENTSUBACTION(InteractiveLevelElement ie, int menuOptInd, int subActInd)
        {
            var m = new Message(GameHeader.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_CANCELCLIENTSUBACTION);

            m.WriteInt32(ie.RelevanceID);
            m.WriteInt32(menuOptInd);
            m.WriteInt32(subActInd);
            return m;
        }

        public static Message S2R_INTERACTIVELEVELELEMENT_SV2CLREL_ENDCLIENTSUBACTION(InteractiveLevelElement ie, int menuOptInd, int subActInd, bool reverse)
        {
            var m = new Message(GameHeader.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_ENDCLIENTSUBACTION);

            m.WriteInt32(ie.RelevanceID);   //element relevance ID?
            m.WriteInt32(menuOptInd);     //option index?
            m.WriteInt32(subActInd);      //suboption index?
            m.WriteInt32(reverse ? 1 : 0);  //bool aReverse?                           
            return m;
        }

        public static Message S2R_INTERACTIVELEVELELEMENT_SV2CLREL_UPDATENETISACTIVATED(InteractiveLevelElement ie, bool activated)
        {
            Message m = new Message(GameHeader.S2R_INTERACTIVELEVELELEMENT_SV2CLREL_UPDATENETISACTIVATED);
            m.WriteInt32(ie.RelevanceID);
            int activatedInt = activated ? 1 : 0;
            m.WriteInt32(activatedInt);
            return m;
        }

        #endregion

        #region Items

        public static Message S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM(Game_Item item, EItemChangeNotification notification)
        {
            //Fsv2cl_item
            var m = new Message(GameHeader.S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM);
            m.WriteInt32(item.DBID);
            m.WriteInt32(item.Type.resourceID);
            m.WriteInt32(item.CharacterID);
            m.WriteByte((byte)item.LocationType);
            m.WriteInt32(item.LocationSlot);
            m.WriteInt32(item.LocationID);
            m.WriteInt32(item.StackSize);
            m.WriteByte(item.Color1);
            m.WriteByte(item.Color2);
            m.WriteInt32(item.Attuned);
            m.WriteByte(0); //dummy
            m.WriteByte((byte)notification);
            return m;
        }

        public static Message S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM(EItemLocationType locationType, int locationSlot, int locationID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM);
            m.WriteByte((byte)locationType);
            m.WriteInt32(locationSlot);
            m.WriteInt32(locationID);
            return m;
        }

        public static Message S2C_GAME_PLAYERCHARACTER_SV2CL_UPDATEMONEY(int amount)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCHARACTER_SV2CL_UPDATEMONEY);

            m.WriteInt32(amount);

            return m;
        }

        #endregion

        #region Looting

        //Declarations found in Game_Looting.uc in SDK, but with different names

        /*
        public static Message S2C_GAME_LOOTING_SV2CL_CHANGELOOTMODE
        */

        public static Message S2C_GAME_LOOTING_SV2CL_LOOTITEMREJECTED(int transID, int lootItemID, ELootRejectedReason reason)
        {
            Message m = new Message(GameHeader.S2C_GAME_LOOTING_SV2CL_LOOTITEMREJECTED);

            m.WriteInt32(transID);
            m.WriteInt32(lootItemID);
            m.WriteInt32((int)reason);

            return m;
        }


        public static Message S2C_GAME_LOOTING_SV2CL_REMOVEITEM(int transID, int lootItemID)
        {
            Message m = new Message(GameHeader.S2C_GAME_LOOTING_SV2CL_REMOVEITEM);

            m.WriteInt32(transID);
            m.WriteInt32(lootItemID);
            return m;
        }
        
        public static Message S2C_GAME_LOOTING_SV2CL_ENDTRANSACTION(int transID)
        {
            Message m = new Message(GameHeader.S2C_GAME_LOOTING_SV2CL_ENDTRANSACTION);
            m.WriteInt32(transID);
            return m;            
        }
        

        public static Message S2C_GAME_LOOTING_SV2CL_INITLOOTOFFER(int transID, float timerValue, ELootMode lootMode, List<ReplicatedLootItem> lootItems, List<PlayerCharacter> eligibleMembers)
        {

            Message m = new Message(GameHeader.S2C_GAME_LOOTING_SV2CL_INITLOOTOFFER);

            m.WriteInt32(transID);          //Transaction ID
            m.WriteFloat(timerValue);       //TimerValue(float)
            m.WriteByte((byte)lootMode);  //Loot mode
            

            //aLootItems
            m.WriteInt32(lootItems.Count);
            foreach(var li in lootItems)
            {
                //TODO - Experimental
                
                m.WriteInt32(li.ResourceId); //Item Type?
                m.WriteInt32(li.LootItemID);
                m.WriteInt32(li.Quantity);
            }

            //aEligibleMembers
            m.WriteInt32(eligibleMembers.Count);
            foreach(var member in eligibleMembers)
            {
                m.WriteInt32(member.RelevanceID);
            }

            return m;
        }

        

        #endregion

        #region Pawns
        
        public static Message S2R_BASE_PAWN_SV2CL_GOTOSTATE(Character c, string stateString)
        {
            var m = new Message(GameHeader.S2R_BASE_PAWN_SV2CL_GOTOSTATE);
            m.WriteInt32(c.RelevanceID);
            m.WriteString(stateString);
            return m;
        }

        public static Message S2C_GAME_PLAYERPAWN_SV2CL_FORCEMOVEMENT(Vector3 position, Vector3 velocity, EPhysics physics)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERPAWN_SV2CL_FORCEMOVEMENT);
            m.WriteVector3(UnitConversion.ToUnreal(position));
            m.WriteVector3(UnitConversion.ToUnreal(velocity));
            m.WriteByte((byte)physics);
            return m;
        }

        public static Message S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN(bool onChair)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN);
            int onChairInt = onChair ? 1 : 0;
            m.WriteInt32(onChairInt);
            return m;
             
        }

        public static Message S2R_GAME_NPCPAWN_SV2REL_FOCUSLOCATION(NpcCharacter character)
        {
            var m = new Message(GameHeader.S2R_GAME_NPCPAWN_SV2REL_FOCUSLOCATION);
            m.WriteInt32(character.RelevanceID);
            m.WriteVector3(UnitConversion.ToUnreal(character.FocusLocation));
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
            m.WriteByte((byte)pc.Physics);
            return m;
        }

        public static Message S2R_GAME_NPCPAWN_SV2REL_MOVE(NpcCharacter npc)
        {
            var m = new Message(GameHeader.S2R_GAME_NPCPAWN_SV2REL_MOVE);
            m.WriteInt32(npc.RelevanceID);
            m.WriteByte((byte)npc.MovementFlags);
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

        public static Message S2C_BASE_PAWN_REMOVE(Entity ro)
        {
            var m = new Message(GameHeader.S2C_BASE_PAWN_REMOVE);
            m.WriteInt32(ro.RelevanceID);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT(Character ch, EPawnEffectType effectType)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte)effectType);
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
            m.WriteByte((byte)soundEffect);
            m.WriteFloat(1f); //volume?
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
            m.WriteInt32(killer != null?killer.RelevanceID:0);
            m.WriteInt32(killer != null?killer.RelevanceID:0);
            m.WriteInt32(dead!=null?dead.RelevanceID:0);
            return m;
        }

        public static Message S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH(Character killer)
        {
            var m = new Message(GameHeader.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH);

            m.WriteInt32(killer ? killer.RelevanceID : 0); //Valshaaran - experimental for kill below Y coord

            return m;

        }

        public static Message S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN);
            m.WriteInt32(p.IsResting ? 0 : 0xff);
            return m;
        }

        public static Message S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteByte((byte)ch.PawnState);
            return m;
        }

        public static Message S2C_GAME_PLAYERPAWN_SV2CL_CLIENTSIDETRIGGER(string eventTag, PlayerCharacter instigator)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERPAWN_SV2CL_CLIENTSIDETRIGGER);
            m.WriteString(eventTag);
            m.WriteInt32(instigator.RelevanceID);
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
            m.WriteInt32((int)reason);
            return m;
        }

        public static Message S2C_TEAM_LEAVE_ACK(int teamID, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LEAVE_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32((int)eTeamRequestResult.TRR_ACCEPT);
            return m;
        }

        public static Message S2C_TEAM_KICK_ACK(int teamID, PlayerCharacter kicked, eTeamRequestResult result, PlayerCharacter kicker)
        {
            var m = new Message(GameHeader.S2C_TEAM_KICK_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32(kicked.RelevanceID);
            m.WriteInt32((int)result);
            return m;
        }

        public static Message S2C_TEAM_LEADER_ACK(int teamID, PlayerCharacter newLead, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LEADER_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32(newLead.RelevanceID);
            m.WriteInt32((int)result);
            return m;
        }

        public static Message S2C_TEAM_LOOTMODE_ACK(int teamID, ELootMode lootMode, eTeamRequestResult result)
        {
            var m = new Message(GameHeader.S2C_TEAM_LOOTMODE_ACK);
            m.WriteInt32(teamID);
            m.WriteInt32((int)lootMode);
            m.WriteInt32((int)result);
            return m;
        }

        public static Message S2C_TEAM_REMOVE_MEMBER(int teamID, PlayerCharacter leaver, eTeamRemoveMemberCode reason)
        {
            var m = new Message(GameHeader.S2C_TEAM_REMOVE_MEMBER);
            m.WriteInt32(teamID); //Team ID?
            m.WriteInt32(0); //unknown
            m.WriteInt32(leaver.RelevanceID); //leaver rid?
            m.WriteInt32((int)reason); //eTeamRemoveMemberCode
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
            m.WriteByte((byte)(isLeader ? 1 : 0)); //+isLeader
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteInt32((int)newMember.LastZoneID); //+memberworld
            m.WriteInt32(0);
            m.WriteInt32(newMember.RelevanceID); //+memberRID
            m.WriteString(newMember.Name); //+membername
            //memberData end
            return m;
        }

        public static Message S2C_GET_TEAM_INFO_ACK(PlayerTeam team, eTeamRequestResult result, PlayerCharacter target)
        {
            var m = new Message(GameHeader.S2C_GET_TEAM_INFO_ACK);

            m.WriteInt32((int)result); //Unknown, 0 = party frame but with no stats

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
                        m.WriteByte((byte)(p == team.Leader ? 1 : 0)); //+isLeader
                        m.WriteByte(0);
                        m.WriteByte(0);
                        m.WriteInt32((int)p.LastZoneID); //+memberworld
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
            m.WriteInt32((int)statsOwner.LastZoneID);
            var pos = UnitConversion.ToUnreal(statsOwner.Position);
            m.WriteFloat(pos.x);
            m.WriteFloat(pos.z);
            m.WriteByte((byte)statsOwner.Appearance.GetGender());
            m.WriteInt32((int)(statsOwner.ArcheType + 1));
            m.WriteInt32(0); //discipline
            m.WriteFloat(statsOwner.Stats.mRecord.MaxHealth);
            m.WriteInt32(statsOwner.Stats.GetPePRank());
            m.WriteInt32(statsOwner.Stats.GetFameLevel());
            return m;
        }

        public static Message S2C_TEAM_CHARACTER_STATS_UPDATE(int teamID, PlayerCharacter statsOwner)
        {
            var m = new Message(GameHeader.S2C_TEAM_CHARACTER_STATS_UPDATE);

            m.WriteInt32(teamID); //TeamID?

            //StatsUpdateData start

            m.WriteInt32(statsOwner.RelevanceID); //Outer.CharacterID
            m.WriteFloat(statsOwner.Stats.mRecord.CopyHealth); //Stats.mHealth
            m.WriteFloat(statsOwner.Stats.mRecord.Physique); //Stats.mPhysiqueLevel
            m.WriteFloat(statsOwner.Stats.mRecord.Morale); //Stats.mMoraleLevel
            m.WriteFloat(statsOwner.Stats.mRecord.Concentration); //Stats.mConcentrationLevel
            m.WriteInt32(statsOwner.Stats.StateRank); //Stats.mStateRankShift
            m.WriteInt32(0); //TODO:Skills.mLastDuffUpdateTime

            //TODO: TeamDuffList, array of ints?
            m.WriteInt32(0);

            //LODData3, array of bytes?
            m.WriteByteArray((statsOwner.Appearance as Game_PlayerAppearance).GetPackedLOD(3));

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
            Debug.Log("partner rel ID = " + partner.RelevanceID);
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

            m.WriteInt32(partner.RelevanceID); //partner resource ID //Polymo: this is actually the RelevanceID
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
            //m.WriteInt32(0);
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
            m.WriteInt32(partner.RelevanceID);
            return m;
        }

        #endregion

        #region Player & game controller

        public static Message S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME(float serverTime)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME);
            m.WriteFloat(serverTime);
            return m;
        }

        public static Message S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE(PersistentVar pVar)
        {
            var m = new Message(GameHeader.S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE);
            m.WriteInt32(pVar.ContextID);
            m.WriteInt32(pVar.VarID);
            m.WriteInt32(pVar.Value);
            return m;
        }

        public static Message S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE(int contextID, int varID, int value)
        {
            var m = new Message(GameHeader.S2C_GAME_CONTROLLER_SV2CL_SETPERSISTENTVARIABLE);
            m.WriteInt32(contextID);
            m.WriteInt32(varID);
            m.WriteInt32(value);
            return m;
        }

        #endregion

        #region QuestLog

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

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_SETTARGETPROGRESS(int questID, int targetIndex, int progress)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_SETTARGETPROGRESS);

            m.WriteInt32(questID);
            m.WriteInt32(targetIndex);
            m.WriteInt32(progress);

            return m;
        }

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST(int questID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST);

            m.WriteInt32(questID);

            return m;
        }

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_FINISHQUEST(int questID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_FINISHQUEST);

            m.WriteInt32(questID);

            return m;
        }

        public static Message S2C_GAME_PLAYERQUESTLOG_SV2CL_COMPLETEQUEST(int questID)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERQUESTLOG_SV2CL_COMPLETEQUEST);

            m.WriteInt32(questID);

            return m;
        }



        #endregion

        #region Skills

        public static Message S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(PlayerCharacter pc, Game_Skills.RunningSkillData s, int tokenItemID)
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL);
            m.WriteInt32(s.Skill.resourceID); //skillID
            m.WriteFloat(s.StartTime); //startTime
            m.WriteFloat(s.Duration); //(animation duration has to be manually collected)
            m.WriteFloat(s.SkillSpeed); //skillSpeed
            m.WriteInt32(s.LockedMovement ? 1 : 0); //freezeMovement
            m.WriteInt32(s.LockedRotation ? 1 : 0); //freezeRotation
            m.WriteInt32(tokenItemID); //aTokenItemID
            m.WriteInt32(s.Skill.animationVariation); //animVarNr
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL(Character ch, Game_Skills.RunningSkillData s, Vector3 skillLocation, int tokenItemID)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2REL_ADDACTIVESKILL);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(s.Skill.resourceID);
            m.WriteFloat(s.StartTime);
            m.WriteFloat(s.Duration); // duration of skill
            m.WriteFloat(s.SkillSpeed); //skillSpeed, AnimationSpeed?
            m.WriteInt32(s.LockedMovement ? 1 : 0); //freezemovement
            m.WriteInt32(s.LockedRotation ? 1 : 0); //freezerotation
            m.WriteInt32(tokenItemID); //tokenItemID
            m.WriteInt32(s.Skill.animationVariation); //animvarNr
            m.WriteVector3(UnitConversion.ToUnreal(skillLocation));
            m.WriteRotator(UnitConversion.ToUnreal(ch.Rotation)); //TODO viewRotation?
            return m;
        }

        public static Message S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL()
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL);
            return m;
        }

        public static Message S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Character ch, int skillID, int eventID, int flags, Character skillPawn, Character triggerPawn,
           Character targetPawn, float time)
        {
            var m = new Message(GameHeader.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(skillID);
            m.WriteInt32(eventID);
            m.WriteInt32(flags); // constants in SkillEvent
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
            m.WriteInt32(flags); //constants in SkillEvent
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

        public static Message S2C_GAME_SKILLS_SV2CL_LEARNSKILL(FSkill_Type s)
        {
            var m = new Message(GameHeader.S2C_GAME_SKILLS_SV2CL_LEARNSKILL);
            m.WriteInt32(s.resourceID);
            return m;
        }

        public static Message S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(PlayerCharacter p, List<FSkill_Type> skills, FSkill_Type[] deck)
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

        #endregion

        #region Stats

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.mRecord.CopyHealth);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.mRecord.MaxHealth);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.mRecord.Physique);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.mRecord.Morale);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.mRecord.Concentration);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATESTATERANKSHIFT(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATESTATERANKSHIFT);
            m.WriteInt32(ch.RelevanceID);
            m.WriteFloat(ch.Stats.StateRank);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEFROZENFLAGS(Character c, byte frozenFlags)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEFROZENFLAGS);
            m.WriteInt32(c.RelevanceID);
            m.WriteByte(frozenFlags);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFAMEPOINTS);
            m.WriteFloat((p.Stats as Game_PlayerStats).FamePoints);
            return m;
        }

        public static Message S2R_GAME_PLAYERSTATS_SV2CLREL_ONLEVELUP(Character p)
        {
            var m = new Message(GameHeader.S2R_GAME_PLAYERSTATS_SV2CLREL_ONLEVELUP);
            m.WriteInt32(p.RelevanceID);
            m.WriteInt32(p.Stats.GetFameLevel());
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEPEPPOINTS(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEPEPPOINTS);
            m.WriteInt32((p.Stats as Game_PlayerStats).PepPoints);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEBODYDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEBODYDELTA);
            m.WriteInt32(p.Stats.mRecord.Body);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMINDDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMINDDELTA);
            m.WriteInt32(p.Stats.mRecord.Mind);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEFOCUSDELTA(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEFOCUSDELTA);
            m.WriteInt32(p.Stats.mRecord.Focus);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMAGICRESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMAGICRESISTANCE);
            m.WriteFloat(p.Stats.mRecord.MagicResistance);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMELEERESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATEMELEERESISTANCE);
            m.WriteFloat(p.Stats.mRecord.MeleeResistance);
            return m;
        }

        public static Message S2C_GAME_CHARACTERSTATS_SV2CL_UPDATERANGEDRESISTANCE(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_CHARACTERSTATS_SV2CL_UPDATERANGEDRESISTANCE);
            m.WriteFloat(p.Stats.mRecord.RangedResistance);
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
            m.WriteInt32(p.Stats.mRecord.Body);
            m.WriteFloat(p.Stats.mRecord.RuneAffinity);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEMINDANDSPIRITAFFINITY(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEMINDANDSPIRITAFFINITY);
            m.WriteInt32(p.Stats.mRecord.Mind);
            m.WriteFloat(p.Stats.mRecord.SpiritAffinity);
            return m;
        }

        public static Message S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFOCUSANDSOULAFFINITY(PlayerCharacter p)
        {
            var m = new Message(GameHeader.S2C_GAME_PLAYERSTATS_SV2CL_UPDATEFOCUSANDSOULAFFINITY);
            m.WriteInt32(p.Stats.mRecord.Focus);
            m.WriteFloat(p.Stats.mRecord.SoulAffinity);
            return m;
        }

        public static Message S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(Character ch)
        {
            var m = new Message(GameHeader.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED);
            m.WriteInt32(ch.RelevanceID);
            m.WriteInt32(ch.GetEffectiveMoveSpeed());
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
            var pStats = p.Stats as Game_PlayerStats;
            m.WriteFloat(pStats.FamePoints); //fame points (not fame level!)
            m.WriteFloat(pStats.PepPoints); //pep
            m.WriteInt32(pStats.MayChoseClass ? 1 : 0); //MayChoseClass
            m.WriteByte(pStats.RemainingAttributePoints); //remainingAttributePoints
            m.WriteFloat(p.Stats.mRecord.CopyHealth); //currentHealth
            m.WriteByte(0); //cameraMode
            m.WriteInt32(p.GetEffectiveMoveSpeed()); //moveSpeed

            //Stats
            m.WriteInt32(p.Stats.mRecord.Body);
            m.WriteInt32(p.Stats.mRecord.Mind);
            m.WriteInt32(p.Stats.mRecord.Focus);
            m.WriteFloat(p.Stats.mRecord.Physique);
            m.WriteFloat(p.Stats.mRecord.Morale);
            m.WriteFloat(p.Stats.mRecord.Concentration);
            m.WriteInt32(p.Stats.mRecord.FameLevel);
            m.WriteInt32(p.Stats.mRecord.PePRank);
            m.WriteFloat(p.Stats.mRecord.RuneAffinity);
            m.WriteFloat(p.Stats.mRecord.SpiritAffinity);
            m.WriteFloat(p.Stats.mRecord.SoulAffinity);
            m.WriteFloat(p.Stats.mRecord.MeleeResistance);
            m.WriteFloat(p.Stats.mRecord.RangedResistance);
            m.WriteFloat(p.Stats.mRecord.MagicResistance);
            m.WriteInt32(p.Stats.mRecord.MaxHealth);
            m.WriteFloat(p.Stats.mRecord.PhysiqueRegeneration);
            m.WriteFloat(p.Stats.mRecord.PhysiqueDegeneration);
            m.WriteFloat(p.Stats.mRecord.MoraleRegeneration);
            m.WriteFloat(p.Stats.mRecord.MoraleDegeneration);
            m.WriteFloat(p.Stats.mRecord.ConcentrationRegeneration);
            m.WriteFloat(p.Stats.mRecord.ConcentrationDegeneration);
            m.WriteFloat(p.Stats.mRecord.HealthRegeneration);
            m.WriteFloat(p.Stats.mRecord.AttackSpeedBonus);
            m.WriteFloat(p.Stats.mRecord.MovementSpeedBonus);
            m.WriteFloat(p.Stats.mRecord.DamageBonus);
            m.WriteFloat(p.Stats.mRecord.CopyHealth);
            //stats end

            m.WriteInt32(p.Stats.StateRank); //slider
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
            var app = p.Appearance as Game_PlayerAppearance;
            m.WriteInt32(app.AppearancePart1); //apearance 1 maybe LOd3, 2 ?
            m.WriteInt32(app.AppearancePart2); //appearance 2
            m.WriteRotator(UnitConversion.ToUnreal(p.Rotation)); //rotation
            m.WriteInt32(p.Faction.ID); //faction/taxonomy
            m.WriteInt32(0); //lastUsedTimeStamp
            //characterSheetData
            m.WriteInt32((int) p.ArcheType); //Archetype
            m.WriteFloat(pStats.FamePoints); //famepoints
            m.WriteFloat(pStats.PepPoints); //pepPoints
            m.WriteFloat(pStats.mRecord.CopyHealth); //health
            m.WriteInt32(0); //selectedSkilldeck (?)
            m.WriteByte(0); //extraBodyPoints
            m.WriteByte(0); //extraMindPoints
            m.WriteByte(0); //extraFocusPoints
            m.WriteByte(0);

            var playerItems = p.Items.GetItems(EItemLocationType.ILT_Unknown);
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

            m.WriteInt32(p.Skills.CharacterSkills.Count); //learnedskills
            for (var i = 0; i < p.Skills.CharacterSkills.Count; i++)
            {
                m.WriteInt32(p.Skills.CharacterSkills[i].resourceID); //skillID
                m.WriteByte((byte) p.Skills.GetTokenSlots(p.Skills.CharacterSkills[i])); //sigilSlots
            }

            var sdeck = (p.Skills as Game_PlayerSkills).GetSkillDeckSkills();
            m.WriteInt32(sdeck.Count); //skilldeckSkills count
            for (var i = 0; i < sdeck.Count; i++)
            {
                m.WriteInt32(0);
                m.WriteInt32(sdeck[i].Type.resourceID);
                m.WriteByte((byte) sdeck[i].AbsoluteDeckSlot);
            }

            #region Player quest data
            //Finished quests
            //Array size
            m.WriteInt32(p.questData.completedQuestIDs.Count);
            //Array content
            foreach(var questID in p.questData.completedQuestIDs)
            {
                m.WriteInt32(questID);
            }

            //Active quest targets
            //Array size - cumulative sum of no. of targets
            int totalTargets = 0;
            foreach (var quest in p.questData.curQuests)
            {
                totalTargets += quest.targetProgress.Count;
            }
            m.WriteInt32(totalTargets);

            //Array contents
            foreach (var pqp in p.questData.curQuests)
            {
                Quest_Type quest = GameData.Get.questDB.GetQuest(pqp.questID);

                for (int n = 0; n < quest.targets.Count; n++) {
                    
                    m.WriteInt32(0);                            //Unknown
                    m.WriteInt32(0);                            //Unknown?
                    m.WriteInt32(quest.targets[n].resource.ID); //Objective resource ID?
                    m.WriteInt32(pqp.targetProgress[n]);        //Trying target's progress value?     
                    
                }
            }
            #endregion

            #region Player persistent variables

            //PerVars array size
            m.WriteInt32(p.persistentVars.varsList.Count);

            //Array contents
            foreach (var pVar in p.persistentVars.varsList)
            {
                m.WriteInt32(pVar.ContextID); //contextID 100=MapExplored
                m.WriteInt32(pVar.VarID); // (100) sectionID (GED files)
                m.WriteInt32(pVar.Value); //(100) 1=discovered
            }

            #endregion

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
                m.WriteInt32(pc.Appearance.AppearanceCachePart1); //appearance1
                m.WriteInt32(pc.Appearance.AppearanceCachePart2); //appearance2
                m.WriteRotator(Rotator.Zero);
                m.WriteInt32(pc.Faction);
                m.WriteInt32(0); //lastusedTimeStamp
                m.WriteInt32(pc.ArcheType);
                m.WriteFloat(pc.FamePep[0]); //fame
                m.WriteFloat(pc.FamePep[1]); //pep
                m.WriteFloat(pc.Health);
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
                m.WriteInt32(characters[f].FameLevelCache); //famevalue
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

    }
}