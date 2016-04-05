using System.Collections.Generic;
using Database.Static;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using UnityEngine;
using World.Paths;
using Common;

namespace World
{
    public class NpcSpawner : MonoBehaviour
    {
        public float aggressionFactor;
        public float boredomFactor;
        public float fearFactor;
        public List<string> groups = new List<string>();

        protected bool initialized;

        public PatrolPoint linkedPatrolPoint;

        public NPC_Type npc;

        public string referenceAiStatemachineName;

        public string referenceEventOnSpawnName;
        public string referenceEventOnWipedName;
        public List<string> referenceScriptNames = new List<string>();

        public float respawnTimeout;

        [ReadOnly] public float respawnTimer;

        public float socialFactor;

        [ReadOnly] public List<NpcCharacter> spawns = new List<NpcCharacter>();

        public bool triggeredDespawn;
        public bool triggeredRespawn;
        public bool triggeredSpawn;

        public bool respawnPending;

        protected Zone zone;

        public virtual void Initialize(Zone z)
        {
            if (initialized)
            {
                return;
            }
            zone = z;
            Spawn();
            initialized = true;
        }

        public virtual void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                return;
            }
            Gizmos.DrawIcon(transform.position, "NpcSpawn.psd");
        }

        public virtual void Spawn()
        {
            if (triggeredSpawn)
            {
                return;
            }

            //Valshaaran : temp hack to not spawn anything with position 0,0,0
            if (    transform.position.x == 0.0
                && transform.position.y == 0.0
                && transform.position.z == 0.0)
            {
                triggeredSpawn = true;
                return;
            }

            TriggerSpawn(zone);
        }

        public virtual void TriggerSpawn(Zone z)
        {
            if (spawns.Count == 0)
            {
                var newSI = new SpawnInfo();
                newSI.setupFromSpawner(this,transform.position,Quaternion.ToEulerAngles(transform.rotation));

                //Valshaaran - experimental raycast spawning to address mid-air ConditionalEnemy spawns
                //var rayCast = zone.Raycast(transform.position, Vector3.down, 20f);

                var newNPC = NpcCharacter.Create(npc, transform.position, transform.rotation.eulerAngles, newSI);
                if (newNPC == null)
                {
                    Debug.LogWarning(name + " null npcType / " + z);
                    return;
                }
                //Set to default faction if typeref faction was null
                if (newNPC.Faction == null)
                    newNPC.Faction = GameData.Get.factionDB.defaultFaction;

                newNPC.InitEnabled = true;
                newNPC.InitColl = ECollisionType.COL_Colliding;

                if (z.AddToZone(newNPC))
                {
                    spawns.Add(newNPC);
                }
                else
                {
                    Debug.LogWarning("NPC could not be added to zone: " + npc.ShortName + " / " + z.ReadableName + ", removing");
                }
            }
        }

        [ContextMenu("Despawn")]
        public virtual void DespawnAll(Zone z)
        {
            foreach (var spawn in spawns)
            {
                z.RemoveFromZone(spawn);
                Destroy(spawn.gameObject);
            }
            spawns.Clear();
        }

        public virtual void ClearDead(Zone z)
        {
            for (int n = 0; n < spawns.Count; n++)
            {
                var spawn = spawns[n];
                if (spawn.PawnState == EPawnStates.PS_DEAD)
                {
                    z.RemoveFromZone(spawn);
                    Destroy(spawn.gameObject);
                    spawns.RemoveAt(n);
                    n--;
                }
            }
        }

        public int liveSpawns ()
        {
            int output = 0;
            foreach (var spawn in spawns)
            {
                if (spawn.PawnState == EPawnStates.PS_ALIVE) output++;
            }
            return output;
        }

        void FixedUpdate()
        {
            if (!initialized || triggeredSpawn) return;

            if (respawnPending)
            {
                if (respawnTimer <= 0)
                {
                    ClearDead(zone);
                    Spawn();
                    respawnPending = false;
                }
                else { respawnTimer -= Time.deltaTime; }
            }
            else if (liveSpawns() == 0)
            {
                respawnPending = true;
                respawnTimer = respawnTimeout;
            }
        }

        //        {
        //        if (z.AddToZone(npcInstance))
        //        npcInstance.PawnState = EPawnStates.PS_ALIVE;
        //        npcInstance.Faction = npc.TaxonomyFaction;
        //        npcInstance.PepRank = npc.PePRank;
        //        npcInstance.FameLevel = npc.FameLevel;
        //        npcInstance.Health = npcInstance.MaxHealth;
        //        npcInstance.MaxHealth = Mathf.Max(hp, levelHP, npc.FameLevel*10, 100);
        //        npc.InitializeStats(npc.FameLevel, npc.PePRank, out hp, out levelHP, out npcInstance.Body, out npcInstance.Mind, out npcInstance.Focus);
        //        int levelHP = 100;
        //        int hp = 100;
        //        npcInstance.name = npc.ShortName;
        //        npcInstance.Name = npc.ShortName;
        //        npcInstance.Initialize(); //add correct behaviours later, for example based on internalType of npc_type and/or overridden (TODO)
        //        npcInstance.LoadSkillDeck(npc.SkillDeck);
        //        go.transform.rotation = transform.rotation;
        //        go.transform.position = transform.position;
        //        NpcCharacter npcInstance = go.AddComponent<NpcCharacter>();
        //        GameObject go = new GameObject(npc.ShortName);
        //    {
        //    if (spawns.Count == 0)
        //{

        //public virtual void TriggerSpawn(Zone z)
        //            spawns.Add(npcInstance);
        //        }
        //        else
        //        {
        //            Debug.LogWarning("NPC could not be added to zone: " + npc.ShortName + " / " + z.Name + ", removing");
        //            Destroy(go);
        //        }
        //    }
        //}
    }
}