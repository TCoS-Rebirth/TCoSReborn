//Debug flag - when false, NPCs will give all topics regardless of prequest / requirements status
//When true, NPCs will filter out topics not relevant to the parameter player
//#define FILTER_TOPICS

using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay.Conversations;
using Gameplay.Entities.Players;
using Gameplay.Entities.NPCs;
using Gameplay.Entities.NPCs.Behaviours;
using Gameplay.Quests;
using Gameplay.Skills;
using Network;
using Pathfinding;
using UnityEngine;
using Utility;
using World;

namespace Gameplay.Entities
{
    public class NpcCharacter : Character
    {
        public enum MoveResult
        {
            TargetNotReachable,
            ReachedTarget,
            SearchingPath,
            Moving
        }

        Vector3 _destination;

        List<int> _dialogs = new List<int>();

        ENPCMovementFlags _movementFlags = ENPCMovementFlags.ENMF_Walking;

        [SerializeField]
        NPCBehaviour behaviour;

        Vector3 focusLocation;

        [SerializeField]
        public SpawnInfo RespawnInfo;

        bool _isMoving;

        [Header("AI"), ReadOnly]
        [Tooltip("This should be set via script")]
        public NPC_Type typeRef;

        public ENPCMovementFlags MovementFlags
        {
            get { return _movementFlags; }
        }

        /// <summary>
        ///     Where this character is looking
        /// </summary>
        public Vector3 FocusLocation
        {
            get { return focusLocation; }
            set { focusLocation = value; }
        }

        public Vector3 Destination
        {
            get { return _destination; }
            set { _destination = value; }
        }

        /// <summary>
        ///     is this character currently moving
        /// </summary>
        public bool IsMoving
        {
            get { return _isMoving; }
            protected set { _isMoving = value; }
        }

        /// <summary>
        ///     TODO: real dialogClass
        /// </summary>
        public List<int> RelatedQuestIDs
        {
            get { return _dialogs; }
            set { _dialogs = value; }
        }

        void SetupFromTypeRef()
        {
            RespawnInfo.typeRef = typeRef;
            ClassType = typeRef.NPCClassClassification;
            //Name = typeRef.ShortName;
            Name = typeRef.name;
            //NPC_Type with level 0 indicates random level generation by spawner
            if (typeRef.FameLevel != 0)
            {
                FameLevel = typeRef.FameLevel;
            }
            else
            {
                FameLevel = Random.Range(RespawnInfo.levelMin, RespawnInfo.levelMax);
            }
            PepRank = typeRef.PePRank;
            var hp = 100;
            var levelHP = 100;
            typeRef.InitializeStats(FameLevel, PepRank, out hp, out levelHP, out Body, out Mind, out Focus);
            MaxHealth = Mathf.Max(hp, levelHP, FameLevel * 10, 100); //TODO fix
            Health = MaxHealth;
            if (typeRef.GroundSpeed > 0)
            {
                movementSpeeds[ENPCMovementFlags.ENMF_Normal] = (int)typeRef.GroundSpeed;
            }
            if (typeRef.StrollSpeed > 0)
            {
                movementSpeeds[ENPCMovementFlags.ENMF_Walking] = (int)typeRef.StrollSpeed;
            }
            Faction = typeRef.TaxonomyFaction;
            PawnState = EPawnStates.PS_ALIVE;
            if (typeRef.SkillDeck != null)
            {
                ActiveSkillDeck = Instantiate(typeRef.SkillDeck);
                ActiveSkillDeck.LoadForNPC(this);
            }

            //TODO: Attach behaviour corresponding to referenced AI state machine (Killer, passive etc.)

            if (RespawnInfo.spawnerCategory == ESpawnerCategory.Wildlife)
            {
                gameObject.AddComponent<KillerBehaviour>();
            }
            else if (RespawnInfo.spawnerCategory == ESpawnerCategory.Deployer)
            {
                gameObject.AddComponent<GroupBehaviour>();
            }

            //Attach pathing if appropriate
            if (RespawnInfo.linkedPatrolPoint != null)
            {
                gameObject.AddComponent<PathingBehaviour>();
            }
        }

