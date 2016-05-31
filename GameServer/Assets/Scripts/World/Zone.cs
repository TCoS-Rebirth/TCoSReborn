using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using Network;
using UnityEngine;
using Utility;
using ZoneScripts;
using Gameplay.Entities.Interactives;
using Database.Static;

namespace World
{
    /// <summary>
    ///     This class represents an SBWorld
    /// </summary>
    public class Zone : MonoBehaviour
    {
        readonly List<PlayerStart> _destinations = new List<PlayerStart>();
        readonly List<InteractiveLevelElement> _interactiveElements = new List<InteractiveLevelElement>();
        readonly List<NpcCharacter> _npcs = new List<NpcCharacter>();
        readonly List<PlayerCharacter> _players = new List<PlayerCharacter>();
        readonly List<Trigger> _triggers = new List<Trigger>();

        readonly List<SBEvent> _events = new List<SBEvent>();
        readonly List<SBScriptedEvent> _scriptedEvents = new List<SBScriptedEvent>();

        ZoneScript _script;

        SimpleWorldGrid<Entity> _zoneGrid;

        //makes this zone process its entities and enables entering via portal etc.
        public bool IsEnabled = true;

        /// <summary>
        ///     List of all player characters in this zone
        /// </summary>
        public ReadOnlyCollection<PlayerCharacter> Players
        {
            get { return _players.AsReadOnly(); }
        }

        /// <summary>
        ///     Number of playercharacters in this zone
        /// </summary>
        public int PlayerCount
        {
            get { return _players.Count; }
        }

        /// <summary>
        ///     List of all npc characters in this zone
        /// </summary>
        public ReadOnlyCollection<NpcCharacter> Npcs
        {
            get { return _npcs.AsReadOnly(); }
        }

        /// <summary>
        ///     List of all ILEs in this zone
        /// </summary>
        public ReadOnlyCollection<InteractiveLevelElement> InteractiveElements
        {
            get { return _interactiveElements.AsReadOnly(); }
        }

        /// <summary>
        ///     nicely formatted info string
        /// </summary>
        public override string ToString()
        {
            return string.Format("Name: {0}, ID: {1}", ReadableName, _id);
        }

        void Awake()
        {
            _zoneGrid = new SimpleWorldGrid<Entity>(GameConfiguration.Get.world.ZoneRelevanceRadius, this);
        }

        public void ShutDown()
        {
            if (_script != null)
            {
                _script.OnBeforeShutDown();
            }
            if (_zoneGrid != null)
            {
                _zoneGrid.ShutDown();
            }
        }

        /// <summary>
        ///     Routine to load the zone over several frames to prevent the application from freezing. Executed by the
        ///     <see cref="ZoneHandler" />
        /// </summary>
        public IEnumerator Load()
        {
            _script = ZoneScript.GetScript(_id);
            _script.Attach(this);
            foreach (var portal in portalHolder.GetComponentsInChildren<SBWorldPortal>())
            {
                portal.owningZone = this;
            }
            var yieldafter = 0;
            foreach (var npc in npcHolder.GetComponentsInChildren<NpcCharacter>())
            {
                AddToZone(npc);
                yieldafter++;
                if (yieldafter%50 == 0)
                {
                    yield return null;
                }
            }

            //Enable spawn deployers
            yieldafter = 0;
            foreach (var sd in npcSpawnerHolder.GetComponentsInChildren<SpawnDeployer>())
            {
                //Move each deployer to needs redeploy
                sd.respawnPending = true;
                sd.moveNextState(EDeployerCommand.Enable);
                yieldafter++;
                if (yieldafter%50 == 0)
                {
                    yield return null;
                }
            }

            //Enable spawners
            yieldafter = 0;
            foreach (var ns in npcSpawnerHolder.GetComponentsInChildren<NpcSpawner>())
            {
                ns.Initialize(this);
                yieldafter++;
                if (yieldafter%50 == 0)
                {
                    yield return null;
                }
            }

            //foreach (WildlifeSpawner ws in npcSpawnerHolder.GetComponentsInChildren<NpcSpawner>())
            //{
            //TODO: properly setup all wildlife spawners
            //}            

            yieldafter = 0;
            foreach (var ie in interactiveElementHolder.GetComponentsInChildren<InteractiveLevelElement>())
            {
                if (ie.LevelObjectID > -1)
                AddToZone(ie);
                yieldafter++;
                if (yieldafter%50 == 0)
                {
                    yield return null;
                }
            }
            foreach (var dest in destinationsHolder.GetComponentsInChildren<PlayerStart>())
            {
                dest.MapID = _id;
                if (!_destinations.Contains(dest))
                {
                    _destinations.Add(dest);
                }
            }
            foreach (var trigger in destinationsHolder.GetComponentsInChildren<Trigger>())
            {
                if (!_triggers.Contains(trigger))
                {
                    _triggers.Add(trigger);
                }
            }
            if (_script != null)
            {
                _script.OnAfterLoaded();
            }
        }

