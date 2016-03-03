using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Gameplay.Entities;
using JetBrains.Annotations;
using UnityEngine;

namespace World
{
    /// <summary>
    ///     Entities used in the SimpleWorldGrid must implement this interface
    /// </summary>
    /// <typeparam name="T">The class that implements this interface</typeparam>
    public interface IGridEntity<T> where T : class
    {
        Vector3 Position { get; }

        /// <summary>
        ///     This will be called when another entity in the grid enters the subscription range
        /// </summary>
        void OnEntityBecameRelevant(T other);

        /// <summary>
        ///     This will be called when a subscribed entity in the grid leaves the subscription range
        /// </summary>
        void OnEntityBecameIrrelevant(T other);

        /// <summary>
        ///     This will be called when a subscribed entity in the grid became invalid
        /// </summary>
        /// <param name="otherID">id of the entity that was removed</param>
        void OnEntityBecameIrrelevant(int otherID);

        /// <summary>
        ///     Needed to unsubscribe invalid entities (i.e. removed during update)
        /// </summary>
        int GetID();
    }

    public class SimpleWorldGrid<T> where T : class, IGridEntity<T>
    {
        readonly List<GridCell> _allCells = new List<GridCell>();

        readonly Dictionary<int, Dictionary<int, GridCell>> _cells = new Dictionary<int, Dictionary<int, GridCell>>();
        readonly float _cellSize;
        readonly List<GridEntry> _nonPlayerEntries = new List<GridEntry>();

        readonly MonoBehaviour _owner;
        readonly List<GridEntry> _players = new List<GridEntry>();

        readonly HashSet<int> _removeQueue = new HashSet<int>();
        readonly float _subscriptionDistanceSquared;
        readonly Coroutine _updateRoutine;

        readonly Stopwatch _updateTimer = new Stopwatch();

        float _updateCycleTime;

        /// <summary>
        ///     pauses all calculations (entities won't be updated about their relevance)
        /// </summary>
        public bool IsPaused = false;

        /// <summary>
        ///     Initializes a grid instance
        /// </summary>
        /// <param name="subscriptionDistance">The distance under which entities become relevant to each other</param>
        /// <param name="owner">owner is required to start the internal update coroutine</param>
        public SimpleWorldGrid(float subscriptionDistance, [NotNull] MonoBehaviour owner)
        {
            if (owner == null) throw new ArgumentNullException("owner cannot be null");
            _subscriptionDistanceSquared = subscriptionDistance*subscriptionDistance;
            _cellSize = subscriptionDistance + 1;
            _owner = owner;
            _updateRoutine = owner.StartCoroutine(UpdateRoutine());
            Profiler.maxNumberOfSamplesPerFrame = -1;
        }

        public float UpdateCycleTime
        {
            get { return _updateCycleTime; }
        }

        public void ShutDown()
        {
            if (_updateRoutine != null)
            {
                _owner.StopCoroutine(_updateRoutine);
            }
            _nonPlayerEntries.Clear();
            _players.Clear();
        }

        IEnumerator UpdateRoutine()
        {
            RESTART:
            while (_nonPlayerEntries.Count == 0 && _players.Count == 0 || IsPaused)
            {
                yield return null;
            }
            _updateTimer.Reset();
            _updateTimer.Start();
            for (var i = _nonPlayerEntries.Count; i-- > 0;)
            {
                if (i%GetFrameSplit() == 0)
                {
                    yield return null;
                }
                var ge = _nonPlayerEntries[i];
                if (ge.Owner == null | _removeQueue.Contains(ge.ID))
                {
                    if (ge.CenterCell != null)
                    {
                        ge.CenterCell.Entities.Remove(ge);
                    }
                    ge.ClearSubscriptions();
                    _nonPlayerEntries.RemoveAt(i);
                    _removeQueue.Remove(ge.ID);
                }
                else
                {
                    ge.CachedPosition = ge.Owner.Position;
                    ProcessNonPlayerEntity(ge);
                }
            }
            for (var i = _players.Count; i-- > 0;)
            {
                var ge = _players[i];
                if (ge.Owner == null | _removeQueue.Contains(ge.ID))
                {
                    if (ge.CenterCell != null)
                    {
                        ge.CenterCell.Entities.Remove(ge);
                        ge.ClearSubscriptions();
                        _players.RemoveAt(i);
                        _removeQueue.Remove(ge.ID);
                    }
                }
                else
                {
                    ge.CachedPosition = ge.Owner.Position;
                    ProcessPlayerEntity(ge);
                }
            }
            _updateTimer.Stop();
            _updateCycleTime = _updateTimer.ElapsedMilliseconds;
            goto RESTART;
        }

