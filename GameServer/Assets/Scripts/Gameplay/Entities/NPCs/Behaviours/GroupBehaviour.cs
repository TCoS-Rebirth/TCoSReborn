using Common;
using Gameplay.Skills;
using Pathfinding;
using UnityEngine;

//Behaviour for NPC groups (from spawn deployers)
//State changes will be managed by the spawn deployer this
//NPC belongs to instead of by the NpcCharacter's own conditions
//(by setting this Behaviour's states foreach deployer NPC?)

namespace Gameplay.Entities.NPCs.Behaviours
{
    internal class GroupBehaviour : NPCBehaviour
    {
        public float aggroRadius = 10f; //TODO: find good value or get from npc

        bool canPath;

        float distanceSquaredToCurrentTarget;

        public float homeRadius = 50f;

        float lastAttack;

        float lastQuery;

        float lastRotation;
        const float rotationUpdate = 0.75f;

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
            get { return "GroupBehaviour"; }
        }

        protected override void OnStart()
        {
            startPosition = owner.Position;
            startOrientation = owner.Rotation;
            owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
            rndTargetPos = startPosition;
            nextRndPath = Time.time + Random.Range(5f, 15f);
            canPath = !ReferenceEquals(owner.ActiveZone, null) && owner.ActiveZone.HasCollision;
        }

        //TODO: add threat from source character to own table / group table?
        public override void OnDamage(Character source, FSkill_Type s, int amount)
        {
            if (!canPath)
            {
                return;
            }
            if (!ReferenceEquals(target, null))
            {
                owner.CancelMovement();
                target = source;
                owner.CombatState.sv_DrawWeapon(ECombatMode.CBM_Melee);
                Debug.Log("TODO replace with correct initial state resolving function");
            }
        }

        //TODO: Remove anything managed by the spawn deployer
        protected override void OnUpdate()
        {
            _currentPosition = owner.Position;
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
                if (!ReferenceEquals(target,null))
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
                    if (Time.time - lastRotation > rotationUpdate)
                    {
                        var targetPos = _targetPosition;
                        targetPos.y = _currentPosition.y;
                        owner.Rotation = Quaternion.LookRotation(targetPos - _currentPosition, Vector3.up);
                        owner.SetFocusLocation(_targetPosition);
                        lastRotation = Time.time;
                    }
                    distanceSquaredToCurrentTarget = Vector3.SqrMagnitude(_targetPosition - _currentPosition);
                    if (Vector3.SqrMagnitude(_targetPosition - startPosition) > homeRadius*homeRadius)
                    {
                        target = null;
                        state = NpcStates.Fleeing;
                        return;
                    }
                }
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
            if (owner.PawnState == EPawnStates.PS_ALIVE)
            {
                state = NpcStates.Idle;
                owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
            }
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
                    nextRndPath = Time.time + Random.Range(5f, 10f); //repath
                    var maxDist = owner.RespawnInfo.maxSpawnDistance;
                    rndTargetPos = startPosition + new Vector3(Random.Range(-maxDist, maxDist), 0f, Random.Range(-maxDist, maxDist));
                }
                    break;

                case NpcCharacter.MoveResult.SearchingPath:
                    if (Time.time - lastRotation > rotationUpdate)
                    {
                        owner.SetFocusLocation(rndTargetPos);
                        lastRotation = Time.time;
                    }
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
                foreach (var p in potentialTargets)
                {
                    if (!owner.Faction.Likes(p.Faction))
                    {
                        target = p;
                        break;
                    }
                }
            }
            lastQuery = Time.time;
        }

        void OnApproaching()
        {
            if (!target)
            {
                state = NpcStates.Fleeing;
                return;
            }
            var result = owner.MoveTo(_targetPosition + (_currentPosition - _targetPosition).normalized*minDistanceToMoveTarget);
            if (HasLeftHabitat())
            {
                target = null;
                state = NpcStates.Fleeing;
                return;
            }
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
            if (!target)
            {
                state = NpcStates.Fleeing;
                return;
            }
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
                    owner.Stats.SetHealth(owner.Stats.MaxHealth);
                    owner.TeleportTo(startPosition, startOrientation);
                    owner.CombatState.sv_SheatheWeapon();
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
            return VectorMath.SqrDistanceXZ(startPosition, _currentPosition) > homeRadius*homeRadius;
        }

        void HandleCombat()
        {
            if (Time.time - lastAttack > 2f)
            {
                if (!owner.Skills.IsCasting)
                {
                    var result = owner.Skills.ExecuteIndex(0, target.RelevanceID, _currentPosition, Time.time);
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
    }
}