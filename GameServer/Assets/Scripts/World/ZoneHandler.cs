using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common;
using Gameplay.Entities;
using Network;
using UnityEngine;

namespace World
{
    public class ZoneHandler : MonoBehaviour
    {
        [SerializeField] Zone _defaultZone;

        [SerializeField] List<Zone> _zones = new List<Zone>();

        bool _zonesLoaded;

        //bool _zonesLoaded;

        /// <summary>
        ///     List of all registered zones
        /// </summary>
        public ReadOnlyCollection<Zone> Zones
        {
            get { return _zones.AsReadOnly(); }
        }

        /// <summary>
        ///     the default zone used by <see cref="GetZoneOrDefault" />
        /// </summary>
        public Zone DefaultZone
        {
            get { return _defaultZone; }
        }

        /// <summary>
        ///     Initalizes/Loads all available zones over several frames to prevent the application from freezing and invokes
        ///     <see cref="callback" /> after all zones were loaded
        /// </summary>
        public void Initialize(Action callback)
        {
            StartCoroutine(LoadZonesAsync(callback));
        }

        IEnumerator LoadZonesAsync(Action finishedCallback)
        {
            Debug.Log("Loading Zones");
            for (var i = _zones.Count; i-- > 0;)
            {
                if (!_zones[i].gameObject.activeInHierarchy)
                {
                    _zones.RemoveAt(i);
                    continue;
                }
                if (_zones[i].IsEnabled)
                {
                    yield return StartCoroutine(_zones[i].Load());
                }
                yield return null;
            }
            for (var i = 0; i < _zones.Count; i++)
            {
                if (_zones[i].IsEnabled)
                {
                    _zones[i].enabled = true;
                }
            }
            finishedCallback();
            _zonesLoaded = true;
        }

        void FixedUpdate()
        {
            if (!_zonesLoaded)
            {
                return;
            }
            for (var i = 0; i < _zones.Count; i++)
            {
                if (!_zones[i].IsEnabled || _zones[i].PlayerCount == 0)
                {
                    continue;
                }
                _zones[i].UpdateZone();
            }
        }

        public void ShutDown()
        {
            for (var i = 0; i < _zones.Count; i++)
            {
                _zones[i].ShutDown();
            }
            _zonesLoaded = false;
        }

        /// <summary>
        ///     Tries to find a <see cref="PlayerStart" /> in all zones by <see cref="destinationTag" />
        /// </summary>
        /// <returns>null if none matched</returns>
        public PlayerStart FindTravelDestination(string destinationTag)
        {
            for (var i = 0; i < _zones.Count; i++)
            {
                var dest = _zones[i].FindTravelDestination(destinationTag);
                if (dest != null)
                {
                    return dest;
                }
            }
            return null;
        }