        /// <summary>
        ///     Teleports <see cref="player" /> to the nearest respawn if one is found
        /// </summary>
        /// <returns>true if teleported successfully</returns>
        public bool TeleportToNearestRespawnLocation(PlayerCharacter playerCharacter)
        {
            var nearest = FindNearestRespawn(playerCharacter.Position);
            if (nearest != null)
            {
                playerCharacter.TeleportTo(nearest.transform.position, nearest.transform.rotation);
                return true;
            }
            Debug.LogWarning(string.Format("Player '{0}' tried to respawn, but no respawnPoint found", playerCharacter));
            return false;
        }

        void OnEnable()
        {
            //Debug.Log("Zone enabled: " + ReadableName);
            if (_zoneGrid != null)
            {
                _zoneGrid.IsPaused = false;
            }
        }

        void OnDisable()
        {
            //Debug.Log("Zone disabled: " + ReadableName);
            if (_zoneGrid != null)
            {
                _zoneGrid.IsPaused = true;
            }
        }

        //void FixedUpdate()
        //{
        //    UpdateZone();
        //}

        public void UpdateZone()
        {
            for (var i = 0; i < _npcs.Count; i++)
            {
                _npcs[i].UpdateEntity();
            }
            for (var i = 0; i < _players.Count; i++)
            {
                _players[i].UpdateEntity();
            }
            if (_script != null)
            {
                _script.Update();
            }
            for (var i = 0; i < _events.Count;i++)
            {
                _events[i].onZoneUpdate();
            }
            for (var i = 0; i < _interactiveElements.Count; i++)
            {
                _interactiveElements[i].UpdateEntity();
            }
        }

        /// <summary>
        ///     Adds <see cref="newPlayer" /> to the zone, registers it to the grid, and updates its
        ///     <see cref="Entity.ActiveZone" />
        /// </summary>
        public bool AddToZone(PlayerCharacter newPlayerCharacter)
        {
            if (_players.Contains(newPlayerCharacter))
            {
                return false;
            }
            if (newPlayerCharacter.ActiveZone != null)
            {
                newPlayerCharacter.ActiveZone.RemoveFromZone(newPlayerCharacter);
            }
            _players.Add(newPlayerCharacter);
            newPlayerCharacter.ActiveZone = this;
            _zoneGrid.Add(newPlayerCharacter);
            _script.OnPlayerEntered(newPlayerCharacter);
            newPlayerCharacter.transform.parent = playerHolder;
            return true;
        }

        /// <summary>
        ///     Adds <see cref="npc" /> to the zone, registers it to the grid and updates its <see cref="Entity.ActiveZone" />.
        ///     Typically called by spawners
        /// </summary>
        public bool AddToZone(NpcCharacter npc)
        {
            if (_npcs.Contains(npc))
            {
                return false;
            }
            if (npc.ActiveZone != null)
            {
                npc.ActiveZone.RemoveFromZone(npc);
            }
            _npcs.Add(npc);
            npc.ActiveZone = this;
            _zoneGrid.Add(npc);
            npc.transform.parent = npcHolder;
            return true;
        }

        /// <summary>
        ///     Adds the ILE to the zone, registers it to the grid and updates its <see cref="Entity.ActiveZone" />
        /// </summary>
        public bool AddToZone(InteractiveLevelElement element)
        {
            if (_interactiveElements.Contains(element))
            {
                return false;
            }
            if (element.ActiveZone != null)
            {
                element.ActiveZone.RemoveFromZone(element);
            }
            _interactiveElements.Add(element);
            element.ActiveZone = this;
            if (!element.isDummy) _zoneGrid.Add(element);   //Real elements added to relevance grid
            _script.OnInteractiveElementAdded(element);
            element.transform.parent = interactiveElementHolder;
            element.AssignRelID();
            return true;
        }

