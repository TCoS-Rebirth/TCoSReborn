using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Entities.Players;
using Network;
using UnityEngine;

namespace World
{
    //
    //TODO handle reconnects and map changes, re-send existing Teaminfos
    //

    public class TeamHandler : MonoBehaviour
    {
        //Team stat update manager
        //Base stats each 10 ticks
        //Update stats each tick
        //These are the constants defined in the game files - assuming  1.0f refers to per second? (rather than per tick)
        //TODO: Possibly move to Unity game settings
        public const int TEAM_MEMBER_INFO_BASE_RATE = 10;
        public const float TEAM_MEMBER_INFO_UPDATE_TIME = 1.0f;

        HashSet<int> allocatedIDs = new HashSet<int>();
        List<PlayerTeam> teams = new List<PlayerTeam>();

        int ticksDelta;

        int ticksPerUpdate;
        int updatesPerBaseDelta;

        int AllocateID()
        {
            var newID = 50; //TODO: Changing this to 1 BREAKS add party member!
            while (allocatedIDs.Contains(newID))
            {
                newID += 1;
            }
            return newID;
        }

        void Start()
        {
            //Init team handler(calculate the number of ticks to wait between stat updates
            ticksPerUpdate = (int) (TEAM_MEMBER_INFO_UPDATE_TIME/Time.fixedDeltaTime);
        }


        void FixedUpdate()
        {
            if (teams.Count == 0)
            {
                return;
            } //Skip update tick if no teams

            if (ticksDelta < ticksPerUpdate)
            {
                //Debug.Log("ticksDelta++");
                ticksDelta++;
            }
            else
            {
                ticksDelta = 0;
                //Debug.Log("sending team stat updates");
                foreach (var t in teams)
                {
                    t.updateAllStats(PlayerTeam.ETeamStatsUpdateType.UPDATE);
                }


                if (updatesPerBaseDelta < TEAM_MEMBER_INFO_BASE_RATE - 1)
                {
                    //Debug.Log("updatesPerBaseDelta++");
                    updatesPerBaseDelta++;
                }
                else
                {
                    updatesPerBaseDelta = 0;
                    //Debug.Log("sending team base stat updates");
                    foreach (var t in teams)
                    {
                        t.updateAllStats(PlayerTeam.ETeamStatsUpdateType.BASE);
                    }
                }
            }
        }

        public void RemoveTeam(PlayerTeam team)
        {
            Debug.Log("TeamHandler : Destroying team with ID " + team.TeamID);
            allocatedIDs.Remove(team.TeamID);
            teams.Remove(team);
        }

        //TODO: Properly handle use case where an inviter invites multiple invitees
        //before the first invitee replies (currently later invites 'overwrite' earlier ones)

        public void HandleInvite(PlayerCharacter inviter, int tid, string targetName)
        {
            var target = GameWorld.Instance.FindPlayerCharacter(targetName);

            if (target != null)
            {
                //Handle valid target
                if (target.Team != null)
                {
                    //...but has a team already
                    //Debug.Log ("Already in team");
                    SendInviteAck(target, eTeamRequestResult.TRR_MEMBER_IN_OTHER_TEAM, tid, inviter);
                    return;
                }
            }
            else
            {
                //Invalid target
                SendInviteAck(target, eTeamRequestResult.TRR_UNKNOWN_CHARACTER, tid, inviter);
                return;
            }


            if (inviter.Team != null)
            {
                //Handle inviter has team

                //Debug.Log (inviter.name + " inviting " + targetName + "to existing team" + tid);

                if (inviter.Team.Leader != inviter)
                {
                    //...but isn't leader
                    SendInviteAck(inviter, eTeamRequestResult.TRR_INCORRECT_INVITER, tid, inviter);
                    return;
                }


                if (inviter.Team.memberCount >= 5)
                {
                    //...but it's full

                    SendInviteAck(target, eTeamRequestResult.TRR_FULL, tid, inviter);
                    return;
                }
            }
            else
            {
                //New team; allocate a new potential ID
                var newTID = AllocateID();
                SendInvite(inviter, newTID, target);
                SendInviteAck(target, eTeamRequestResult.TRR_INVITE_SUCCESS, newTID, inviter);

                //Debug.Log (	inviter.name + " inviting " + targetName + "to new team: newTID = " + newTID);
                return;
            }

            //Send out the invite
            SendInvite(inviter, tid, target);
            SendInviteAck(target, eTeamRequestResult.TRR_INVITE_SUCCESS, tid, inviter);
        }

        public void handleInvitationAnswer(PlayerCharacter answerer, int tid, eTeamRequestResult answer, string requesterName)
        {
            //Debug.Log (	answerer.Name + " replied to " + requesterName + "'s invite to Team " + tid + ": result = " + answer);
            var requester = GameWorld.Instance.FindPlayerCharacter(requesterName);

            if (requester != null)
            {
                var existingTeam = requester.Team;


                switch (answer)
                {
                    case eTeamRequestResult.TRR_ACCEPT:


                        if (existingTeam != null)
                        {
                            if (existingTeam.memberCount < 5)
                            {
                                //If non-full existing group
                                Debug.Log("adding to existing team");
                                existingTeam.AddMember(answerer);
                                //existingTeam.LootModeAck(answerer,eTeamRequestResult.TRR_NONE,existingTeam.CurLootMode);
                            }
                            else
                            {
                                Debug.Log("Handle Team full error"); //If full existing team
                                SendInviteAck(answerer, eTeamRequestResult.TRR_FULL, tid, answerer);
                                SendInviteAck(answerer, eTeamRequestResult.TRR_FULL, tid, requester);
                                return;
                            }
                        }
                        else
                        {
                            //If no existing team

                            var newTeam = new PlayerTeam(tid);
                            newTeam.teamHandler = this;
                            newTeam.AddMember(requester);
                            newTeam.Leader = requester;
                            newTeam.AddMember(answerer);
                            //newTeam.SetLootMode (requester, ELootMode.LM_GROUP);
                            teams.Add(newTeam);
                            Debug.Log("TeamHandler : Created team with ID " + tid);
                        }


                        SendInviteAck(answerer, eTeamRequestResult.TRR_ACCEPT, tid, requester);
                        break;

                    case eTeamRequestResult.TRR_DECLINE:

                        SendInviteAck(answerer, eTeamRequestResult.TRR_DECLINE, tid, requester);

                        //Inviter allocated an ID if proposed team was new, so de-allocate this ID
                        if (existingTeam == null)
                        {
                            allocatedIDs.Remove(tid);
                        }
                        break;
                }
            }
        }

        void SendInvite(PlayerCharacter inviter, int teamID, PlayerCharacter target)
        {
            var m = new Message(GameHeader.S2C_TEAM_INVITE);
            m.WriteInt32(teamID);
            m.WriteString(inviter.Name);
            target.ReceiveRelevanceMessage(null, m);
        }

        void SendInviteAck(PlayerCharacter answerer, eTeamRequestResult answer, int teamID, PlayerCharacter target)
        {
            var m = new Message(GameHeader.S2C_TEAM_INVITE_ACK);
            m.WriteInt32((int) answer);
            m.WriteInt32(teamID);
            if (answerer)
                m.WriteString(answerer.Name);
            else m.WriteString("");
            target.ReceiveRelevanceMessage(null, m);
        }
    }
}