using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay.Events;
using Gameplay.RequirementSpecifier;
using UnityEngine;
using Random = UnityEngine.Random;
using Gameplay.Entities;

namespace Gameplay.Conversations
{
    [Serializable]
    public class ConversationTopic : ScriptableObject
    {
        //var (Conversation) editinline array<Conversation_Node> Conversations;
        public List<ConversationNode> allNodes = new List<ConversationNode>();
        public List<ConversationResponse> allResponses = new List<ConversationResponse>();

        //var (Requirements) bool AvailableTopic;
        //var (Requirements) bool PublicTopic;
        public bool AvailableTopic, PublicTopic;

        //var (Banner) class<Emitter> EmitterClass;       

        //var (Banner) float BannerTime;
        float BannerTime;

        //var (Banner) LocalizedString Chat;
        SBLocalizedString Chat;

        [Header("Banner")]
        //var (Banner) byte Emote;
        EContentEmote Emote;

        [Header("Other")]
        //var (Events) editinline array<Content_Event> Events;
        public List<Content_Event> Events = new List<Content_Event>();

        public string internalName;

        //var (Conversation) int ButtonImage;

        [Header("Topics")]
        //var (Requirements) int Priority;
        public int Priority;

        //var (Requirements) array<Content_Requirement> Requirements;
        public List<Content_Requirement> Requirements;
        public SBResource resource;

        //var (Requirements) string ScriptReference;
        public string ScriptReference;

        //var (Banner) Texture Icon;

        //var (Banner) Sound Speech;
        EPawnSound Speech;
        //var (Minimap) float MinimapRange;

        public List<SBResource> startNodes = new List<SBResource>();

        //var (Conversation) LocalizedString TopicText;
        public SBLocalizedString TopicText; //, defaultText;

        [Header("Conversation")]

        //var (Type) byte TopicType;
        public EConversationType TopicType = EConversationType.ECT_None;


        public List<ConversationResponse> getResponses(ConversationNode node)
        {
            var output = new List<ConversationResponse>();
            foreach (var r in allResponses)
            {
                foreach (var respRes in node.responses)
                {
                    if (r.resource.ID == respRes.ID)
                    {
                        output.Add(r);
                    }
                }
            }
            return output;
        }

        public List<ConversationNode> getNodes(ConversationResponse resp)
        {
            var output = new List<ConversationNode>();
            foreach (var n in allNodes)
            {
                foreach (var nodeRes in resp.conversations)
                {
                    if (n.resource.ID == nodeRes.ID)
                    {
                        output.Add(n);
                    }
                }
            }
            return output;
        }

        public static ConversationNode chooseBestNode(List<ConversationNode> inputNodes)    //TODO : proper selection will need Player parameter
                                                                                            //More examination of multi-node instances needed
        {
            if (inputNodes == null)
            {
                Debug.Log("ConversationTopic.chooseBestNode() : NULL input nodes list!");
                return null;
            }

            if (inputNodes.Count == 0)
            {
                Debug.Log("ConversationTopic.chooseBestNode() : empty input nodes list");
                return null;
            }
            if (inputNodes.Count == 1)
            {
                return inputNodes[0];
            }

            //TODO : placeholder, implement proper best node selection logic
            var index = Random.Range(0, inputNodes.Count - 1);
            Debug.Log("ConversationTopic.chooseBestNode() : TODO - proper best node selection, randomly choose a node for now...");
            return inputNodes[index];
        }

        //TODO: Currently assuming responseID refers to the response's index
        public ConversationNode getNextNode(ConversationNode curNode, int responseID)
        {
            foreach (var cr in getResponses(curNode))
            {
                if (cr.resource.ID == responseID)
                {
                    return chooseBestNode(getNodes(cr));
                }
            }
            //Debug.Log("ConversationTopic.chooseBestNode : failed to find response " + responseID + " in node " + curNode.resource.ID);
            return null;
        }


