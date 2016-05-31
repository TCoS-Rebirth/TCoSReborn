using System.Collections.Generic;
using Common;
using UnityEngine;
using World.Paths;

namespace Gameplay.Entities.NPCs.Behaviours
{
    internal class PathingBehaviour : NPCBehaviour
    {
        List<Vector3> _currentPath = new List<Vector3>();

        PatrolPoint _currentPathPoint;
        int _currentPathTargetIndex;

        [SerializeField, ReadOnly] bool _doPatrol;

        float _lastRotation;

        public override string Description
        {
            get { return "PathingBehaviour"; }
        }

        protected override void OnStart()
        {
            if ((_currentPathPoint = GetHasLinkedPatrolPoint()) != null)
            {
                if (_currentPathPoint.PatrolPaths.Count > 0 && _currentPathPoint.PatrolPaths[0].Path.Count > 0)
                {
                    SetNewPath(_currentPathPoint.PatrolPaths[0].Path);
                    owner.SetMoveSpeed(ENPCMovementFlags.ENMF_Walking);
                }
            }
        }

        void SetNewPath(List<Vector3> newPath)
        {
            if (newPath == null || newPath.Count == 0)
            {
                _doPatrol = false;
            }
            _currentPath = newPath;
            owner.SetFocusLocation(_currentPath[_currentPath.Count - 1]);
            _currentPathTargetIndex = 0;
            _doPatrol = true;
        }

        protected override void OnUpdate()
        {
            if (owner.PawnState == EPawnStates.PS_DEAD)
            {
                return;
            }
            if (_doPatrol && _currentPath.Count > 0 && !owner.isConversing)
            {
                if (owner.MoveToDirect(_currentPath[_currentPathTargetIndex]) == NpcCharacter.MoveResult.ReachedTarget)
                {
                    OnReachedPathTarget();
                }
            }
        }

        void OnReachedPathTarget()
        {
            if (_currentPathTargetIndex == _currentPath.Count - 1)
            {
                if (_currentPathPoint.Connections.Count > 0)
                {
                    var linkedPoint = _currentPathPoint.Connections[0].ConnectedActor as PatrolPoint;
                    if (linkedPoint != null && linkedPoint.PatrolPaths.Count > 0)
                    {
                        _currentPathPoint = linkedPoint;
                        SetNewPath(linkedPoint.PatrolPaths[0].Path);
                    }
                    else
                    {
                        _doPatrol = false;
                    }
                }
                else
                {
                    _doPatrol = false;
                }
            }
            else
            {
                _currentPathTargetIndex += 1;
            }
        }
    }
}