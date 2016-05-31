using System;
using System.Collections.Generic;
using Common;
using Network;
using World;

namespace Gameplay.Entities.Players
{
    public class PlayerTeam
    {
        public enum ETeamStatsUpdateType
        {
            BASE = 0,
            UPDATE = 1
        }

        ELootMode curLootMode;

        PlayerCharacter leader;

        List<PlayerCharacter> members = new List<PlayerCharacter>();

        public TeamHandler teamHandler;

        int teamID;

        public PlayerTeam(int tID)
        {
            teamID = tID;
        }

        public int TeamID
        {
            get { return teamID; }
        }

        public List<PlayerCharacter> Members
        {
            get { return members; }
        }

        public int memberCount
        {
            get { return members.Count; }
        }

        internal void StartPartyTravel(int targetWorld, string portalName)
        {
            //TODO
            foreach (var teamMember in members)
            {
                teamMember.ReceiveChatMessage("", "Party travel NYI", EGameChatRanges.GCR_SYSTEM);
            }

            throw new NotImplementedException();

        }

        public PlayerCharacter Leader
        {
            get { return leader; }
            set { leader = value; }
        }

        public ELootMode CurLootMode
        {
            get { return curLootMode; }
            set { curLootMode = value; }
        }

        public PlayerCharacter GetMember(int rid)
        {
            for (var i = memberCount; i-- > 0;)
            {
                if (members[i].RelevanceID == rid)
                {
                    return members[i];
                }
            }
            return null;
        }

        public void AddMember(PlayerCharacter pc)
        {
            members.Add(pc);
            pc.Team = this;

            if (memberCount != 0)
            {
                for (var i = memberCount; i-- > 0;)
                {
                    if (members[i] != pc)
                    {
                        NotifyTeamAddMember(pc, members[i]);
                    }
                }


                //Send team info to new member
                GetTeamInfoAck(pc, eTeamRequestResult.TRR_NONE);
            }
            else
            {
                //If membercount == 0
                leader = pc;
            }
        }

        public void Leave(PlayerCharacter pc)
        {
            if (!members.Contains(pc))
            {
                return;
            }
            members.Remove(pc);

            //Pass lead
            if (leader == pc)
            {
                SetLeader(pc, members[0]);
            }
            pc.Team = null;
            NotifyRemovedFromTeam(pc, eTeamRemoveMemberCode.TRMC_LEAVE);

            for (var i = 0; i < memberCount; i++)
            {
                NotifyMemberRemoved(pc, eTeamRemoveMemberCode.TRMC_LEAVE, members[i]);
            }

            if (memberCount == 1)
            {
                Disband(leader);
            }
        }

        //TODO: Currently removes from group 
        //- proper implementation should mark team member as DCed
        public void Disconnect(PlayerCharacter pc)
        {
            if (!members.Contains(pc))
            {
                return;
            }
            members.Remove(pc);

            //Pass lead
            if (leader == pc)
            {
                SetLeader(pc, members[0]);
            }
            pc.Team = null;
            NotifyRemovedFromTeam(pc, eTeamRemoveMemberCode.TRMC_OFFLINE);

            for (var i = 0; i < memberCount; i++)
            {
                NotifyMemberRemoved(pc, eTeamRemoveMemberCode.TRMC_OFFLINE, members[i]);
            }

            if (memberCount == 1)
            {
                Disband(leader);
            }
        }

        public void Kick(PlayerCharacter kicker, int kickedID)
        {
            var kicked = GetMember(kickedID);
            if (kicker != leader)
            {
                KickAck(kicker, kicked, eTeamRequestResult.TRR_KICK_FAILED);
                return;
            }

            if (kicked == null)
            {
                KickAck(kicker, kicked, eTeamRequestResult.TRR_UNKNOWN_CHARACTER);
                return;
            }

            NotifyRemovedFromTeam(kicked, eTeamRemoveMemberCode.TRMC_KICK);
            members.Remove(kicked);
            kicked.Team = null;


            for (var i = 0; i < memberCount; i++)
            {
                NotifyMemberRemoved(kicked, eTeamRemoveMemberCode.TRMC_KICK, members[i]);
            }


            if (memberCount == 1)
            {
                //1 player isn't a party, so disband
                SetLeader(members[0], kicker);
                Disband(kicker);
            }
        }

        public void Disband(PlayerCharacter caller)
        {
            if (caller != leader)
            {
                return;
            }
            for (var i = 0; i < memberCount; i++)
            {
                TeamDisbandAck(members[i]);
                members[i].Team = null;
            }
            leader = null;
            teamHandler.RemoveTeam(this);
        }

        public void SetLeader(PlayerCharacter origin, PlayerCharacter newLead)
        {
            //handle: request origin isn't current leader
            if (origin != leader)
            {
                SetLeadAck(origin, newLead, eTeamRequestResult.TRR_CHANGE_LEADER_FAILED);
                return;
            }


            if (newLead == origin)
            {
                //skip rest of method if leader unchanged
                return;
            }


            leader = newLead;

            for (var i = memberCount; i-- > 0;)
            {
                SetLeadAck(members[i], newLead, eTeamRequestResult.TRR_NONE);
            }
        }

