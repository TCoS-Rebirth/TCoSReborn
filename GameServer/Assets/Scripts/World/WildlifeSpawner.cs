using Common;
using Database.Static;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using UnityEngine;

namespace World
{
    public class WildlifeSpawner : NpcSpawner
    {
        public int LevelMax;
        public int LevelMin;
        public bool LoSSpawning = true;
        public float MaxSpawnDistance;
        public float RespawnTime;
        public float RespawnVariation;
        public int SpawnMax;
        public int SpawnMin;
        public static float DefaultThreatRange = 5.0f;
        public float ThreatRange;
        public bool UseAbsoluteAmounts;
        public float VisualRange;

        public bool respawnPending;

        //Valshaaran - experimental factor by which to multiple spawnmin, spawnmax, so the numbers are more reasonable!
        const float spawnNumMult = 0.2f;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawWireSphere(transform.position, MaxSpawnDistance);
        }

        public override void TriggerSpawn(Zone z)
        {
            int spawnNum;
            
            if (SpawnMax > 1)
            {
                spawnNum = (int)(spawnNumMult * Random.Range(SpawnMin, SpawnMax));
                if (spawnNum < 1) spawnNum = 1;
            }
            else spawnNum = 1;

            while (spawns.Count < spawnNum)
            {
            //Random position
                var deployX = transform.position.x;
                var deployZ = transform.position.z;
                var deployOffset = MaxSpawnDistance*Random.insideUnitCircle;
                deployX += deployOffset.x;
                deployZ += deployOffset.y;
                var deployPos = new Vector3(deployX, transform.position.y, deployZ);
                var rayCast = zone.Raycast(deployPos, Vector3.down, 20f);

                //Random rotation
                var randRotY = Random.Range(-180.0f, 180.0f);
                var rndRot = new Vector3(0, randRotY, 0);

                var newSI = new SpawnInfo();
                newSI.setupFromSpawner(this, rayCast, rndRot);

            

                var newNPC = NpcCharacter.Create(npc, rayCast, rndRot, newSI);

                if (newNPC != null)
                {
                    //Set to default faction if typeref faction was null
                    if (newNPC.Faction == null)
                        newNPC.Faction = GameData.Get.factionDB.defaultFaction;
                    //Roll level if not specified
                    if (newNPC.FameLevel == 0)
                        newNPC.FameLevel = Random.Range(LevelMin, LevelMax);

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
        }

        void OnTriggerEnter(Collider col)
        {
            var ch = col.GetComponent<Character>();
            if (ch && ch.ActiveZone == zone)
            {
                Spawn();
            }
        }

        //TODO : would be more efficient to have an event called when an npc in the spawner is killed
        //Spawn timing
        void FixedUpdate()
        {
            if (!initialized || triggeredSpawn || (RespawnTime <= 0)) return;

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
            else if (liveSpawns() == 0 || liveSpawns() < (int)(spawnNumMult * SpawnMin))
            {
                respawnPending = true;
                respawnTimer = RespawnTime * Random.Range(1.0f - RespawnVariation, 1.0f + RespawnVariation);
            }
        }

        //public override void TriggerSpawn(Zone z)
        //{
        //    if (spawns.Count < SpawnMax)
        //    {
        //        GameObject go = new GameObject(npc.ShortName);
        //        NpcCharacter npcInstance = go.AddComponent<NpcCharacter>();
        //        Vector3 pos = new Vector3(Random.Range(-MaxSpawnDistance, MaxSpawnDistance), 0, Random.Range(-MaxSpawnDistance, MaxSpawnDistance));
        //        go.transform.position = AstarPath.active.GetNearest(transform.position + pos).clampedPosition;
        //        go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
        //        npcInstance.LoadSkillDeck(npc.SkillDeck);
        //        npcInstance.Initialize();
        //        WildlifeBehaviour wb = go.AddComponent<WildlifeBehaviour>();//add correct behaviours later, for example based on internalType of npc_type and/or overridden (TODO)
        //        wb.aggroRadius = ThreatRange;

        //        npcInstance.Name = npc.ShortName;
        //        npcInstance.name = npc.ShortName;
        //        int hp = 100;
        //        int levelHP = 100;
        //        int selectedLevel = Random.Range(LevelMin, LevelMax);
        //        npc.InitializeStats(selectedLevel, npc.PePRank, out hp, out levelHP, out npcInstance.Body, out npcInstance.Mind, out npcInstance.Focus);
        //        npcInstance.MaxHealth = Mathf.Max(hp, levelHP, npc.FameLevel * 10, 100);
        //        npcInstance.Health = npcInstance.MaxHealth;
        //        npcInstance.FameLevel = selectedLevel;
        //        npcInstance.PepRank = npc.PePRank;
        //        npcInstance.Faction = npc.TaxonomyFaction;
        //        npcInstance.PawnState = EPawnStates.PS_ALIVE;
        //        if (z.AddToZone(npcInstance))
        //        {
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