        int GetFrameSplit()
        {
            if (_nonPlayerEntries.Count <= 200)
            {
                return 200;
            }
            return _nonPlayerEntries.Count/10;
        }

        void ProcessNonPlayerEntity(GridEntry ge)
        {
            var x = Mathf.CeilToInt(ge.CachedPosition.x/_cellSize);
            var z = Mathf.CeilToInt(ge.CachedPosition.z/_cellSize);
            if (ge.CenterCell == null || (ge.CenterCell.X != x | ge.CenterCell.Z != z))
            {
                if (ge.CenterCell != null)
                {
                    ge.CenterCell.Entities.Remove(ge);
                }
                GetCell(x, z, out ge.CenterCell);
                ge.CenterCell.Entities.Add(ge);
            }
        }

        void ProcessPlayerEntity(GridEntry ge)
        {
            var x = Mathf.CeilToInt(ge.CachedPosition.x/_cellSize);
            var z = Mathf.CeilToInt(ge.CachedPosition.z/_cellSize);
            if (ge.CenterCell == null || (ge.CenterCell.X != x | ge.CenterCell.Z != z))
            {
                if (ge.CenterCell != null)
                {
                    ge.CenterCell.Entities.Remove(ge);
                }
                GetCell(x, z, out ge.CenterCell);
                ge.CenterCell.Entities.Add(ge);
            }
            UpdatePlayerBasedSubscriptions(ge);
        }

        void GetCell(int x, int z, out GridCell cell)
        {
            Dictionary<int, GridCell> d1;
            if (!_cells.TryGetValue(x, out d1))
            {
                d1 = new Dictionary<int, GridCell>();
                _cells.Add(x, d1);
            }
            if (!d1.TryGetValue(z, out cell))
            {
                cell = new GridCell(x, z);
                d1.Add(z, cell);
                _allCells.Add(cell);
            }
            if (!cell.Initialized)
            {
                InitializeCell(cell);
            }
        }

        void InitializeCell(GridCell cell)
        {
            for (var x = cell.X - 1; x <= cell.X + 1; x++)
            {
                for (var z = cell.Z - 1; z <= cell.Z + 1; z++)
                {
                    if (x == cell.X && z == cell.Z)
                    {
                        continue;
                    }
                    GridCell neighbor;
                    Dictionary<int, GridCell> d1;
                    if (!_cells.TryGetValue(x, out d1))
                    {
                        d1 = new Dictionary<int, GridCell>();
                        _cells.Add(x, d1);
                    }
                    if (!d1.TryGetValue(z, out neighbor))
                    {
                        neighbor = new GridCell(x, z);
                        d1.Add(z, neighbor);
                        _allCells.Add(neighbor);
                    }
                    cell.NeighborCells.Add(neighbor);
                }
            }
            cell.Initialized = true;
        }

