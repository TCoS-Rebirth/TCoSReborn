using Common;
using Gameplay.Skills;
using Pathfinding;
using UnityEngine;

//Behaviour for a lone, stationary combat NPC
//Lifted directly from old WildlifeBehaviour

namespace Gameplay.Entities.NPCs.Behaviours
{
    internal class KillerBehaviour : NPCBehaviour
    {
        bool canPath;

        float aggroRadius;
        float distanceSquaredToCurrentTarget;
        float distanceSquaredToHomePoint;

        public float homeRadius = 50f;

        float lastAttack;

        float lastQuery;

        float lastRotation;

        [SerializeField] float maxDistanceToMoveTarget = 3f;

        [SerializeField] float minDistanceToMoveTarget = 1f;

        float nextRndPath;

        Vector3 rndTargetPos;

        Quaternion startOrientation;

        [SerializeField] Vector3 startPosition;

        [SerializeField] NpcStates state = NpcStates.Idle;

        [SerializeField] Character target;

        Vector3 _currentPosition;
        Vector3 _targetPosition;

        public override string Description
        {
            get { return "KillerBehaviour"; }
        }

        protected override void OnStart()
        {
            if (!ReferenceEquals(owner.ActiveZone, null))
            {
                //Valshaaran - amended _currentPosition to owner.Position in 1st parameter
                //Not sure if _currentPosition ought to be set to this elsewhere?
                owner.Position = owner.ActiveZone.Raycast(owner.Position + Vector3.up, Vector3.down, 10f);
            }
            owner.RespawnInfo.initialSpawnPoint = owner.Position;
            startPosition = owner.Position;
            startOrientation = owner.Rotation;
            owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
            rndTargetPos = startPosition;
            nextRndPath = Time.time + Random.Range(5f, 15f);
            canPath = owner.ActiveZone != null && owner.ActiveZone.HasCollision;
            aggroRadius = owner.RespawnInfo.aggroRadius;
        }

        public override void OnDamage(Character source, FSkill s, int amount)
        {
            if (!canPath)
            {
                return;
            }
            if (!ReferenceEquals(target,null))
            {
                owner.CancelMovement();
                target = source;
                owner.equippedWeaponType = EWeaponCategory.EWC_MeleeOrUnarmed;
                owner.DrawWeapon();
            }
        }

        protected override void OnUpdate()
        {
            if (owner.PawnState == EPawnStates.PS_DEAD)
            {
                if (state != NpcStates.Dead)
                {
                    target = null;
                    state = NpcStates.Dead;
                }
            }
            else
            {
                _currentPosition = owner.Position;
                if (!ReferenceEquals(target, null))
                {
                    _targetPosition = target.Position;
                    if (!canPath)
                    {
                        target = null;
                        return;
                    }
                    if (target.PawnState == EPawnStates.PS_DEAD)
                    {
                        target = null;
                        state = NpcStates.Fleeing;
                        return;
                    }
                    if (Time.time - lastRotation > 0.5f)
                    {
                        var targetPos = _targetPosition;
                        targetPos.y = _currentPosition.y;
                        owner.Rotation = Quaternion.LookRotation(targetPos - _currentPosition, Vector3.up);
                        owner.SetFocusLocation(_targetPosition);
                        lastRotation = Time.time;
                    }
                    distanceSquaredToCurrentTarget = VectorMath.SqrDistanceXZ(_targetPosition, _currentPosition);
                    if (Vector3.SqrMagnitude(_targetPosition - startPosition) > homeRadius*homeRadius)
                    {
                        target = null;
                        state = NpcStates.Fleeing;
                        return;
                    }
                }
                distanceSquaredToHomePoint = VectorMath.SqrDistanceXZ(startPosition, _currentPosition);
            }
            switch (state)
            {
                case NpcStates.Idle:
                    OnIdle();
                    break;
                case NpcStates.Approaching:
                    OnApproaching();
                    break;
                case NpcStates.InRange:
                    OnInRange();
                    break;
                case NpcStates.Fleeing:
                    OnFleeing();
                    break;
                case NpcStates.Dead:
                    OnDead();
                    break;
            }
        }

        void OnDead()
        {
            if (owner.PawnState != EPawnStates.PS_ALIVE) return;
            state = NpcStates.Idle;
            owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
        }

