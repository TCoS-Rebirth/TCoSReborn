using System;
using System.Collections.Generic;
using Gameplay.Entities;
using Gameplay.RequirementSpecifier;
using UnityEngine;
using Utility;

namespace Gameplay.Events
{
    public class EV_Party : Content_Event
    {
        public Content_Event PartyAction;
        public float Range;
        public List<Content_Requirement> Requirements;

        protected bool meetsRequirements(PlayerCharacter pc)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(pc)) { return false; }
            }
            return true;
        }

        public override bool CanExecute(Entity obj, Entity subject)
        {
            if (!(subject as PlayerCharacter)) return false;
            if (!PartyAction) return false;
            if (!subject) return false;
            if (!PartyAction.CanExecute(obj, subject)) return false;

            return true;
        }

        protected override void Execute(Entity obj, Entity subject)
        {
            var playerSubj = subject as PlayerCharacter;
            if (meetsRequirements(playerSubj))
            {
                PartyAction.TryExecute(obj, subject);
            }

            var team = playerSubj.Team;
            if (team != null)
            {
                foreach(var teamMember in team.Members)
                {
                    if (teamMember == playerSubj) continue;
                    if (teamMember.ActiveZone != playerSubj.ActiveZone) continue;

                    var dist = Vector3.Distance(playerSubj.Position, teamMember.Position);
                    var unityRange = Range * UnitConversion.UnrUnitsToMeters;
                    if (dist > unityRange) continue;

                    if (!meetsRequirements(teamMember)) continue;
                    PartyAction.TryExecute(obj, teamMember);
                }
            }
        }
    }
}