        //Going left from LSB, sets bit to 1 if corresponding node response is found in all responses
        // == 0 means no responses
        public int getResponseFlags(ConversationNode currentState)
        {
            int output, allRespCount, nodeRespCount;
            output = 0;
            allRespCount = 0;
            //Iterate all responses
            while (allRespCount < allResponses.Count)
            {
                //Iterate current node's responses
                nodeRespCount = 0;
                while (nodeRespCount < currentState.responses.Count)
                {
                    //If resps match
                    if (allResponses[allRespCount].resource.ID == currentState.responses[nodeRespCount].ID)
                    {
                        //sets bit to 1 nodeRespCount bits left of LSB
                        output = output | (1 << nodeRespCount);
                        break;
                    }
                    nodeRespCount++;
                }
                if (allRespCount >= currentState.responses.Count)
                {
                    Debug.Log("ConversationTopic.getResponseFlags : Failed to find response " + allResponses[allRespCount].resource.Name);
                }
                allRespCount++;
            }
            return output;

            /*
             local int ret;
            local int rii;	//Counts up to amount of responses
            local int rio;	//Counts up to current state responses, for each in all responses
            rii = 0;                                                                    //0000 : 0F 00 60 41 75 14 25 
            while (rii < Responses.Length) {                                            //0007 : 07 D9 00 96 00 60 41 75 14 37 01 80 CC 73 14 16 
              rio = 0;                                                                  //0017 : 0F 00 F0 66 74 14 25 
              while (rio < CurrentState.Responses.Length) {                             //001E : 07 7D 00 96 00 F0 66 74 14 37 19 01 A8 4E C6 0F 05 00 00 01 38 0C 75 14 16 
                if (CurrentState.Responses[rio] == Responses[rii]) {                    //0037 : 07 73 00 72 10 00 F0 66 74 14 19 01 A8 4E C6 0F 05 00 00 01 38 0C 75 14 10 00 60 41 75 14 01 80 CC 73 14 16 
		
			        //Bitwise operation:
			        //ret | 1 sets lowest bit to 1, all others to ret
			        // << rio shifts this to the left rio bits
			        
				
                  ret = ret | 1 << rio;                                                 //005B : 0F 00 88 60 76 14 9E 00 88 60 76 14 94 26 00 F0 66 74 14 16 16 
                  goto jl007D;                                                          //0070 : 06 7D 00 
                }
                rio++;                                                                  //0073 : A5 00 F0 66 74 14 16 
              }
              UCASSERT(rio < CurrentState.Responses.Length,"Couldn't find response" @ string(Responses[rii])
                @ "in"
                @ string(CurrentState));//007D : 1C 48 12 49 11 96 00 F0 66 74 14 37 19 01 A8 4E C6 0F 05 00 00 01 38 0C 75 14 16 A8 A8 A8 1F 43 6F 75 6C 64 6E 27 74 20 66 69 6E 64 20 72 65 73 70 6F 6E 73 65 00 39 56 10 00 60 41 75 14 01 80 CC 73 14 16 1F 69 6E 00 16 39 56 01 A8 4E C6 0F 16 16 
              rii++;                                                                    //00CF : A5 00 60 41 75 14 16 
            }
            return ret;   
            
            ret == 0 implies no responses available
            
             */
        }

        public List<ConversationNode> getStartNodes()
        {
            var output = new List<ConversationNode>();

            foreach (var startNode in startNodes)
            {
                foreach (var conNode in allNodes)
                {
                    if (startNode.ID == conNode.resource.ID)
                    {
                        output.Add(conNode);
                        if (output.Count >= startNodes.Count)
                        {
                            return output;
                        }
                    }
                }
            }
            Debug.Log("ConversationTopic.getStartNodes : failed to return all the start nodes");
            return null;
        }

        public bool requirementsMet(PlayerCharacter p)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(p)) return false;
            }
            return true;
        }
    }

    [Serializable]
    public class ConversationNode //What the NPC says
    {
        public SBResource resource;
        public List<SBResource> responses = new List<SBResource>();
        public SBLocalizedString textLocStr;
    }

    [Serializable]
    public class ConversationResponse //What the player replies with
    {
        public List<SBResource> conversations = new List<SBResource>();
        public SBResource resource;
        public SBLocalizedString textLocStr;
    }
}