        /// <summary>
        ///     Registers an entity to the grid for regular proximity updates
        /// </summary>
        /// <param name="entity">The entity to register</param>
        public void Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity is null");
            var entry = new GridEntry(entity);
            if (entity is PlayerCharacter)
            {
                for (var i = 0; i < _players.Count; i++)
                {
                    if (_players[i].Owner == entity)
                    {
                        return;
                    }
                }
                _players.Add(entry);
            }
            else
            {
                for (var i = 0; i < _nonPlayerEntries.Count; i++)
                {
                    if (_nonPlayerEntries[i].Owner == entity)
                    {
                        return;
                    }
                }
                _nonPlayerEntries.Add(entry);
            }
        }

        /// <summary>
        ///     Queues an entity for removing from the grid
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void Remove(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity is null");
            _removeQueue.Add(entity.GetID());
        }

        void UpdatePlayerBasedSubscriptions(GridEntry ge)
        {
            var currentEntities = ge.RecycledSubscriptionList;
            var subList = ge.Subscriptions;
            var cellEntities = ge.CenterCell.Entities;
            for (var e = 0; e < cellEntities.Count; e++)
            {
                if (ReferenceEquals(cellEntities[e], ge))
                {
                    continue;
                }
                if (Vector3.SqrMagnitude(ge.CachedPosition - cellEntities[e].CachedPosition) > _subscriptionDistanceSquared)
                {
                    continue;
                }
                if (!subList.Remove(cellEntities[e]))
                {
                    ge.Owner.OnEntityBecameRelevant(cellEntities[e].Owner);
                    cellEntities[e].Owner.OnEntityBecameRelevant(ge.Owner);
                }
                currentEntities.Add(cellEntities[e]);
            }
            for (var n = 0; n < ge.CenterCell.NeighborCells.Count; n++)
            {
                cellEntities = ge.CenterCell.NeighborCells[n].Entities;
                for (var e = 0; e < cellEntities.Count; e++)
                {
                    if (Vector3.SqrMagnitude(ge.CachedPosition - cellEntities[e].CachedPosition) > _subscriptionDistanceSquared)
                    {
                        continue;
                    }
                    if (!subList.Remove(cellEntities[e]))
                    {
                        ge.Owner.OnEntityBecameRelevant(cellEntities[e].Owner);
                        cellEntities[e].Owner.OnEntityBecameRelevant(ge.Owner);
                    }
                    currentEntities.Add(cellEntities[e]);
                }
            }
            ge.ClearSubscriptions();
            ge.RecycledSubscriptionList = ge.Subscriptions;
            ge.Subscriptions = currentEntities;
        }

        class GridEntry : IEquatable<GridEntry>
        {
            public readonly int ID;
            public readonly T Owner;
            public Vector3 CachedPosition;
            public GridCell CenterCell;
            public HashSet<GridEntry> RecycledSubscriptionList = new HashSet<GridEntry>();

            public HashSet<GridEntry> Subscriptions = new HashSet<GridEntry>();

            public GridEntry(T entity)
            {
                ID = entity.GetID();
                Owner = entity;
            }

            public void ClearSubscriptions()
            {
                foreach (var subscription in Subscriptions)
                {
                    if (subscription.Owner != null)
                    {
                        Owner.OnEntityBecameIrrelevant(subscription.Owner);
                        subscription.Owner.OnEntityBecameIrrelevant(Owner);
                    }
                    else
                    {
                        Owner.OnEntityBecameIrrelevant(subscription.ID);
                    }
                }
                Subscriptions.Clear();
            }

            #region internal

            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj);
            }

            public override int GetHashCode()
            {
                return ID;
            }

            public bool Equals(GridEntry other)
            {
                return ReferenceEquals(this, other);
            }

            #endregion
        }

        class GridCell
        {
            public readonly List<GridEntry> Entities = new List<GridEntry>();
            public readonly List<GridCell> NeighborCells = new List<GridCell>();
            public readonly int X;
            public readonly int Z;
            public bool Initialized;

            public GridCell(int x, int z)
            {
                Initialized = false;
                X = x;
                Z = z;
            }
        }
    }
}