        /// <summary>
        ///     Makes this character look at <see cref="unityPosition" /> and broadcasts the corresponding message to its relevance
        /// </summary>
        public void SetFocusLocation(Vector3 unityPosition)
        {
            focusLocation = unityPosition;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_NPCPAWN_SV2REL_FOCUSLOCATION(this));
        }

        public override void UpdateEntity()
        {
            base.UpdateEntity();
            HandleMovement();
            if (!ReferenceEquals(behaviour, null) && behaviour.enabled)
            {
                behaviour.UpdateBehaviour();
            }
        }

        public void AttachBehaviour(NPCBehaviour b)
        {
            behaviour = b;
        }

        public void DeAttachBehaviour()
        {
            if (behaviour != null)
            {
                Destroy(behaviour);
            }
        }

        protected override void OnDamageReceived(SkillApplyResult sap)
        {
            base.OnDamageReceived(sap);
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnDamage(sap.skillSource, sap.appliedSkill, sap.damageCaused);
            }
        }

        protected override void OnHealReceived(SkillApplyResult sap)
        {
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnHeal(sap.skillSource, sap.healCaused);
            }
        }

        public override void OnEntityBecameRelevant(Entity rel)
        {
            base.OnEntityBecameRelevant(rel);
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnLearnedRelevance(rel);
            }
        }

        public override void OnEntityBecameIrrelevant(Entity rel)
        {
            base.OnEntityBecameIrrelevant(rel);
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnReleasedRelevance(rel);
            }
        }

        protected override void OnEndCastSkill(SkillContext s)
        {
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnEndedCast(s.ExecutingSkill);
            }
        }

        public override void TeleportTo(Vector3 newPos, Quaternion newRot)
        {
            base.TeleportTo(newPos, newRot);
            if (behaviour && behaviour.enabled)
            {
                behaviour.OnTeleported();
            }
        }

        protected override void OnEnterZone(Zone z)
        {
            base.OnEnterZone(z);
            //drop to ground and fixate rotation
            Position = z.Raycast(transform.position, Vector3.down, 10f) + Vector3.up * BodyCenterHeight;
            _destination = Position;
            SetFocusLocation(transform.position + transform.forward);
        }

        #region Factory

        /// <summary>
        ///     Creates a new npc character instance and initializes it
        /// </summary>
        public static NpcCharacter Create(NPC_Type npcType, Vector3 pos, Vector3 rot, SpawnInfo sInfo = null)
        {
            if (npcType == null)
            {
                Debug.Log("NpcCharacter.Create : Invalid npcType, NpcCharacter not created");
                return null;
            }

            GameObject go;

            if (npcType.ShortName.Contains("No Text"))
                go = new GameObject(npcType.ShortName);
            else
                go = new GameObject(npcType.name);

            var newNpc = go.AddComponent<NpcCharacter>();
            newNpc.RetrieveRelevanceID();
            newNpc.typeRef = npcType;
            newNpc.transform.position = pos;
            newNpc.transform.rotation = Quaternion.Euler(rot);
            newNpc.RespawnInfo = sInfo;
            newNpc.SetupCollision();
            newNpc.SetupFromTypeRef();
            newNpc.InitializeStats();
            return newNpc;
        }

        #endregion

        #region Movement

        Dictionary<ENPCMovementFlags, int> movementSpeeds = new Dictionary<ENPCMovementFlags, int>
        {
            {ENPCMovementFlags.ENMF_Normal, 200},
            {ENPCMovementFlags.ENMF_Walking, 100},
            {ENPCMovementFlags.ENMF_Sitting, 0}
        };

        public void SetMoveSpeed(ENPCMovementFlags speedFlag)
        {
            int speed;
            if (movementSpeeds.TryGetValue(speedFlag, out speed))
            {
                _movementFlags = speedFlag;
                SetMoveSpeed(speed);
            }
        }

        List<Vector3> currentPath = new List<Vector3>();

        [SerializeField, HideInInspector]
        public float minMoveDistance = 0.5f;

        Vector3 prevTargetPos;
        float searchStarted;
        MoveResult pathMoveState = MoveResult.SearchingPath;

        /// <summary>
        ///     Use pathfinding to reach the target
        /// </summary>
        public MoveResult MoveTo(Vector3 location)
        {
            if (VectorMath.SqrDistanceXZ(location, prevTargetPos) > minMoveDistance * minMoveDistance)
            {
                currentPath.Clear();
                prevTargetPos = location;
                var activePath = ABPath.Construct(Position, location, OnPathComplete);
                AstarPath.StartPath(activePath);
                searchStarted = Time.time;
                pathMoveState = MoveResult.SearchingPath;
            }
            if (pathMoveState == MoveResult.SearchingPath)
            {
                if (Time.time - searchStarted > 5f) //timeout
                {
                    pathMoveState = MoveResult.TargetNotReachable;
                    return pathMoveState;
                }
                if (currentPath.Count > 0)
                {
                    if (VectorMath.SqrDistanceXZ(location, currentPath[currentPath.Count - 1]) > 1f)
                    {
                        pathMoveState = MoveResult.TargetNotReachable;
                    }
                    else
                    {
                        if (currentPath.Count > 0)
                        {
                            currentPath.RemoveAt(0);
                        }
                        SetNextDestinationInCurrentPath();
                        pathMoveState = MoveResult.Moving;
                    }
                }
            }
            return pathMoveState;
        }

        /// <summary>
        ///     Skips pathfinding (useful for AIPaths, where the path is already there)
        /// </summary>
        public MoveResult MoveToDirect(Vector3 location)
        {
            if (VectorMath.SqrDistanceXZ(location, prevTargetPos) > minMoveDistance * minMoveDistance)
            {
                prevTargetPos = location;
                IsMoving = true;
                _destination = location;
                currentPath.Clear();
                if (RelevanceContainsPlayers)
                {
                    BroadcastRelevanceMessage(PacketCreator.S2R_GAME_NPCPAWN_SV2REL_MOVE(this));
                }
                pathMoveState = MoveResult.Moving;
            }
            if (pathMoveState != MoveResult.Moving & pathMoveState != MoveResult.ReachedTarget)
            {
                pathMoveState = MoveResult.Moving;
            } //force mode, if switched from pathfinding-movement
            return pathMoveState;
        }

        public void CancelMovement()
        {
            _destination = Position;
            IsMoving = false;
            if (!RelevanceContainsPlayers) return;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_NPCPAWN_SV2REL_STOPMOVEMENT(this));
        }

        void OnPathComplete(Path p)
        {
            p.Claim(this);
            PathPostProcessor.Instance.Process(p);
            currentPath = p.vectorPath;
            p.vectorPath = null;
            p.Release(this);
        }

        Vector3 GetNavMeshClampedPosition(Vector3 queryPos)
        {
            //Vector3 clampedPos = AstarPath.active.GetNearest(queryPos, NNConstraint.Default).clampedPosition + (Vector3.up * bodyCenterHeight);
            //if (AstarMath.MagnitudeXZ(transform.position, clampedPos) < 1f)
            //{
            //    return clampedPos;
            //}
            //return queryPos;
            return queryPos + Vector3.up * (BodyCenterHeight + 50f * UnitConversion.UnrUnitsToMeters); //safe margin
        }

        bool IsPathPossible(Vector3 end, float threshold)
        {
            var a = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
            var ni = AstarPath.active.GetNearest(end, NNConstraint.Default);
            var b = ni.node;
            if (a == null || b == null)
            {
                return false;
            }
            if (VectorMath.SqrDistanceXZ(end, ni.clampedPosition) >= threshold * threshold)
            {
                return false;
            }
            var isPossible = PathUtilities.IsPathPossible(a, b);
            return isPossible;
        }

        bool SetNextDestinationInCurrentPath()
        {
            if (!ReferenceEquals(currentPath, null) && currentPath.Count > 0)
            {
                _destination = GetNavMeshClampedPosition(currentPath[0]);
                currentPath.RemoveAt(0);
                if (RelevanceContainsPlayers)
                {
                    BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PAWN_SV2CLREL_TELEPORTTO(this));
                    //enable if desyncs become too noticable (client can behave stupid sometimes)
                    //BroadcastRelevanceMessage(PacketCreator.S2R_GAME_NPCPAWN_SV2REL_STOPMOVEMENT(this));
                    BroadcastRelevanceMessage(PacketCreator.S2R_GAME_NPCPAWN_SV2REL_MOVE(this));
                }
                return true;
            }
            _destination = Position;
            return false;
        }

        void HandleMovement()
        {
            if (PawnState == EPawnStates.PS_DEAD)
            {
                IsMoving = false;
                //yield return null;
                return;
            }
            if (pathMoveState == MoveResult.Moving && !FreezePosition)
            {
                IsMoving = true;
                if (VectorMath.SqrDistanceXZ(_destination, transform.position) > 0.25f)
                {
                    Position = Vector3.MoveTowards(Position, _destination,
                    //GetEffectiveMoveSpeed() * UnitConversion.UnrUnitsToMeters * 0.1f);
                    GetEffectiveMoveSpeed() * UnitConversion.UnrUnitsToMeters * Time.deltaTime);
                    Vector3 rCastPoint;
                    if (ActiveZone.Raycast(transform.position + Vector3.up, Vector3.down, 5f, out rCastPoint))
                    {
                        Position = rCastPoint + Vector3.up * BodyCenterHeight;
                    }
                }
                else
                {
                    if (!SetNextDestinationInCurrentPath())
                    {
                        pathMoveState = MoveResult.ReachedTarget;
                    }
                }
            }
            else
            {
                IsMoving = false;
            }
        }

        #endregion

        #region Interact

        public void OnSwirlyOption(PlayerCharacter source, ERadialMenuOptions option1, ERadialMenuOptions option2)
        {
            switch (option1)
            {
                case ERadialMenuOptions.RMO_MAIN:
                    switch (option2)
                    {
                        case ERadialMenuOptions.RMO_CONVERSATION:
                            NewTopic(source, typeRef.chooseBestTopic());
                            break;
                    }
                    break;
            }
        }

        //public void React(PlayerCharacter source,)
        #endregion

        #region Conversations

        public void NewTopic(PlayerCharacter source, ConversationTopic newTopic)
        {
            if (newTopic != null)
            {
                var bestNode = ConversationTopic.chooseBestNode(newTopic.getStartNodes());
                source.currentConv = new PlayerCharacter.CurrentConv(newTopic, bestNode, this);

                var topicsToGive = PrepareTopics(source);

                var m = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE(this, newTopic, bestNode, topicsToGive);
                source.ReceiveRelevanceMessage(this, m);
            }
            else
            {
                Debug.Log("NpcCharacter.NewTopic : failed to get the new topic");
            }
        }


        public void Converse(PlayerCharacter source, int responseID)
        {
            var srcConv = source.currentConv;

            var topicsToGive = PrepareTopics(source);

            //Handle quest provide topic special responses (i.e. Accept, Decline quest)
            if (srcConv.curTopic.TopicType == EConversationType.ECT_Provide)
            {
                CT_ProvideQuest srcTopic = (CT_ProvideQuest)srcConv.curTopic;

                if (responseID == srcTopic.Accept.resource.ID)
                {
                    tryGiveQuest(source, srcTopic);
                    return;
                }
                else if (responseID == srcTopic.Decline.resource.ID)
                {
                    handleQuestDeclined(source);
                }
            }

            //Select response

            var nextNode = srcConv.curTopic.getNextNode(srcConv.curNode, responseID);

            if (nextNode == null)
            {
                Debug.Log("NpcCharacter.Converse : failed to get next node, searching topics...");
                foreach (var ct in topicsToGive)
                {
                    if (ct.resource.ID == responseID)
                    {
                        source.currentConv.curTopic = ct;
                        NewTopic(source, ct);
                        return;
                    }
                }

                //No matching nodes or topics - end the conversation
                Debug.Log("NpcCharacter.Converse : ... failed to get any new node, ending conversation");                
                var mEndConverse = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE(this);
                source.currentConv = null;
                source.ReceiveRelevanceMessage(this, mEndConverse);
                return;
            }

            //Update the currentConv curNode to the new node
            srcConv.curNode = nextNode;

            if (srcConv.curTopic == null)
            {
                Debug.Log("NpcCharacter.Converse: failed to get converse topic");
                return;
            }


            var mConverse = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE(this, srcConv.curTopic, srcConv.curNode,
                topicsToGive);
            source.ReceiveRelevanceMessage(this, mConverse);
        }



        public List<ConversationTopic> PrepareTopics(PlayerCharacter p)   //can set p to null when noFilter == true
        {

            var topicRefs = new List<SBResource>();
#if FILTER_TOPICS
            //Offer all topics
            topicRefs.AddRange(typeRef.Topics);
            topicRefs.AddRange(typeRef.QuestTopics);
#else
                //Add normal topics, filter out current topic ID, greeting topics
                foreach (var topic in typeRef.Topics)
                {
                    if (    (topic.ID != p.currentConv.curTopic.resource.ID)
                        &&  (!topic.Name.Contains("CT_G"))
                        )
                    {
                        topicRefs.Add(topic);
                    }
                }

                foreach (var topic in typeRef.QuestTopics)
                {
                    if (topic.ID == p.currentConv.curTopic.resource.ID) { break; }  //Ignore the current topic

                    Quest_Type parentQuest = GameData.Get.questDB.GetQuestFromContained(topic);
                    if (parentQuest == null)
                    {
                        Debug.Log("NPC.PrepareTopics : Couldn't resolve parent quest of topic " + topic.Name);
                        break;
                    }

                    //Provide quest topics

                    if (topic.Name.Contains("CT_Provide")
                        && (p.IsEligibleForQuest(parentQuest))                           //check player is eligible
                        && (!p.HasQuest(parentQuest.resourceID))                    //and they don't currently have quest
                        && (!p.CompletedQuest(parentQuest.resourceID))              //and they haven't already completed quest
                    )
                    {
                        topicRefs.Add(topic);
                    }

                    //Mid quest topics
                    else if (topic.Name.Contains("CT_Mid")
                        && (p.HasQuest(parentQuest.resourceID))                 //check player currently has quest
                        && (p.HasUnfinishedTargets(parentQuest))                 //and at least 1 quest target remains incomplete                                                        
                        )
                    {
                        topicRefs.Add(topic);
                    }

                    //Complete quest topics (quests in curQuests with all targets complete)
                    else if (topic.Name.Contains("CT_Finish")
                         && (p.HasQuest(parentQuest.resourceID))    //check player currently has quest
                         && (!p.HasUnfinishedTargets(parentQuest))  //and all quest targets must be complete
                        )
                    {
                        topicRefs.Add(topic);
                    }

                    //TODO : Quest target topics

                }
#endif
            var topicsOut = GameData.Get.convDB.GetTopics(topicRefs);
            return topicsOut;
        }
        #endregion

        #region Quests

        public bool tryGiveQuest(PlayerCharacter p, CT_ProvideQuest provideTopic)
        {
            //Handle quest accept response here
            var quest = GameData.Get.questDB.GetQuestFromContained(provideTopic.Accept.resource);
            if (quest == null)
            {
                Debug.Log("NPC.Converse : Find quest failed (GetQuestFromContained returned null)");
                return false;
            }
            else {
                List<int> tarProgressArray = new List<int>();
                //Target progress array generation
                //foreach (var target in quest.targets)
                for (int n = 0; n < quest.targets.Count; n++)
                {
                    //TODO : proper target base values
                    tarProgressArray.Add(0);
                }

                p.QuestData.curQuests.Add(new PlayerQuestProgress(quest.resourceID, tarProgressArray));

                var mQuestAdd = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_ADDQUEST(quest.resourceID, tarProgressArray);                
                var endConv = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE(this);
                p.currentConv = null;
                p.ReceiveRelevanceMessage(this, mQuestAdd);
                p.ReceiveRelevanceMessage(this, endConv);
                return true;
            }
        }

        public void handleQuestDeclined(PlayerCharacter p)
        {
            //TODO: Placeholder implementation just closes the conversation window (return to start node?)            
            var endConv = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE(this);
            p.currentConv = null;
            p.ReceiveRelevanceMessage(this, endConv);
        }
        #endregion

        /*
        private bool containsID(List<SBResource> list, int id)
        {
            foreach (var r in list)
            {
                if (r.ID == id) { return true; }
            }

            return false;
        }
        */
    }
}