        void NotifyRemovedFromTeam(PlayerCharacter removed, eTeamRemoveMemberCode reason)
        {
            var m = PacketCreator.S2C_REMOVED_FROM_TEAM(teamID, removed, reason);
            removed.SendToClient(m);
        }

        void LeaveAck(PlayerCharacter target)
        {
            var m = PacketCreator.S2C_TEAM_LEAVE_ACK(teamID, eTeamRequestResult.TRR_ACCEPT);
            target.SendToClient(m);
        }

        void KickAck(PlayerCharacter kicker, PlayerCharacter kicked, eTeamRequestResult result)
        {
            var m = PacketCreator.S2C_TEAM_KICK_ACK(teamID, kicked, result, kicker);
            kicker.SendToClient(m);
        }

        void SetLeadAck(PlayerCharacter target, PlayerCharacter newLead, eTeamRequestResult result)
        {
            var m = PacketCreator.S2C_TEAM_LEADER_ACK(teamID, newLead, result);
            target.SendToClient(m);
        }

        public void SetLootMode(PlayerCharacter origin, ELootMode newLootMode)
        {
            //Handle origin isn't leader
            if (origin != leader)
            {
                LootModeAck(origin, eTeamRequestResult.TRR_CHANGE_LOOTMODE_FAILED, 0);
                return;
            }

            curLootMode = newLootMode;
            //Debug.Log ("setting loot mode for team" + teamID + " to " + newLootMode);
            for (var i = memberCount; i-- > 0;)
            {
                LootModeAck(members[i], eTeamRequestResult.TRR_NONE, newLootMode);
            }
        }

        public void LootModeAck(PlayerCharacter target, eTeamRequestResult result, ELootMode lootMode)
        {
            var m = PacketCreator.S2C_TEAM_LOOTMODE_ACK(teamID, lootMode, result);
            target.SendToClient(m);
        }


        void NotifyMemberRemoved(PlayerCharacter leaver, eTeamRemoveMemberCode reason, PlayerCharacter target)
        {
            var m = PacketCreator.S2C_TEAM_REMOVE_MEMBER(teamID, leaver, reason);
            target.SendToClient(m);
        }

        void TeamDisbandAck(PlayerCharacter target)
        {
            var m = PacketCreator.S2C_TEAM_DISBAND_ACK();
            target.SendToClient(m);
        }

        void NotifyTeamAddMember(PlayerCharacter newMember, PlayerCharacter target)
        {
            var newMemIsLeader = newMember == leader ? true : false;
            var m = PacketCreator.S2C_TEAM_ADD_MEMBER(teamID, newMember, newMemIsLeader);
            target.SendToClient(m);

            SendStatsBase(newMember, target);
            SendStatsUpdate(newMember, target);
        }

        public void GetTeamInfoAck(PlayerCharacter target, eTeamRequestResult result)
        {
            var m = PacketCreator.S2C_GET_TEAM_INFO_ACK(this, result, target);
            target.SendToClient(m);
            //Debug.Log ("Attempted to send team info packet");

            //TODO: currently a bit hackish - should really only update new member's partyinfo
            updateAllStats(ETeamStatsUpdateType.BASE);
            updateAllStats(ETeamStatsUpdateType.UPDATE);
        }

        //Update all members' stats for all other members, for the given updateType (base or update)
        public void updateAllStats(ETeamStatsUpdateType updateType)
        {
            foreach (var p in members)
            {
                foreach (var q in members)
                {
                    if (p != q)
                    {
                        switch (updateType)
                        {
                            case ETeamStatsUpdateType.BASE:
                                SendStatsBase(p, q);
                                break;

                            case ETeamStatsUpdateType.UPDATE:
                                SendStatsUpdate(p, q);
                                break;
                        }
                    }
                }
            }
        }

        void SendStatsBase(PlayerCharacter statsOwner, PlayerCharacter target)
        {
            var m = PacketCreator.S2C_TEAM_CHARACTER_STATS_BASE(teamID, statsOwner);
            target.SendToClient(m);
        }

        void SendStatsUpdate(PlayerCharacter statsOwner, PlayerCharacter target)
        {
            var m = PacketCreator.S2C_TEAM_CHARACTER_STATS_UPDATE(teamID, statsOwner);
            target.SendToClient(m);
        }

        public void teamMessage(PlayerCharacter origin, string message)
        {
            var m = PacketCreator.S2C_GAME_CHAT_SV2CL_ONMESSAGE(origin.Name, message, EGameChatRanges.GCR_TEAM);
            foreach (var target in members)
            {
                if (target != origin)
                {
                    target.SendToClient(m);
                }
            }
        }
    }
}