//Debug flags

//#define NO_TOPIC_FILTER
//when undefined, NPCs will give all topics regardless of prequest / requirements status
//When defined, NPCs will filter out topics not relevant to the parameter player


#define BLOCK_DISABLED_QUESTS   //When defined, NPCs won't give players quests flagged disabled

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
using System.Linq;
using Gameplay.Quests.QuestTargets;
using Gameplay.Items;

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
        NPCBehaviour curBehaviour;

        [SerializeField]
        NPCBehaviour defaultBehaviour;

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
           // if (typeRef.ShortName != null) transform.name = Name = typeRef.ShortName;
            //else if (typeRef.LongName != null) transform.name = Name = typeRef.LongName;
            transform.name = Name = typeRef.name;

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

            //TODO: Make AI state machine an enumerated flag for efficiency(Killer, passive etc.)
            if (    RespawnInfo.referenceAiStateMachine != null
                &&  RespawnInfo.referenceAiStateMachine.Contains("Kill")) {
                defaultBehaviour = gameObject.AddComponent<KillerBehaviour>();
                
                
            }
            else if (RespawnInfo.spawnerCategory == ESpawnerCategory.Wildlife)
            {
                
                //If AI state machine reference is critter machine, set critter
                //Pacifies the killer bunny rabbits =p
                if (RespawnInfo.referenceAiStateMachine != null
                &&  RespawnInfo.referenceAiStateMachine.Contains("Critter"))
                {
                    defaultBehaviour = gameObject.AddComponent<CritterBehaviour>();
                }
                else {
                    defaultBehaviour = gameObject.AddComponent<KillerBehaviour>();
                }
            }
            else if (RespawnInfo.spawnerCategory == ESpawnerCategory.Deployer)
            {
                defaultBehaviour = gameObject.AddComponent<GroupBehaviour>();
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
            if (!ReferenceEquals(curBehaviour, null) && curBehaviour.enabled)
            {
                curBehaviour.UpdateBehaviour();
            }
        }

        public void AttachBehaviour(NPCBehaviour b)
        {
            curBehaviour = b;
        }

        public void DeAttachBehaviour()
        {
            if (curBehaviour != null)
            {
                Destroy(curBehaviour);
            }
        }

        public void DefaultBehaviour()
        {
            DeAttachBehaviour();
            curBehaviour = defaultBehaviour;
        }

        protected override void OnDamageReceived(SkillApplyResult sap)
        {
            base.OnDamageReceived(sap);
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnDamage(sap.skillSource, sap.appliedSkill, sap.damageCaused);
            }
        }

        protected override void OnHealReceived(SkillApplyResult sap)
        {
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnHeal(sap.skillSource, sap.healCaused);
            }
        }

        public override void OnEntityBecameRelevant(Entity rel)
        {
            base.OnEntityBecameRelevant(rel);
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnLearnedRelevance(rel);
            }
        }

        public override void OnEntityBecameIrrelevant(Entity rel)
        {
            base.OnEntityBecameIrrelevant(rel);
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnReleasedRelevance(rel);
            }
        }

        protected override void OnEndCastSkill(SkillContext s)
        {
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnEndedCast(s.ExecutingSkill);
            }
        }

        protected override void OnDiedThroughDamage(Character source)
        {
            base.OnDiedThroughDamage(source);

            //Valshaaran - dispatch NPC death event to source player
            if (source is PlayerCharacter)
            {
                var sourcePlayer = source as PlayerCharacter;
                sourcePlayer.OnNPCKill(this);

                //If player has team, dispatch to relevant team members other than player
                if (sourcePlayer.Team != null)
                {
                    foreach (var teamMember in sourcePlayer.Team.Members)
                    {
                        if (teamMember.RelevanceID == sourcePlayer.RelevanceID) continue;   //not self
                        if (source.ObjectIsRelevant(this))
                        {
                            teamMember.OnNPCKill(this);
                        }
                    }
                }
            }
        }

        public override void TeleportTo(Vector3 newPos, Quaternion newRot)
        {
            base.TeleportTo(newPos, newRot);
            if (curBehaviour && curBehaviour.enabled)
            {
                curBehaviour.OnTeleported();
            }
        }

        protected override void OnEnterZone(Zone z)
        {
            base.OnEnterZone(z);

            //drop to ground and fixate rotation
            /*Valshaaran - experimentally done in spawners to prevent dropping in from air
            Position = z.Raycast(transform.position, Vector3.down, 10f) + Vector3.up * BodyCenterHeight;
            */
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
                            SetFocusLocation(source.Position);
                            isConversing = true;
                            NewTopic(source, chooseBestTopic(source));
                            break;
                    }
                    break;
            }
        }

        //public void React(PlayerCharacter source,)
        #endregion

        #region Conversations

        [ReadOnly]
        public bool isConversing;

        public void NewTopic(PlayerCharacter source, ConversationTopic newTopic)
        {
            if (newTopic != null)
            {
                var bestNode = ConversationTopic.chooseBestNode(newTopic.getStartNodes());
                source.currentConv = new PlayerCharacter.CurrentConv(newTopic, bestNode, this);

                var topicsToGive = PrepareTopics(source);

                var m = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE(this, newTopic, bestNode, topicsToGive);
                source.ReceiveRelevanceMessage(this, m);

                //Execute topic events
                foreach (var ev in newTopic.Events)
                {
                    ev.TryExecute(this, source);
                }

            }
            else
            {
                Debug.Log("NpcCharacter.NewTopic : failed to get the new topic");
            }
        }

        public void Converse(PlayerCharacter source, int responseID)
        {
            var srcConv = source.currentConv;
            var nextNode = srcConv.curTopic.getNextNode(srcConv.curNode, responseID);

            #region Provide
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
                    return;
                }
            }
            #endregion

            #region QT_Talk last node
            //Handle QT_Talk last node quest target update
            if ((srcConv.curTopic.TopicType == EConversationType.ECT_Talk)
            && ((nextNode == null) || nextNode.responses.Count == 0))                     //If no further responses
            {
                {
                    //Set target progress to 1 serverside
                    //Get relevant quest
                    Quest_Type curQuest = GameData.Get.questDB.GetQuestFromContained(srcConv.curTopic.resource);

                    //Match current topic with QT_Target in curQuest
                    foreach (var target in curQuest.targets)
                    {
                        var qtTalk = target as QT_Talk;
                        if (qtTalk != null)
                        {
                            if (qtTalk.TopicID.ID == srcConv.curTopic.resource.ID)
                            {
                                //Update quest data
                                source.TryAdvanceTarget(curQuest, target);
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region QT_Fedex

            #endregion

            #region Finish topic
            if (srcConv.curTopic.TopicType == EConversationType.ECT_Finish)
            {
                Quest_Type questToFinish = GameData.Get.questDB.GetQuestFromContained(srcConv.curTopic.resource);
                if (!source.QuestIsComplete(questToFinish.resourceID))
                {
                    source.FinishQuest(questToFinish);

                    //TODO: Placeholder - new topic?
                    //NewTopic(p, typeRef.chooseBestTopic());
                }
            }

            #endregion

            var topicsToGive = PrepareTopics(source);

            //Select response

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
                EndConversation(source);
                return;
            }

            //Update the currentConv curNode to the new node
            srcConv.curNode = nextNode;

            if (srcConv.curTopic == null)
            {
                Debug.Log("NpcCharacter.Converse: failed to get converse topic");
                return;
            }


            //TODO : Valshaaran - experimental
            if ((responseID == 0) && (source.currentConv != null)) //Response ID 0 ends conversation
            {
                EndConversation(source);
            }
            else {
                var mConverse = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_CONVERSE(this, srcConv.curTopic, srcConv.curNode,
                    topicsToGive);
                source.ReceiveRelevanceMessage(this, mConverse);
            }
        }

        public List<ConversationTopic> PrepareTopics(PlayerCharacter p)   //can set p to null when noFilter == true
        {
            var topicRefs = new List<SBResource>();

#if NO_TOPIC_FILTER
            //Offer all topics
            topicRefs.AddRange(typeRef.Topics);
            topicRefs.AddRange(typeRef.QuestTopics);
#else

            //Fulfil any null-topic quest target

            //Add normal topics, filter out current topic ID, greeting topics
            foreach (var nTopic in typeRef.Topics)
            {
                ConversationTopic fullTopic = GameData.Get.convDB.GetTopic(nTopic);
                if ((nTopic.ID != p.currentConv.curTopic.resource.ID)
                    && (!nTopic.Name.Contains("CT_G"))
                    )
                {
                    //Check requirements
                    if (fullTopic.requirementsMet(p)) topicRefs.Add(nTopic);
                }
            }

            foreach (var qTopic in typeRef.QuestTopics)
            {
                ConversationTopic fullTopic = GameData.Get.convDB.GetTopic(qTopic);
                if (!fullTopic.requirementsMet(p)) continue;

                if (p.currentConv != null)
                {
                    if (qTopic.ID == p.currentConv.curTopic.resource.ID) { continue; }  //Ignore the current topic
                }
                Quest_Type parentQuest = GameData.Get.questDB.GetQuestFromContained(qTopic);
                if (parentQuest == null)
                {
                    Debug.Log("NPC.PrepareTopics : Couldn't resolve parent quest of topic " + qTopic.Name);
                    break;
                }
                #region QT_Talk handling
                if (qTopic.Name.Contains("QT_T"))    //QT_Talk
                {
                    var qtTalkParentQuest = GameData.Get.questDB.GetQuestFromContained(qTopic);
                    var pHasQuest = p.HasQuest(qtTalkParentQuest.resourceID);
                    if (pHasQuest)
                    {  //Initial check that player has quest 

                        //Get the QT_Talk obj
                        foreach (var target in qtTalkParentQuest.targets)
                        {

                            var qtTalk = target as QT_Talk;
                            if (qtTalk != null)
                            {

                                if (qtTalk.TopicID.ID == qTopic.ID)
                                {
                                    if (p.PreTargetsComplete(target, qtTalkParentQuest))    //If all pretargets and completed and requirements are met
                                        topicRefs.Add(qTopic);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region QT_Fedex Thanks handling
                //TODO
                #endregion

                #region Provide
                //Provide quest topics

                else if (qTopic.Name.Contains("CT_Prov")
                        && (p.IsEligibleForQuest(parentQuest))                           //check player is eligible
                        && (!p.HasQuest(parentQuest.resourceID))                    //and they don't currently have quest
                        && (!p.QuestIsComplete(parentQuest.resourceID))              //and they haven't already completed quest
                    )
                {
                    topicRefs.Add(qTopic);
                }
                #endregion

                #region Mid
                //Mid quest topics
                else if (qTopic.Name.Contains("CT_Mid")
                        && (p.HasQuest(parentQuest.resourceID))                 //check player currently has quest
                        && (p.HasUnfinishedTargets(parentQuest))                 //and at least 1 quest target remains incomplete                                                        
                        )
                {
                    topicRefs.Add(qTopic);
                }
                #endregion

                #region Finish
                //Complete quest topics (quests in curQuests with all targets complete)                

                else if (qTopic.Name.Contains("CT_Fin"))
                {

                    if (p.HasQuest(parentQuest.resourceID))
                    {
                        //check player currently has quest

                        
                        foreach (var tar in parentQuest.targets)
                        {
                            //Null-topic QT_Talk is handled here so that is is fulfilled when the quest targets are checked below
                            var qtTalk = tar as QT_Talk;
                            if ((qtTalk != null)
                                && (qtTalk.TopicID.ID == 0)
                                && (p.PreTargetsComplete(tar, parentQuest))
                                )
                            {
                                //Complete the target
                                p.TryAdvanceTarget(parentQuest, tar);

                                break;
                            }

                            //Null-recipient QT_Fedex Thanks
                            var qtFedex = tar as QT_Fedex;
                            if ((qtFedex != null)
                                && (qtFedex.NpcRecipientID.ID == 0)
                                && (p.PreTargetsComplete(tar, parentQuest))
                                //TODO : Proper serverside inventory check needed?
                                ) {

                                //Complete the target
                                p.TryAdvanceTarget(parentQuest, tar);
                                break;
                            }
                        }

                        if (!p.HasUnfinishedTargets(parentQuest))  //all quest targets must be complete)  
                        {
                            topicRefs.Add(qTopic);
                        }
                    }
                }
                #endregion

                //TODO : Quest target topics player is eligible for by other conditions

            }
#endif

            var topicsOut = GameData.Get.convDB.GetTopics(topicRefs);
            return topicsOut;
        }

        public ConversationTopic chooseBestTopic(PlayerCharacter p)
        {
            //TODO : Placeholder, improve topic choice logic

            //Retrieve non-quest topics
            var choices = GameData.Get.convDB.GetTopics(typeRef.Topics);

            //Talk topic (QT_Talk?)
            foreach (var topic in choices)
            {
                if (topic.TopicType == EConversationType.ECT_Talk)
                {
                    return topic;
                }
            }

            //Otherwise pick a topic of type Chat
            var freeTopics = new List<ConversationTopic>();
            foreach (var topic in choices)
            {
                if (topic.TopicType == EConversationType.ECT_Free)
                {
                    freeTopics.Add(topic);
                }
            }
            if (freeTopics.Count > 0)
            {
                var rndInd = Random.Range(0, freeTopics.Count - 1);
                return freeTopics[rndInd];
            }

            //Otherwise pick a Greeting topic
            foreach (var topic in choices)
            {
                if (topic.TopicType == EConversationType.ECT_Greeting)
                {
                    return topic;
                }
            }


            //TODO : Placeholder - Otherwise preparetopics[0]
            List<ConversationTopic> prepareTopics = PrepareTopics(p);
            if ((prepareTopics != null) && (prepareTopics.Count > 0))
                return PrepareTopics(p)[0];
            else {
                Debug.Log("NpcCharacter.chooseBestTopic : Couldn't choose any suitable topic");
                return null;
            }
        }

        public void EndConversation(PlayerCharacter p)
        {
            isConversing = false;
            var mEndConverse = PacketCreator.S2C_GAME_PLAYERCONVERSATION_SV2CL_ENDCONVERSE(this);
            p.currentConv = null;
            p.SendToClient(mEndConverse);
            SetFocusLocation(transform.position + transform.forward);
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




                //Handle disabled quest
#if BLOCK_DISABLED_QUESTS
                if (!quest.Disabled)
                {
#endif
                    List<int> tarProgressArray = new List<int>();
                    //Target progress array generation
                    //foreach (var target in quest.targets)
                    for (int n = 0; n < quest.targets.Count; n++)
                    {
                        //TODO : proper target base values
                        tarProgressArray.Add(0);
                    }

                    p.questData.curQuests.Add(new PlayerQuestProgress(quest.resourceID, tarProgressArray));

                    var mQuestAccept = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_ACCEPTQUEST(quest.resourceID, tarProgressArray);
                    p.ReceiveRelevanceMessage(this, mQuestAccept);
                    EndConversation(p);

                    return true;
#if BLOCK_DISABLED_QUESTS                    
                }

                else {
                    EndConversation(p);
                    p.ReceiveChatMessage("", "Quest is disabled in this build!", EGameChatRanges.GCR_SYSTEM);
                    return false;
                }
#endif
            }
        }

        public void handleQuestDeclined(PlayerCharacter p)
        {
            //TODO: Placeholder implementation just closes the conversation window (return to start node?)            
            EndConversation(p);
        }

        public List<int> getRelatedQuestIDs()
        {
            List<int> output = new List<int>();

            //cycle quest topics
            foreach (var questTopic in typeRef.QuestTopics)
            {
                Quest_Type relatedQuest = GameData.Get.questDB.GetQuestFromContained(questTopic);
                if (relatedQuest != null)
                {
                    output.Add(relatedQuest.resourceID);
                }
                else
                {
                    Debug.Log("NpcCharacter.getRelatedQuestIDs : Couldn't find a parent quest of topic " + questTopic.Name);
                }
            }
            return output;
        }

        #endregion

        #region Loot

        public int DropMoney()
        {
            int output = 0;
            foreach(var lt in Faction.Loot)
            {
                output += lt.GenerateMoney(FameLevel);
            }

            foreach(var lt in typeRef.Loot)
            {
                output += lt.GenerateMoney(FameLevel);
            }
            return output;
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