        public bool StartEvent(SBEvent ev)
        {
            _script.OnStartEvent(ev);
            if (ev.eventZone != this) return false;
            _events.Add(ev);
            return true;
        }

        public bool StartScriptedEvent(string eventTag, Entity other, Character instigator)
        {

            //TODO:Retrieve by tag (from GameData?)
            //var scriptedEv = GameData.Get.eventDB.GetScriptedEvent(eventTag);
            //scriptedEv.Trigger(other,instigator)
            //if (scriptedEv.eventZone != this) return false;

            return false;
        }

        public void UntriggerEvent(Entity other, Character instigator)
        {
            foreach (var ev in _events)
            {
                if (ev.other == other && ev.instigator == instigator)
                {
                    ev.UnTrigger();
                    return;
                }
            }
        }

        public void UntriggerEvent(string eventTag)
        {
            foreach (var ev in _events)
            {
                if (ev.EventTag == eventTag)
                {
                    ev.UnTrigger();
                    return;
                }
            }
        }

        /// <summary>
        ///     Removes <see cref="playerCharacter" /> from the zone, its grid and updates <see cref="Entity.ActiveZone" /> to null
        /// </summary>
        public void RemoveFromZone(PlayerCharacter playerCharacter)
        {
            _script.OnPlayerLeaves(playerCharacter);
            playerCharacter.ActiveZone = null;
            _players.Remove(playerCharacter);
            _zoneGrid.Remove(playerCharacter);
            playerCharacter.transform.parent = null;
        }

        /// <summary>
        ///     Removes <see cref="npc" /> from the zone, its grid and updates <see cref="Entity.ActiveZone" /> to null
        /// </summary>
        public void RemoveFromZone(NpcCharacter npc)
        {
            npc.ActiveZone = null;
            _npcs.Remove(npc);
            _zoneGrid.Remove(npc);
            npc.transform.parent = null;
        }

        /// <summary>
        ///     Removes <see cref="element" /> from the zone, its grid and updates <see cref="Entity.ActiveZone" /> to null
        /// </summary>
        public void RemoveFromZone(InteractiveLevelElement element)
        {
            _script.OnInteractiveElementRemoved(element);
            element.ActiveZone = null;
            _interactiveElements.Remove(element);
            if (!element.isDummy) _zoneGrid.Remove(element);
            element.transform.parent = null;
        }

        public void StopEvent(SBEvent ev)
        {
            _script.OnStopEvent(ev);
            _events.Remove(ev);
        }

