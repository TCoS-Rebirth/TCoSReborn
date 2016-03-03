using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;
using UnityEngine;
using World.Paths;
using Random = UnityEngine.Random;

namespace World
{
    public enum EDeployerState
    {
        Disabled = 0, //See SpawnDeployer state diagram in documentation                
        Deployed = 1,
        Combat = 2,
        Needs_Redeploy = 3
    }

    public enum EDeployerCommand
    {
        Disable = 0,
        Enable = 1,
        Enter_Combat = 2,
        Leave_Combat_Intact = 3,
        Leave_Combat_Casualties = 4,
        Repopulate = 5
    }


    [Serializable]
    public class SpawnDeployer : MonoBehaviour
    {
        public const float defaultSpawnDist = 5.0f;

        public const float defaultSpawnTO = 300.0f;
        //Placeholder to assign primary combat modes to NPCs by their class type
        //TODO:Move to appropriate class or remove when superceded
        public static List<EContentClass> meleeClasses = new List<EContentClass>
        {
            EContentClass.ECC_Assassin,
            EContentClass.ECC_Bloodwarrior,
            EContentClass.ECC_Bodyguard,
            EContentClass.ECC_FrontMan,
            EContentClass.ECC_FuryHammer,
            EContentClass.ECC_MartialArtist,
            EContentClass.ECC_Rogue,
            EContentClass.ECC_Warrior,
            EContentClass.ECC_WrathGuard,
            EContentClass.ECC_Flagellant
        };

        public static List<EContentClass> rangedClasses = new List<EContentClass>
        {
            EContentClass.ECC_Alchemist,
            EContentClass.ECC_AntiMage,
            EContentClass.ECC_Entertainer,
            EContentClass.ECC_Gadgeteer,
            EContentClass.ECC_Trickster,
            EContentClass.ECC_DeathHand
        };

        public static List<EContentClass> casterClasses = new List<EContentClass>
        {
            EContentClass.ECC_AncestralMage,
            EContentClass.ECC_Consumer,
            EContentClass.ECC_DeathHand,
            EContentClass.ECC_Infuser,
            EContentClass.ECC_Nuker,
            EContentClass.ECC_Priest,
            EContentClass.ECC_RuneMage,
            EContentClass.ECC_RuneMaster,
            EContentClass.ECC_ShapeChanger,
            EContentClass.ECC_Spellcaster,
            EContentClass.ECC_Summoner,
            EContentClass.ECC_Visionary,
            EContentClass.ECC_VoidSeer
        };

        public List<NPC_Type> bosses = new List<NPC_Type>();

        //public List<ActorGroup> actorGroups; //TODO: Can see in zone package, but purpose unknown

        [SerializeField] EDeployerState curState = EDeployerState.Disabled;

        public List<NpcCharacter> dead = new List<NpcCharacter>();


        public List<NpcCharacter> deployed = new List<NpcCharacter>();

        //[SerializeField]
        //private int factionID;
        [SerializeField] public Taxonomy Faction;

        public PatrolPoint linkedPatrolPoint;

        public bool
            LoSSpawning,
            spawnImmediately,
            triggeredSpawn;

        //{   get { return GameData.Get.factionDB.GetFaction(factionID); }
        //                            set { factionID = GameData.Get.factionDB.getFacID(value); }
        //}

        public int minLevel, maxLevel;
        public NPCGroupClass_Type npcGroupClass;
        public string referenceAiStateMachine;

        public List<string> referenceLinkedScripts = new List<string>();

        [SerializeField] public bool respawnPending;

        public float respawnTimeout;

        public float respawnVariation,
            maxSpawnDistance,
            chaseRange,
            visualRange,
            threatRange,
            losRange;

        [SerializeField] float timeToRespawn;

