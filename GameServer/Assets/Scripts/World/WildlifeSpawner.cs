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
        public float ThreatRange;
        public bool UseAbsoluteAmounts;
        public float VisualRange;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawWireSphere(transform.position, MaxSpawnDistance);
        }

        public override void TriggerSpawn(Zone z)
        {
            //int spawnNum = Random.Range(SpawnMin, SpawnMax);

            //while (spawns.Count < spawnNum)
            //{
            //Random position
            var deployX = transform.position.x;
            var deployZ = transform.position.z;
            var deployOffset = MaxSpawnDistance*Random.insideUnitCircle;
            deployX += deployOffset.x;
            deployZ += deployOffset.y;

            //Random rotation
            var randRotY = Random.Range(-180.0f, 180.0f);
            var rndRot = new Vector3(0, randRotY, 0);

            var newSI = new SpawnInfo();
            newSI.initialSpawnPoint = new Vector3(deployX, transform.position.y, deployZ);
            newSI.initialSpawnRotation = rndRot;
            //newSI.linkedPatrolPoint = linkedPatrolPoint; //Wildlife spawners don't ever have patrol points?
            newSI.respawnInterval = respawnTimeout;
            newSI.typeRef = npc;
            newSI.spawnerCategory = ESpawnerCategory.Wildlife;
            // si.timeOfDespawn

            var newNPC = NpcCharacter.Create(npc, transform.position, transform.rotation.eulerAngles, newSI);

            if (newNPC != null)
            {
                //Set to default faction if typeref faction was null
                if (newNPC.Faction == null)
                    newNPC.Faction = GameData.Get.factionDB.defaultFaction;
                //Roll level if not specified
                if (newNPC.FameLevel == 0)
                    newNPC.FameLevel = Random.Range(LevelMin, LevelMax);

                if (z.AddToZone(newNPC))
                {
                    spawns.Add(newNPC);
                }
                else
                {
                    Debug.LogWarning("NPC could not be added to zone: " + npc.ShortName + " / " + z.ReadableName + ", removing");
                }
            }
            //}
        }

        void OnTriggerEnter(Collider col)
        {
            var ch = col.GetComponent<Character>();
            if (ch && ch.ActiveZone == zone)
            {
                Spawn();
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