/*

//==============================================================================
//  Conversation_Topic
//==============================================================================

class Conversation_Topic extends Content_Type
    native
    abstract
    dependsOn(Game_Pawn,Game_Controller,Game_Conversation,Content_Event)
  ;

  enum EConversationType {
    ECT_None ,
    ECT_Free ,
    ECT_Greeting ,
    ECT_Provide ,
    ECT_Mid ,
    ECT_Finish ,
    ECT_Talk ,
    ECT_LastWords ,
    ECT_Victory ,
    ECT_Thanks ,
    ECT_Deliver 
  };

  var (Type) byte TopicType;
  var (Conversation) editinline array<Conversation_Node> Conversations;
  var (Conversation) LocalizedString TopicText;
  var (Conversation) int ButtonImage;
  var (Requirements) int Priority;
  var (Requirements) array<Content_Requirement> Requirements;
  var (Requirements) string ScriptReference;
  var (Requirements) bool AvailableTopic;
  var (Requirements) bool PublicTopic;
  var (Events) editinline array<Content_Event> Events;
  var (Banner) byte Emote;
  var (Banner) LocalizedString Chat;
  var (Banner) Texture Icon;
  var (Banner) Sound Speech;
  var (Banner) class<Emitter> EmitterClass;
  var (Banner) float BannerTime;
  var (Minimap) float MinimapRange;


  function Content_Type GetContext() {
    return None;                                                                //0000 : 04 2A 
    //04 2A 04 0B 47 
  }


  function sv_Start(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local export editinline Conversation_Node bestConv;
    sv_OnStart(aSpeaker,aPartner);                                              //0000 : 1B A1 07 00 00 00 78 20 77 14 00 F8 1F 77 14 16 
    bestConv = GetConversationNode(aSpeaker,aPartner);                          //0010 : 0F 00 70 08 C5 0F 1C B0 83 7A 14 00 78 20 77 14 00 F8 1F 77 14 16 
    if (bestConv != None) {                                                     //0026 : 07 65 00 77 00 70 08 C5 0F 2A 16 
      Game_Controller(aSpeaker.Controller).ConversationControl.Converse(aPartner,self,bestConv);//0031 : 19 19 2E 80 02 77 01 19 00 78 20 77 14 05 00 04 01 18 ED 4B 11 05 00 04 01 F0 4B CA 0F 11 00 00 1C 68 D6 7B 14 00 F8 1F 77 14 17 00 70 08 C5 0F 16 
    } else {                                                                    //0062 : 06 B4 00 
      SBLog("Conversation warning: found no follow conversation after topic change"
        @ string(self));//0065 : 62 82 A8 1F 43 6F 6E 76 65 72 73 61 74 69 6F 6E 20 77 61 72 6E 69 6E 67 3A 20 66 6F 75 6E 64 20 6E 6F 20 66 6F 6C 6C 6F 77 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 61 66 74 65 72 20 74 6F 70 69 63 20 63 68 61 6E 67 65 00 39 56 17 16 16 
    }
    //1B A1 07 00 00 00 78 20 77 14 00 F8 1F 77 14 16 0F 00 70 08 C5 0F 1C B0 83 7A 14 00 78 20 77 14 
    //00 F8 1F 77 14 16 07 65 00 77 00 70 08 C5 0F 2A 16 19 19 2E 80 02 77 01 19 00 78 20 77 14 05 00 
    //04 01 18 ED 4B 11 05 00 04 01 F0 4B CA 0F 11 00 00 1C 68 D6 7B 14 00 F8 1F 77 14 17 00 70 08 C5 
    //0F 16 06 B4 00 62 82 A8 1F 43 6F 6E 76 65 72 73 61 74 69 6F 6E 20 77 61 72 6E 69 6E 67 3A 20 66 
    //6F 75 6E 64 20 6E 6F 20 66 6F 6C 6C 6F 77 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 61 66 74 65 
    //72 20 74 6F 70 69 63 20 63 68 61 6E 67 65 00 39 56 17 16 16 04 0B 47 
  }


  event bool sv_OnFinish(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local int eventI;
    eventI = 0;                                                                 //0000 : 0F 00 A8 11 CA 0F 25 
    while (eventI < Events.Length) {                                            //0007 : 07 7A 00 96 00 A8 11 CA 0F 37 01 F8 9F C9 0F 16 
      if (Events[eventI] != None) {                                             //0017 : 07 70 00 77 10 00 A8 11 CA 0F 01 F8 9F C9 0F 2A 16 
        if (!Events[eventI].sv_CanExecute(aSpeaker,aPartner)) {                 //0028 : 07 51 00 81 19 10 00 A8 11 CA 0F 01 F8 9F C9 0F 10 00 04 1B 76 07 00 00 00 F0 AB 77 14 00 50 6B C5 0F 16 16 
          return False;                                                         //004C : 04 28 
          goto jl0070;                                                          //004E : 06 70 00 
        }
        Events[eventI].sv_Execute(aSpeaker,aPartner);                           //0051 : 19 10 00 A8 11 CA 0F 01 F8 9F C9 0F 10 00 00 1B 85 07 00 00 00 F0 AB 77 14 00 50 6B C5 0F 16 
      }
      eventI++;                                                                 //0070 : A5 00 A8 11 CA 0F 16 
    }
    return True;                                                                //007A : 04 27 
    //0F 00 A8 11 CA 0F 25 07 7A 00 96 00 A8 11 CA 0F 37 01 F8 9F C9 0F 16 07 70 00 77 10 00 A8 11 CA 
    //0F 01 F8 9F C9 0F 2A 16 07 51 00 81 19 10 00 A8 11 CA 0F 01 F8 9F C9 0F 10 00 04 1B 76 07 00 00 
    //00 F0 AB 77 14 00 50 6B C5 0F 16 16 04 28 06 70 00 19 10 00 A8 11 CA 0F 01 F8 9F C9 0F 10 00 00 
    //1B 85 07 00 00 00 F0 AB 77 14 00 50 6B C5 0F 16 A5 00 A8 11 CA 0F 16 06 07 00 04 27 04 0B 47 
  }


  event sv_OnStart(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    //04 0B 47 
  }


  protected final native function bool CheckPawn(Game_Pawn aSpeaker,Game_Pawn aPartner);


  protected final native function Conversation_Node GetConversationNode(Game_Pawn aSpeaker,Game_Pawn aPartner);


  final native function Conversation_Node CheckConversation(Game_Pawn aSpeaker,Game_Pawn aPartner);


  function string GetText() {
    return TopicText.Text;                                                      //0000 : 04 36 18 57 4B 11 01 68 6F 7C 14 
    //04 36 18 57 4B 11 01 68 6F 7C 14 04 0B 47 
  }



*/