        public bool StopScriptedEvent(string eventTag)
        {
            //Find zone event with matching tag
            foreach (var sEv in _scriptedEvents)
            {
                if (sEv.EventTag == eventTag)
                {
                    StopEvent(sEv.Stages[sEv._stageInd]);
                    _scriptedEvents.Remove(sEv);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Shortcut to send <see cref="message" /> from <see cref="player" /> to all player characters in the zone (including
        ///     the sender)
        /// </summary>
        public void BroadcastToPlayers(PlayerCharacter playerCharacter, Message message)
        {
            for (var i = _players.Count; i-- > 0;)
            {
                _players[i].ReceiveRelevanceMessage(playerCharacter, message);
            }
        }

        #region ZoneInfo

#pragma warning disable 649
        [SerializeField, ReadOnly] MapIDs _id;

        /// <summary>
        ///     Readable name
        /// </summary>
        [SerializeField, ReadOnly] public string ReadableName;

        /// <summary>
        ///     The package internal object name of the SBWorld instance
        /// </summary>
        [SerializeField, ReadOnly] public string InternalName;

        /// <summary>
        ///     The file that corresponds to this world (.sbw)
        /// </summary>
        [SerializeField, ReadOnly] public string PackageFileName;

        /// <summary>
        ///     The classification of this world
        /// </summary>
        [SerializeField, ReadOnly] public eZoneWorldTypes WorldType;

        /// <summary>
        ///     If this is an instance, how many players are allowed
        /// </summary>
        [SerializeField, ReadOnly] public int InstanceMaxPlayers;

        /// <summary>
        ///     If this is an instance, should it be destroyed, after all players left
        /// </summary>
        [SerializeField, ReadOnly] public bool InstanceAutoDestroy;

        /// <summary>
        ///     The ID of this zone
        /// </summary>
        public MapIDs ID
        {
            get { return _id; }
        }

#pragma warning restore 649

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Spawn from NPCSpawners")]
        public void EditorSpawnNPCsFromSpawners()
        {
            var npcSpawners = npcSpawnerHolder.GetComponentsInChildren<NpcSpawner>();
            for (var i = npcSpawners.Length; i-- > 0;)
            {
                var nps = npcSpawners[i];
                var newNPC = NpcCharacter.Create(nps.npc, nps.transform.position, nps.transform.rotation.eulerAngles);
                newNPC.transform.parent = npcHolder;
                var s = new SpawnInfo();
                s.initialSpawnPoint = nps.transform.position;
                s.initialSpawnRotation = nps.transform.rotation.eulerAngles;
                var wls = nps as WildlifeSpawner;
                if (wls != null)
                {
                    s.spawnerCategory = ESpawnerCategory.Wildlife;
                    s.maxSpawnDistance = wls.MaxSpawnDistance;
                    s.levelMin = wls.LevelMin;
                    s.levelMax = wls.LevelMax;
                }
                else
                {
                    s.maxSpawnDistance = 0;
                    s.levelMin = nps.npc.FameLevel;
                }
                s.respawnInterval = nps.respawnTimeout;
                s.referenceAiStateMachine = nps.referenceAiStatemachineName;
                s.referencelinkedScripts.AddRange(nps.referenceScriptNames);
                if (nps.linkedPatrolPoint != null)
                {
                    s.linkedPatrolPoint = nps.linkedPatrolPoint;
                }
                newNPC.RespawnInfo = s;
                DestroyImmediate(nps.gameObject);
            }
        }

        public void EditorLoadID()
        {
            MapIDs theID;
            if (Helper.EnumTryParse(name, out theID))
            {
                _id = theID;
            }
            else
            {
                Debug.Log("ID could not be parsed: " + name);
            }
        }

        /// <summary>
        ///     currently used to enable batch drawing of editor gizmos for all npcs in the zone. It's faster than giving each one
        ///     its own OnDrawGizmos
        /// </summary>
        public bool drawNpcGizmos;

        void OnDrawGizmos()
        {
            if (!drawNpcGizmos)
            {
                return;
            }
            foreach (Transform np in npcHolder)
            {
                Gizmos.DrawIcon(np.position, "Npc.psd");
            }
        }
#endif

        #region sortingTransforms

        [SerializeField] Transform npcHolder;

        [SerializeField] Transform playerHolder;

        [SerializeField] Transform interactiveElementHolder;
        public Transform InteractiveElementHolder { get { return interactiveElementHolder; } }

        [SerializeField] Transform destinationsHolder;

        [SerializeField] Transform portalHolder;

        [SerializeField] Transform npcSpawnerHolder;

        [SerializeField] Transform triggersHolder;

        #endregion

        #region RetrieverFunctions

        /// <summary>
        ///     Tries to find a <see cref="PlayerStart" /> by its <see cref="destinationTag" />
        /// </summary>
        /// <returns>null if none was found</returns>
        public PlayerStart FindTravelDestination(string destinationTag)
        {
            if (_destinations.Count == 0)
            {
                var t = destinationsHolder.FindChild(destinationTag);
                if (t)
                {
                    return t.GetComponent<PlayerStart>();
                }
            }
            for (var i = 0; i < _destinations.Count; i++)
            {
                if (_destinations[i].NavigationTag.Equals(destinationTag, StringComparison.OrdinalIgnoreCase))
                {
                    return _destinations[i];
                }
            }
            return null;
        }

        /// <summary>
        ///     Tries to find a portal by name (gameObject name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SBWorldPortal FindPortal(string name)
        {
            foreach (Transform portal in portalHolder)
            {
                if (portal.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return portal.GetComponent<SBWorldPortal>();
                }
            }
            return null;
        }

        /// <summary>
        ///     returns the <see cref="PlayerStart" /> that is nearest to <see cref="position" /> in this zone; if one exists,
        ///     otherwise null
        /// </summary>
        public PlayerStart FindNearestRespawn(Vector3 position)
        {
            PlayerStart nearest = null;
            var nearestDist = float.PositiveInfinity;
            for (var i = 0; i < _destinations.Count; i++)
            {
                if (!_destinations[i].IsRespawn)
                {
                    continue;
                }
                var currentDist = Vector3.SqrMagnitude(position - _destinations[i].transform.position);
                if (!(currentDist < nearestDist)) continue;
                nearest = _destinations[i];
                nearestDist = currentDist;
            }
            return nearest;
        }

        /// <summary>
        ///     Tries to find a character (player or npc) by its GameObject in this zone (useful for unity's Physics.Raycast and
        ///     similar)
        /// </summary>
        /// <returns>null if none matched <see cref="obj" /> from this zone</returns>
        public Character FindCharacter(GameObject obj)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].gameObject == obj)
                {
                    return _players[i];
                }
            }
            for (var i = 0; i < _npcs.Count; i++)
            {
                if (_npcs[i].gameObject == obj)
                {
                    return _npcs[i];
                }
            }
            return null;
        }