        Dictionary<DeployerStateTransition, EDeployerState> transitions = new Dictionary<DeployerStateTransition, EDeployerState>
        {
            {new DeployerStateTransition(EDeployerState.Disabled, EDeployerCommand.Enable), EDeployerState.Needs_Redeploy},
            {new DeployerStateTransition(EDeployerState.Deployed, EDeployerCommand.Disable), EDeployerState.Disabled},
            {new DeployerStateTransition(EDeployerState.Deployed, EDeployerCommand.Enter_Combat), EDeployerState.Combat},
            {new DeployerStateTransition(EDeployerState.Combat, EDeployerCommand.Leave_Combat_Intact), EDeployerState.Deployed},
            {new DeployerStateTransition(EDeployerState.Combat, EDeployerCommand.Leave_Combat_Casualties), EDeployerState.Needs_Redeploy},
            {new DeployerStateTransition(EDeployerState.Combat, EDeployerCommand.Disable), EDeployerState.Disabled},
            {new DeployerStateTransition(EDeployerState.Needs_Redeploy, EDeployerCommand.Repopulate), EDeployerState.Deployed},
            {new DeployerStateTransition(EDeployerState.Needs_Redeploy, EDeployerCommand.Enter_Combat), EDeployerState.Combat},
            {new DeployerStateTransition(EDeployerState.Needs_Redeploy, EDeployerCommand.Disable), EDeployerState.Disabled}
        };

        public Zone zone;

        public EDeployerState getNextState(EDeployerCommand com)
        {
            var transition = new DeployerStateTransition(curState, com);
            EDeployerState nextState;
            if (!transitions.TryGetValue(transition, out nextState))
                Debug.Log("SpawnDeployer : Invalid state transition " + curState + "->" + com);
            return nextState;
        }

        public EDeployerState moveNextState(EDeployerCommand com)
        {
            curState = getNextState(com);
            return curState;
        }

        void FixedUpdate()
        {
            switch (curState)
            {
                case EDeployerState.Disabled:
                    //Disable command only called externally
                    //TODO: If deployed isn't empty, empty it
                    if (respawnPending)
                    {
                        for (var i = 0; i < deployed.Count; i++)
                        {
                            zone.RemoveFromZone(deployed[i]);
                        }
                        for (var i = 0; i < dead.Count; i++)
                        {
                            zone.RemoveFromZone(dead[i]);
                        }
                        deployed.Clear();
                        dead.Clear();
                        respawnPending = false;
                    }
                    break;

                case EDeployerState.Deployed:

                    if (isCombatTriggered())
                    {
                        onEnterCombat();
                        moveNextState(EDeployerCommand.Enter_Combat);
                        Debug.Log("SpawnDeployer : entered combat");
                    }
                    break;

                case EDeployerState.Combat:

                    //TODO: If distance from spawn > chaseRange
                    //If deployed.Count >= getMinDeployed()
                    //moveNextState(EDeployerCommand.Leave_Combat_Intact);
                    //Reset positions
                    //Else
                    //moveNextState(EDeployerCommand.Leave_Combat_Casualties);
                    //Reset positions of any remaining NPCs

                    //Hack: for unit testing, leave combat and redeploy if deployed < minDeployed()
                    //otherwise leave combat 'intact'

                    var hasInjured = false;
                    for (var n = 0; n < deployed.Count; n++)
                    {
                        var npc = deployed[n];
                        if (npc.Health <= 0)
                        {
                            dead.Add(npc);
                            deployed.Remove(npc);
                            n--;
                        }
                        else if (npc.Health < npc.MaxHealth)
                        {
                            hasInjured = true;
                        }
                    }

                    if (!hasInjured)
                    {
                        onLeaveCombat();
                        if ((deployed.Count < minDeployed())
                            || (npcGroupClass.isBoss && dead.Count > 0))
                        {
                            moveNextState(EDeployerCommand.Leave_Combat_Casualties);
                            Debug.Log("SpawnDeployer : left combat with casualties");
                        }
                        else
                        {
                            moveNextState(EDeployerCommand.Leave_Combat_Intact);
                            Debug.Log("SpawnDeployer : left combat 'intact'");
                        }
                    }

                    break;

                case EDeployerState.Needs_Redeploy:

                    if (isCombatTriggered())
                    {
                        onEnterCombat();
                        moveNextState(EDeployerCommand.Enter_Combat);
                        Debug.Log("SpawnDeployer : entered combat");
                    }

                    //TODO: If respawn timer inactive, activate
                    if (!respawnPending)
                    {
                        respawnPending = true;
                        if (respawnTimeout == 0)
                        {
                            respawnTimeout = defaultSpawnTO;
                        }
                        timeToRespawn = respawnTimeout*Random.Range(1.0f - respawnVariation, 1.0f + respawnVariation);
                    }
                    else //Else if respawn timer expired
                    {
                        if (timeToRespawn <= 0)
                        {
                            respawnPending = false;
                            doRedeploy();
                            moveNextState(EDeployerCommand.Repopulate);
                            //Debug.Log("SpawnDeployer : deploying");
                        }
                        else
                        {
                            timeToRespawn -= Time.deltaTime;
                        }
                    }
                    break;
            }
        }

