using System;
using System.Collections.Generic;
using UnityEngine;
using World;
using World.Paths;

namespace Gameplay.Entities.NPCs
{
    public enum ESpawnerCategory
    {
        Default = 0,
        Wildlife = 1,
        Deployer = 2
    }

    [Serializable]
    public class SpawnInfo
    {
        public Vector3 initialSpawnPoint;
        public Vector3 initialSpawnRotation;
        public int levelMax;

        [Header("Level")] public int levelMin;

        public PatrolPoint linkedPatrolPoint;
        public float maxSpawnDistance;
        public string referenceAiStateMachine;
        public List<string> referencelinkedScripts = new List<string>();
        public float respawnInterval;
        public ESpawnerCategory spawnerCategory;

        [Header("Runtime"), ReadOnly] public float timeOfDespawn;

        public NPC_Type typeRef;

        public void setupFromSpawner(NpcSpawner spawner)
        {
            initialSpawnPoint = spawner.transform.position;
            initialSpawnRotation = spawner.transform.rotation.eulerAngles;

            linkedPatrolPoint = spawner.linkedPatrolPoint;
            referenceAiStateMachine = spawner.referenceAiStatemachineName;
            referencelinkedScripts = spawner.referenceScriptNames;
            //respawnInterval?
            typeRef = spawner.npc;
            timeOfDespawn = spawner.respawnTimeout; //Valshaaran - not sure?

        }

        public void setupFromSpawner(WildlifeSpawner spawner)
        {
            setupFromSpawner((NpcSpawner) spawner);

            levelMax = spawner.LevelMax;
            levelMin = spawner.LevelMin;
            maxSpawnDistance = spawner.MaxSpawnDistance;
            respawnInterval = spawner.RespawnTime;
            spawnerCategory = ESpawnerCategory.Wildlife;            
            
        }

        //TODO: setup from spawn deployer?
    }
}