        /// <summary>
        ///     Returns the first zone matching <see cref="mapID" /> or null if no match
        /// </summary>
        public Zone GetZone(MapIDs mapID, bool onlyReturnEnabled = false)
        {
            for (var i = 0; i < _zones.Count; i++)
            {
                if (_zones[i].ID == mapID)
                {
                    if (!onlyReturnEnabled || _zones[i].IsEnabled)
                    {
                        return _zones[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Returns the first zone matching <see cref="desiredID" /> or <see cref="DefaultZone" /> if no match
        /// </summary>
        public Zone GetZoneOrDefault(MapIDs desiredID)
        {
            var desired = GetZone(desiredID);
            if (desired != null)
            {
                return desired;
            }
            return _defaultZone;
        }

        /// <summary>
        ///     removes <see cref="player" /> from its former zone, calls <see cref="PlayerInfo.LoadClientMap" /> and moves it to
        ///     <see cref="destination" />
        /// </summary>
        public void TravelPlayer(PlayerCharacter playerCharacter, PlayerStart destination)
        {
            if (playerCharacter.ActiveZone != null)
            {
                playerCharacter.ActiveZone.RemoveFromZone(playerCharacter);
            }
            playerCharacter.Owner.LoadClientMap(destination.MapID);
            playerCharacter.transform.position = destination.transform.position;
            playerCharacter.transform.rotation = destination.transform.rotation;
        }

        /// <summary>
        ///     Inserts the player into a zone given by <see cref="mapID" /> or <see cref="DefaultZone" /> if
        ///     <see cref="useDefaultOnMissing" /> is true.
        ///     If <see cref="useDefaultOnMissing" /> is false, and no zone was found, no action is taken
        /// </summary>
        public bool InsertPlayer(PlayerCharacter playerCharacter, MapIDs mapID, bool useDefaultOnMissing)
        {
            if (useDefaultOnMissing)
            {
                var z = GetZoneOrDefault(mapID);
                if (z != null)
                {
                    return z.AddToZone(playerCharacter);
                }
                return false;
            }
            else
            {
                var z = GetZone(mapID);
                if (z != null)
                {
                    return z.AddToZone(playerCharacter);
                }
                return false;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Spawn All Spawners")]
        void EditorSpawnNPCS()
        {
            foreach (var z in _zones)
            {
                z.EditorSpawnNPCsFromSpawners();
            }
        }

        [ContextMenu("Load Zone ids")]
        void EditorLoadZoneIDs()
        {
            foreach (var z in _zones)
            {
                z.EditorLoadID();
            }
        }

        [ContextMenu("Delete all npcs")]
        void EditorDeleteAllNPCs()
        {
            foreach (var z in _zones)
            {
                var NPCs = z.transform.FindChild("Npcs");
                var ts = NPCs.GetComponentsInChildren<NpcCharacter>();
                for (var i = ts.Length; i-- > 0;)
                {
                    DestroyImmediate(ts[i].gameObject);
                }
            }
        }

        [ContextMenu("Load Zones")]
        void EditorLoadZones()
        {
            _zones.Clear();
            _zones.AddRange(GetComponentsInChildren<Zone>());
        }

        [ContextMenu("Auto adjust PortalColliders")]
        void EditorAutoAdjustPortalColliders()
        {
            foreach (var p in FindObjectsOfType<SBWorldPortal>())
            {
                var bc = p.GetComponent<BoxCollider>();
                if (!bc) continue;
                bc.size = new Vector3(p.collisionRadius, p.collisionHeight, p.collisionRadius);
                bc.center = Vector3.zero;
            }
        }

        [ContextMenu("Clear ALL Spawners and Deployers!")]
        void EditorClearSpawners()
        {
            foreach (var z in _zones)
            {
                var spawnersObj = z.transform.FindChild("Spawners");
                var spawners = spawnersObj.GetComponentsInChildren<NpcSpawner>();
                var deployers = spawnersObj.GetComponentsInChildren<SpawnDeployer>();
                for (var i = spawners.Length; i-- > 0;)
                {
                    DestroyImmediate(spawners[i].gameObject);
                }
                for (var i = deployers.Length; i-- > 0;)
                {
                    DestroyImmediate(deployers[i].gameObject);
                }
            }
        }
#endif

        #region Helper

        /// <summary>
        ///     Returns the population of player characters in all zones
        /// </summary>
        public int GetAllZonePlayerPopulation()
        {
            var count = 0;
            for (var i = 0; i < _zones.Count; i++)
            {
                count += _zones[i].Players.Count;
            }
            return count;
        }

        /// <summary>
        ///     Tries to find a player character by its name
        /// </summary>
        /// <returns>null if none found</returns>
        public PlayerCharacter FindPlayerCharacter(string characterName)
        {
            for (var i = 0; i < _zones.Count; i++)
            {
                var found = _zones[i].FindPlayerCharacter(characterName);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        ///     Broadcasts <see cref="message" /> from <see cref="player" /> to all entities in all zones (including the sender)
        /// </summary>
        public void BroadCastToAllZones(PlayerCharacter playerCharacter, Message message)
        {
            for (var i = 0; i < _zones.Count; i++)
            {
                _zones[i].BroadcastToPlayers(playerCharacter, message);
            }
        }

        #endregion
    }
}