        /// <summary>
        ///     Tries to find a player character by its name in this zone
        /// </summary>
        /// <returns>null if no match</returns>
        public PlayerCharacter FindPlayerCharacter(string name)
        {
            for (var i = _players.Count; i-- > 0;)
            {
                if (_players[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return _players[i];
                }
            }
            return null;
        }

        public NpcCharacter FindNpcCharacter(string name)
        {
            foreach (var npc in _npcs)
            {
                if (npc.typeRef.LongName.Contains(name)) {
                    return npc;
                }
            }
            return null;
        }

        public NpcCharacter FindNpcCharacter(string name, int instanceIndex)
        {
            int count = 0;
            foreach (var npc in _npcs)
            {
                if (npc.typeRef.LongName.Contains(name))
                {
                    if (count == instanceIndex) return npc;
                    else count++;
                }
            }
            return null;
        }

        /// <summary>
        ///     Tries to find an npc by it's relevanceID in this zone
        /// </summary>
        /// <returns>null if no match</returns>
        public NpcCharacter GetNpc(int relID)
        {
            for (var i = _npcs.Count; i-- > 0;)
            {
                if (_npcs[i].RelevanceID == relID)
                {
                    return _npcs[i];
                }
            }
            return null;
        }

        public InteractiveLevelElement GetILE(int relID)
        {
            for (var i = _interactiveElements.Count; i-- > 0;)
            {
                if (_interactiveElements[i].RelevanceID == relID)
                {
                    return _interactiveElements[i];
                }
            }
            return null;
        }

        public InteractiveLevelElement GetChair(int levelObjectID)
        {
            if (levelObjectID < 0) return null;
            foreach (var ile in _interactiveElements)
            {
                if (    ile.ileType == EILECategory.ILE_Chair
                    &&  ile.LevelObjectID == levelObjectID)
                {
                    return ile;
                }
            }
            return null;
        }

        #endregion

        #region Physics

        [SerializeField] List<Collider> collisionGeometry;

        /// <summary>
        ///     Reports if this zone has colliders assigned TODO still open to find a suitable method to extract these
        /// </summary>
        public bool HasCollision
        {
            get { return collisionGeometry.Count > 0; }
        }

        /// <summary>
        ///     Fires a ray agains the colliders of this zone
        /// </summary>
        /// <param name="origin">the source position of the ray</param>
        /// <param name="direction">the direction of the ray</param>
        /// <param name="length">the length of the ray</param>
        /// <param name="hitPosition">is set to the point the ray hit a collider or origin of no hit</param>
        /// <returns>true if a collider was hit</returns>
        public bool Raycast(Vector3 origin, Vector3 direction, float length, out Vector3 hitPosition)
        {
            for (var i = 0; i < collisionGeometry.Count; i++)
            {
                var c = collisionGeometry[i];
                var r = new Ray(origin, direction);
                RaycastHit hit;
                if (!c.Raycast(r, out hit, length)) continue;
                hitPosition = hit.point;
                return true;
            }
            hitPosition = origin;
            return false;
        }

        /// <summary>
        ///     Fires a ray agains the colliders in this zone and returns <see cref="origin" /> if no collider was hit
        /// </summary>
        public Vector3 Raycast(Vector3 origin, Vector3 direction, float length)
        {
            for (var i = 0; i < collisionGeometry.Count; i++)
            {
                var c = collisionGeometry[i];
                var r = new Ray(origin, direction);
                RaycastHit hit;
                if (c.Raycast(r, out hit, length))
                {
                    return hit.point;
                }
            }

            return origin;
        }

        #endregion

        [ReadOnly]
        public float killY;    //Y-coordinate below with players are killed

    }
}