        int minDeployed()
        {
            if (npcGroupClass.isBoss)
                return 0;
            var cmlTot = 0;
            for (var i = 0; i < npcGroupClass.units.Count; i++)
            {
                var u = npcGroupClass.units[i];
                cmlTot += u.min;
            }
            return cmlTot;
        }

        int maxDeployed()
        {
            if (npcGroupClass.isBoss)
                return 0;
            var cmlTot = 0;
            for (var i = 0; i < npcGroupClass.units.Count; i++)
            {
                var u = npcGroupClass.units[i];
                cmlTot += u.max;
            }
            return cmlTot;
        }

        List<NPC_Type> newDeployeesFinal(NPCGroupClassUnit targetUnit)
        {
            //TODO: cache as much static data as possible
            //e.g.lookup tables returning NPC_Type options for given faction
            //instead of calculating at runtime

            var output = new List<NPC_Type>();
            var curDeployed = 0;

            //return empty if no valid faction
            if (Faction == null)
            {
                Debug.Log("SpawnDeployer : No valid faction, cancelling deployment");
                return output;
            }

            for (var d = 0; d < deployed.Count; d++)
            {
                var member = deployed[d];
                if (!bosses.Contains(member.typeRef))
                {
                    //exclude live bosses from count
                    var countedDeployed = false; //flag so we only count a member once if they match
                    for (var i = 0; i < targetUnit.ReqClassTypes.Count; i++)
                    {
                        var type = targetUnit.ReqClassTypes[i];
                        if (member.typeRef.ClassTypes.Contains(type))
                        {
                            if (!countedDeployed)
                            {
                                curDeployed++;
                                countedDeployed = true;
                            }
                        }
                        //reqClasses.Add(type);
                    }
                }
            }


            //Get min and max new deployees
            var maxDeployees = targetUnit.max - curDeployed;
            //but if max <= 0 we can return an empty list right now!
            if (maxDeployees <= 0)
            {
                Debug.Log("SpawnDeployer : called newDeployees() but the unit is full!");
                return output;
            }

            var minDeployees = targetUnit.min - curDeployed;


            //Roll the number of new deployees
            var numDeployees = Random.Range(minDeployees, maxDeployees);
            //Debug.Log("SpawnDeployer : rolled " + numDeployees
            //            + "new members (" + minDeployees +
            //            "-" + maxDeployees + ")");


            var npcTypeCandidates = new NPCTList();

            //Filter and add faction NPCs to candidates
            if (GameData.Get.factionDB.lookupTables.factionMembers.ContainsKey(Faction.ID))
            {
                for (var i = 0; i < GameData.Get.factionDB.lookupTables.factionMembers[Faction.ID].Values.Count; i++)
                {
                    var npct = GameData.Get.factionDB.lookupTables.factionMembers[Faction.ID].Values[i];
                    if (isClassTypeMatch(npct, targetUnit))
                    {
                        npcTypeCandidates.Values.Add(npct);
                    }
                }
            }


            //Filter and add contents of parent's class package to candidates
            //If parent class package is null, recurse to next parent

            var ancestorClassPackage = getAncestorClassPackage(Faction);
            if (ancestorClassPackage != null) //null means failed to find parent with non-null class package property
            {
                for (var i = 0; i < getAncestorClassPackage(Faction).types.Count; i++)
                {
                    var npct = getAncestorClassPackage(Faction).types[i];
                    if (isClassTypeMatch(npct, targetUnit))
                    {
                        npcTypeCandidates.Values.Add(npct);
                    }
                }
            }

            //Debug.Log("SpawnDeployer : npcTypeCandidateIDs.Count = " + npcTypeCandidates.Count);

            if (npcTypeCandidates.Count != 0)
            {
                for (var n = 0; n < numDeployees; n++)
                {
                    int rndInd;
                    rndInd = Random.Range(0, npcTypeCandidates.Count - 1);
                    output.Add(npcTypeCandidates.Values[rndInd]);
                    //Debug.Log("SpawnDeployer : Added faction-classtype matched NPC");

                    //If more than 1 candidate, remove the picked candidate 
                    //from list to give some variety in spawned NPCs!
                    if (npcTypeCandidates.Count > 1)
                        npcTypeCandidates.Values.RemoveAt(rndInd);
                }
            }
            else
            {
                Debug.Log("SpawnDeployer : No NPC_Type candidates found to deploy!");
            }

            /*
            if (output.Count == 0)
            {
                //Still no matches
                //So recurse to parent as input faction, pass its output along
                Debug.Log("SpawnDeployer : Removed classtype restriction, added same-faction NPCs");
                return newDeployeeIDs(targetUnit, inputParentID);
            }
            */

            return output;
        }