        void OnIdle()
        {
            PathAround();
            LookForTargets();
            if (ReferenceEquals(target, null)) return;
            owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Normal);
            state = NpcStates.Approaching;
        }

        void PathAround()
        {
            if (!canPath)
            {
                return;
            }
            if (!(Time.time > nextRndPath)) return;
            var res = owner.MoveTo(rndTargetPos);
            switch (res)
            {
                case NpcCharacter.MoveResult.ReachedTarget:
                {
                    nextRndPath = Time.time + Random.Range(5f, 15f);
                    var maxDist = owner.RespawnInfo.maxSpawnDistance;
                    rndTargetPos = startPosition + new Vector3(Random.Range(-maxDist, maxDist), 0f, Random.Range(-maxDist, maxDist));
                }
                    break;
                case NpcCharacter.MoveResult.TargetNotReachable:
                {
                    nextRndPath = Time.time + Random.Range(4f, 5f); //repath
                    var maxDist = owner.RespawnInfo.maxSpawnDistance;
                    rndTargetPos = startPosition + new Vector3(Random.Range(-maxDist, maxDist), 0f, Random.Range(-maxDist, maxDist));
                }
                    break;
                case NpcCharacter.MoveResult.SearchingPath:
                    owner.SetFocusLocation(rndTargetPos);
                    break;
                default:
                    nextRndPath = Time.time + Random.Range(1f, 2f); //prevent too frequent updates
                    break;
            }
        }

        void LookForTargets()
        {
            if (owner.RelObjectCount == 0)
            {
                return;
            }
            if (!(Time.time - lastQuery > 1f)) return;
            var potentialTargets = owner.GetRelevantEntiesOfType<PlayerCharacter>(aggroRadius);
            if (potentialTargets.Count > 0)
            {
                for (var i = 0; i < potentialTargets.Count; i++)
                {
                    if (owner.Faction.Likes(potentialTargets[i].Faction)) continue;
                    target = potentialTargets[i];
                    break;
                }
            }
            lastQuery = Time.time;
        }

        void OnApproaching()
        {
            if (HasLeftHabitat())
            {
                target = null;
                state = NpcStates.Fleeing;
                return;
            }
            var result = owner.MoveTo(_targetPosition + (_currentPosition - _targetPosition).normalized*minDistanceToMoveTarget);
            switch (result)
            {
                case NpcCharacter.MoveResult.TargetNotReachable:
                    target = null;
                    state = NpcStates.Fleeing;
                    return;
                case NpcCharacter.MoveResult.ReachedTarget:
                    state = NpcStates.InRange;
                    return;
            }
        }

        void OnInRange()
        {
            if (HasLeftHabitat())
            {
                target = null;
                state = NpcStates.Fleeing;
                return;
            }
            if (IsOutOfAttackRange())
            {
                state = NpcStates.Approaching;
            }
            else
            {
                HandleCombat();
            }
        }

        void OnFleeing()
        {
            var result = owner.MoveTo(startPosition);
            switch (result)
            {
                case NpcCharacter.MoveResult.ReachedTarget:
                case NpcCharacter.MoveResult.TargetNotReachable:
                    owner.SetHealth(owner.MaxHealth);
                    owner.TeleportTo(startPosition, startOrientation);
                    owner.SheatheWeapon();
                    state = NpcStates.Idle;
                    owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
                    return;
            }
        }

        bool IsOutOfAttackRange()
        {
            return distanceSquaredToCurrentTarget > maxDistanceToMoveTarget*maxDistanceToMoveTarget;
        }

        bool HasLeftHabitat()
        {
            return distanceSquaredToHomePoint > homeRadius*homeRadius;
        }

        void HandleCombat()
        {
            if (Time.time - lastAttack > 2f)
            {
                if (!owner.IsCasting)
                {
                    var result = owner.UseSkillIndex(0, target.RelevanceID, _currentPosition, Time.time);
                    if (result == ESkillStartFailure.SSF_INVALID_SKILL)
                    {
                        state = NpcStates.Fleeing;
                        return;
                    }
                }
                lastAttack = Time.time;
            }
        }

        enum NpcStates
        {
            Idle,
            Approaching,
            InRange,
            Fleeing,
            Dead
        }

        public void ForceAggro(Character tc)
        {
            target = tc;
            state = NpcStates.Approaching;
        }
    }
}