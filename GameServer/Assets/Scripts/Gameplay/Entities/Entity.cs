using System;
using System.Collections.Generic;
using Common;
using Gameplay.Skills;
using Network;
using Pathfinding;
using UnityEngine;
using Utility;
using World;

namespace Gameplay.Entities
{
    /// <summary>
    ///     Base class for all entities in the game world, provides access to shared functionality
    /// </summary>
    public abstract class Entity : MonoBehaviour, IGridEntity<Entity>
    {
        [SerializeField, ReadOnly] string _name;

        [SerializeField, ReadOnly] Zone activeZone;

        [SerializeField] bool invisible;

        MapIDs lastZoneID = 0;

        [Header("Entity")] [SerializeField, ReadOnly] int relevanceID = -1;

        /// <summary>
        ///     A unique ID for this among all entities
        /// </summary>
        public int RelevanceID
        {
            get { return relevanceID; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        ///     controls visibility TODO currently unused
        /// </summary>
        public bool Invisible
        {
            get { return invisible; }
            set { invisible = value; }
        }

        /// <summary>
        ///     The ID of the last Zone this entity was in
        /// </summary>
        public MapIDs LastZoneID
        {
            get { return lastZoneID; }
            set { lastZoneID = value; }
        }

        /// <summary>
        ///     The current zone this entity resides in TODO refactor inner logic into methods
        /// </summary>
        public Zone ActiveZone
        {
            get { return activeZone; }
            set
            {
                if (value == null)
                {
                    OnLeavingZone(activeZone);
                }
                if (activeZone != value)
                {
                    if (value != null)
                    {
                        OnEnterZone(value);
                        lastZoneID = value.ID;
                    }
                }
                activeZone = value;
            }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>
        ///     override for performance reasons (the unique relevanceID)
        /// </summary>
        public override int GetHashCode()
        {
            return relevanceID;
        }

        /// <summary>
        ///     Assings this entity a uniqueID from <see cref="GameWorld.GetUniqueEntityID" />. Must be called as early as possible
        ///     after instantiation
        /// </summary>
        protected void RetrieveRelevanceID()
        {
            relevanceID = GameWorld.Instance.GetUniqueEntityID();
        }

        /// <summary>
        ///     Override this for regular updates to handle logic and similar. Called by the Zone's update loop
        /// </summary>
        public virtual void UpdateEntity()
        {
        }

        /// <summary>
        ///     This callback is invoked when the entity just entered a new zone
        /// </summary>
        protected virtual void OnEnterZone(Zone z)
        {
        }

        /// <summary>
        ///     This callback is invoked when the entity is about to leave the current zone (still in it at this point)
        /// </summary>
        protected virtual void OnLeavingZone(Zone z)
        {
        }

        /// <summary>
        ///     Call this to Teleport the entity to <see cref="newPos" /> with <see cref="newRot" />. override this to implement
        ///     the relevant broadcasts. Always call this base method
        /// </summary>
        public virtual void TeleportTo(Vector3 newPos, Quaternion newRot)
        {
            Position = newPos;
            Rotation = newRot;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PAWN_SV2CLREL_TELEPORTTO(this));
        }

        /// <summary>
        ///     Checks if this entity is facing forward towards <see cref="pos" /> in a cone defined by <see cref="fov" />
        /// </summary>
        /// <returns>true if inside the view cone</returns>
        public bool IsFacing(Vector3 pos, float fov)
        {
            return Vector3.Angle(pos - transform.position, transform.forward) <= fov;
        }


        /// <summary>
        ///     Shortcut for <see cref="IsInRange(Vector3,FSkill)" /> where the position is taken from the entity
        ///     <see cref="other" />
        /// </summary>
        public bool IsInRange(Entity other, FSkill s)
        {
            if (other == null)
            {
                return false;
            }
            return IsInRange(other.Position, s);
        }

        /// <summary>
        ///     Checks if <see cref="pos" /> is in range of Skill <see cref="s" />.
        ///     This evaluates if the skill is melee/directed or aoe (paintLocation)
        /// </summary>
        public bool IsInRange(Vector3 pos, FSkill s)
        {
            if (s.paintLocation)
            {
                var minDist = s.paintLocationMinDistance*UnitConversion.UnrUnitsToMeters;
                minDist = minDist*minDist;
                var maxDist = s.paintLocationMaxDistance*UnitConversion.UnrUnitsToMeters;
                maxDist = maxDist*maxDist;
                var dist = Vector3.SqrMagnitude(pos - Position);
                return dist <= maxDist && dist >= minDist;
            }
            else
            {
                var minDist = s.minDistance*UnitConversion.UnrUnitsToMeters;
                minDist = minDist*minDist;
                var maxDist = s.maxDistance*UnitConversion.UnrUnitsToMeters;
                maxDist = maxDist*maxDist;
                var dist = Vector3.SqrMagnitude(pos - Position);
                return dist <= maxDist && dist >= minDist;
            }
        }

        /// <summary>
        ///     Fast way to check if this entity is closer to <see cref="toPos" /> than <see cref="thanDist" />
        /// </summary>
        public bool IsXZDistanceCloserToThan(Vector3 toPos, float thanDist)
        {
            return VectorMath.SqrDistanceXZ(Position, toPos) < thanDist*thanDist;
        }

        /// <summary>
        ///     Fast way to check if this entity is further to <see cref="toPos" /> than <see cref="thanDist" />
        /// </summary>
        public bool IsXZDistanceFurtherToThan(Vector3 toPos, float thanDist)
        {
            return VectorMath.SqrDistanceXZ(Position, toPos) > thanDist*thanDist;
        }

        #region Interest Management

        [NonSerialized] public int RelObjectCount;

        [NonSerialized] public bool RelevanceContainsPlayers;

        /// <summary>
        ///     List of all relevant/in range entities
        /// </summary>
        protected List<Entity> relevantObjects = new List<Entity>();

        /// <summary>
        ///     Call this to allow overriding classes to handle a message broadcast (used in player client sync), returns true if
        ///     the sender is relevant to this entity
        /// </summary>
        public virtual bool ReceiveRelevanceMessage(Entity rel, Message m)
        {
            if (rel != null && !relevantObjects.Contains(rel))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Call this to broadcast a Message to all entites in relevance
        /// </summary>
        /// <param name="m"></param>
        protected void BroadcastRelevanceMessage(Message m)
        {
            for (var i = 0; i < relevantObjects.Count; i++)
            {
                relevantObjects[i].ReceiveRelevanceMessage(this, m);
            }
        }

        /// <summary>
        ///     If found, returns the entity with the given (relevance-)ID from the entity's relevance
        /// </summary>
        public Entity GetRelevantObject(int id)
        {
            for (var i = relevantObjects.Count; i-- > 0;)
            {
                if (relevantObjects[i].RelevanceID == id)
                {
                    return relevantObjects[i];
                }
            }
            return null;
        }

        /// <summary>
        ///     If found, returns the entity as <see cref="T" /> if it is of the requested type from the entity's relevance
        /// </summary>
        public T GetRelevantEntity<T>(int id) where T : Entity
        {
            for (var i = relevantObjects.Count; i-- > 0;)
            {
                if (relevantObjects[i].RelevanceID == id)
                {
                    return relevantObjects[i] as T;
                }
            }
            return null;
        }


        /// <summary>
        ///     Returns a list of entities of the given Type <see cref="T" /> from the entity's relevance
        /// </summary>
        public List<T> GetRelevantEntiesOfType<T>() where T : Entity
        {
            var foundEntities = new List<T>();
            for (var i = relevantObjects.Count; i-- > 0;)
            {
                if (relevantObjects[i] is T)
                {
                    foundEntities.Add(relevantObjects[i] as T);
                }
            }
            return foundEntities;
        }

        /// <summary>
        ///     Returns a list of entities of the given Type <see cref="T" /> from the entity's relevance and evaluates their
        ///     distance in the query
        /// </summary>
        public List<T> GetRelevantEntiesOfType<T>(float distance) where T : Entity
        {
            var foundEntities = new List<T>();
            for (var i = relevantObjects.Count; i-- > 0;)
            {
                if (relevantObjects[i] == null)
                {
                    relevantObjects.RemoveAt(i);
                    continue;
                }
                if (relevantObjects[i] is T && relevantObjects[i].IsXZDistanceCloserToThan(Position, distance))
                {
                    foundEntities.Add(relevantObjects[i] as T);
                }
            }
            return foundEntities;
        }

        public bool KnowsRelevantObjectOfType<T>() where T : Entity
        {
            for (var i = 0; i < relevantObjects.Count; i++)
            {
                if (relevantObjects[i] is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     This is called by the GridSystem to enable overriding classes to handle a new visible entity in relevance (used in
        ///     player client sync)
        ///     if overriden include a call to the base method
        /// </summary>
        public virtual void OnEntityBecameRelevant(Entity other)
        {
            if (other.RelevanceID == -1)
            {
                Debug.LogWarning(other + " has invalid relID");
                return;
            }
            relevantObjects.Add(other);
            if (other is PlayerCharacter)
            {
                RelevanceContainsPlayers = true;
            }
            RelObjectCount = relevantObjects.Count;
        }

        /// <summary>
        ///     This is called by the Gridsystem to enable overriding classes to handle a destroyed or otherwise now invisible
        ///     entity in earlier relevance (used in player client sync)
        ///     if overriden include a call to the base method
        /// </summary>
        public virtual void OnEntityBecameIrrelevant(Entity other)
        {
            relevantObjects.Remove(other);
            RelObjectCount = relevantObjects.Count;
            RelevanceContainsPlayers = KnowsRelevantObjectOfType<PlayerCharacter>();
        }

        public void OnEntityBecameIrrelevant(int otherID)
        {
            for (var i = 0; i < relevantObjects.Count; i++)
            {
                if (relevantObjects[i].relevanceID != otherID) continue;
                relevantObjects.RemoveAt(i);
                break;
            }
            RelObjectCount = relevantObjects.Count;
            RelevanceContainsPlayers = KnowsRelevantObjectOfType<PlayerCharacter>();
        }

        int IGridEntity<Entity>.GetID()
        {
            return relevanceID;
        }

        #endregion
    }
}