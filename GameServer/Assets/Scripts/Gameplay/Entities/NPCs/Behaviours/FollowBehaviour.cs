using System.Collections.Generic;
using Common;
using UnityEngine;
using World.Paths;

namespace Gameplay.Entities.NPCs.Behaviours
{
    internal class FollowBehaviour : NPCBehaviour
    {
        
        /// <summary>
        /// Npc stops moving to target if they attain under this distance
        /// </summary>
        const float stopMovingMinDist = 1.0f;

        /// <summary>
        /// Npc resumes moving to target if the target is over this distance
        /// </summary>
        const float startMovingMaxDist = 5.0f;

        Character _followTarget;

        [SerializeField, ReadOnly]
        bool _reachedMinDist;

        public override string Description
        {
            get { return "FollowBehaviour"; }
        }

        public void setTarget(Character t) { _followTarget = t; }

        protected override void OnUpdate()
        {
            if (owner.PawnState == EPawnStates.PS_DEAD)
            {
                return;
            }

            if (_reachedMinDist)
            {
                if (owner.IsXZDistanceFurtherToThan(_followTarget.Position,startMovingMaxDist))
                {
                    _reachedMinDist = false;
                    owner.MoveTo(_followTarget.Position);
                }
            }
            else
            {
                if (owner.IsXZDistanceCloserToThan(_followTarget.Position, stopMovingMinDist))
                {
                    _reachedMinDist = true;
                }
            }
        }
    }
}