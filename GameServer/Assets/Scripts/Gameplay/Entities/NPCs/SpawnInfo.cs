using System;
using System.Collections.Generic;
using UnityEngine;
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
    }
}