        //function for recursing through taxonomy parents
        //to get a relevant class package
        NPCCollection getAncestorClassPackage(Taxonomy f)
        {
            if (f.parent == null)
            {
                return null;
            } //Failed to find an ancestor with a class package attached, return nothing

            if (f.parent.classPackage != null)
                return f.parent.classPackage;
            return getAncestorClassPackage(f.parent);
        }

        void deployNPC(NPC_Type npcType)
        {
            //Generate a spawn position and rotation, and create NPC
            //TODO: Spawn around the edge of / in a circle defined by maxSpawnDistance?
            //TODO: Set rotation to face deployer's transform.position
            //(Currently spawns in a square area)

            float deployX, deployZ; //either the deployer coords, or where its members are currently
            var newSI = new SpawnInfo();

            //If the unit has a linked patrol point & units still alive, we spawn at a still-alive unit
            if ((linkedPatrolPoint != null) && (deployed.Count > 0))
            {
                //TODO: Glitchy, NPC spawns as expected 
                //but then remains still briefly before teleporting
                //to deployer home location
                deployX = deployed[0].Position.x;
                deployZ = deployed[0].Position.z;
                newSI = deployed[0].RespawnInfo;
            }
            else
            {
                //Otherwise normal deploy
                deployX = transform.position.x;
                deployZ = transform.position.z;

                newSI.initialSpawnPoint = transform.position;
                newSI.initialSpawnRotation = transform.rotation.eulerAngles;
                newSI.levelMax = maxLevel;
                newSI.levelMin = minLevel;
                newSI.maxSpawnDistance = maxSpawnDistance;
                newSI.linkedPatrolPoint = linkedPatrolPoint;
                newSI.spawnerCategory = ESpawnerCategory.Deployer;
                //si.respawnInterval
                // si.timeOfDespawn
            }
            //For both types of deploy
            var deployOffset = maxSpawnDistance*Random.insideUnitCircle;
            deployX += deployOffset.x;
            deployZ += deployOffset.y;
            newSI.typeRef = npcType;


            if (maxSpawnDistance == 0)
                maxSpawnDistance = defaultSpawnDist;

            var rndPos = new Vector3(deployX, transform.position.y, deployZ);

            //TODO: Proper calculation and conversion to Unity of rotation?
            var randRotY = Random.Range(-180.0f, 180.0f);
            var rndRot = new Vector3(0, randRotY, 0);

            //NPC_Type newMemType = GameData.Get.npcDB.    GetNPCType(npcTypeID);

            var newNPC = NpcCharacter.Create(npcType, rndPos, rndRot, newSI);

            //TODO: Should be redundant after SpawnInfo alterations, remove once verified
            //if (newNPC.FameLevel == 0)
            //    newNPC.FameLevel = Random.Range(minLevel, maxLevel);

            //Type property altering
            if (newNPC != null)
            {
                if (Faction != null)
                    newNPC.Faction = Faction;
                else if (npcType.TaxonomyFaction != null)
                    newNPC.Faction = npcType.TaxonomyFaction;
                else
                    newNPC.Faction = GameData.Get.factionDB.defaultFaction;

                deployed.Add(newNPC);
                zone.AddToZone(newNPC);

                //If placing at existing moving deployees
                if ((linkedPatrolPoint != null) && (deployed.Count > 0))
                {
                    newNPC.Destination = deployed[0].Destination;
                }
                else
                {
                    //Face centre of the deployer
                    newNPC.SetFocusLocation(transform.position);
                }
            }
        }

        bool isClassTypeMatch(NPC_Type nt, NPCGroupClassUnit unit)
        {
            //Filter out if has forbidden CT
            for (var i = 0; i < unit.ForbidClassTypes.Count; i++)
            {
                var fct = unit.ForbidClassTypes[i];
                if (nt.ClassTypes.Contains(fct))
                {
                    return false;
                }
            }

            //Check for requested CT match
            for (var i = 0; i < unit.ReqClassTypes.Count; i++)
            {
                var rct = unit.ReqClassTypes[i];
                if (nt.ClassTypes.Contains(rct))
                {
                    return true;
                }
            }

            return false;
        }

        void doRedeploy()
        {
            //Clear out dead NPCs
            for (var i = 0; i < dead.Count; i++)
            {
                zone.RemoveFromZone(dead[i]);
            }
            dead = new List<NpcCharacter>();

            //Non-boss units
            //SimpleBoss group classes have no units so they just skip this bit

            for (var index = 0; index < npcGroupClass.units.Count; index++)
            {
                var units = newDeployeesFinal(npcGroupClass.units[index]);
                for (var i = 0; i < units.Count; i++)
                {
                    deployNPC(units[i]);
                }
            }


            //Refill the group with missing boss(es)           

            for (var index = 0; index < bosses.Count; index++)
            {
                var boss = bosses[index];
                var isPresent = false;
                for (var i = 0; i < deployed.Count; i++)
                {
                    var mem = deployed[i];
                    if (mem.typeRef == boss)
                    {
                        isPresent = true;
                        break;
                    }
                }
                //This boss isn't present, so deploy it
                if (!isPresent)
                {
                    deployNPC(boss);
                }
            }
        }

        void onEnterCombat()
        {
            for (var i = 0; i < deployed.Count; i++)
            {
                var n = deployed[i];
//TODO: appropriate combat mode selection / parameters
                if (meleeClasses.Contains(n.ClassType))
                {
                    n.CombatMode = ECombatMode.CBM_Melee;
                }
                else if (rangedClasses.Contains(n.ClassType))
                {
                    n.CombatMode = ECombatMode.CBM_Ranged;
                }
                else if (casterClasses.Contains(n.ClassType))
                {
                    n.CombatMode = ECombatMode.CBM_Cast;
                }
                else
                {
                    n.CombatMode = ECombatMode.CBM_Melee;
                } //default
            }
        }

        void onLeaveCombat()
        {
            for (var i = 0; i < deployed.Count; i++)
            {
                var n = deployed[i];
                n.CombatMode = ECombatMode.CBM_Idle;
            }
        }

        //TODO : Replace placeholder implementation
        //We want : If aggro triggered, enter combat
        //Hack: For unit testing, move to combat on any member health damage
        bool isCombatTriggered()
        {
            for (var i = 0; i < deployed.Count; i++)
            {
                var n = deployed[i];
                if ((n.Health < n.MaxHealth) && n.Health > 0)
                {
                    return true;
                }
            }
            return false;
        }

        bool isCombatEnded()
        {
            for (var i = 0; i < deployed.Count; i++)
            {
                var n = deployed[i];
                if ((n.Health < n.MaxHealth) && n.Health > 0)
                {
                    return false;
                }
            }
            return true;
        }

        [ContextMenu("Toggle spawn deployer enabled")]
        public void ToggleEnabled()
        {
            respawnPending = true;
            if (curState == EDeployerState.Disabled)
            {
                moveNextState(EDeployerCommand.Enable);
            }
            else
                moveNextState(EDeployerCommand.Disable);
        }
    }

    internal class DeployerStateTransition
    {
        EDeployerCommand command;
        EDeployerState currentState;

        public DeployerStateTransition(EDeployerState cs, EDeployerCommand c)
        {
            currentState = cs;
            command = c;
        }

        public override int GetHashCode()
        {
            return 17 + 31*currentState.GetHashCode() + 31*command.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as DeployerStateTransition;
            return (other != null) && (currentState == other.currentState) && (command == other.command);
        }
    }
}