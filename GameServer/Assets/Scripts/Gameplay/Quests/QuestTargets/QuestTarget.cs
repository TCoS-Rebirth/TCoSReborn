using System;
using System.Collections.Generic;
using Database.Static;
using Gameplay.Events;
using UnityEngine;
using Gameplay.Entities;
using Common;

namespace Gameplay.Quests.QuestTargets
{
    [Serializable]
    public class QuestTarget : ScriptableObject
    {
        public SBResource resource;

        public bool AlwaysVisible;
        public List<Content_Event> CompleteEvents;
        public SBLocalizedString Description;
        public List<SBResource> Pretargets;

        //TODO: method works out what progress value denotes that an input target is completed
        public virtual int GetCompletedProgressValue() { return 1;}

        /// <summary>
        /// Compares new progress value to completed progress,
        /// and if the target is complete, calls onComplete();
        /// </summary>
        /// <param name="progressValue"></param>
        public void onAdvance(PlayerCharacter p, int progressValue)
        {
            if (isComplete(progressValue)) { tryComplete(p); }
        }

        public bool isComplete(int progressValue)
        {
            if (progressValue >= GetCompletedProgressValue()) return true;
            else return false;
        }

        /// <summary>
        /// Try to execute all the target's Complete Events
        /// </summary>
        protected bool tryComplete(PlayerCharacter p)
        {
            bool success = true;
            foreach (var completeEvent in CompleteEvents)
            {
                if (!completeEvent.TryExecute(null, p))
                {
                    p.ReceiveChatMessage(completeEvent.name, "Failed to execute completion event!", EGameChatRanges.GCR_SYSTEM);
                    success = false;
                }
            }

            return success;
        }

        //ConversationTopic attachedTopic;
    }
}

/*
//==============================================================================
//  Quest_Target
//==============================================================================

class Quest_Target extends Content_Type
    native
    abstract
    exportstructs
    collapsecategories
    editinlinenew
    dependsOn(Game_ActiveTextItem,Quest_Type,Game_Pawn,Content_Event)
  ;

  struct QuestInventory {
      var export editinline Item_Type Item;
      var int Amount;

  };


  var (Target) editinline array<Quest_Target> Pretargets;
  var (Target) bool AlwaysVisible;
  var (Target) editinline array<Content_Event> CompleteEvents;
  var (Target) LocalizedString Description;


  final native function int GetCompletedProgressValue();


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    //04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    //04 0B 47 
  }


  event string GetActiveText(Game_ActiveTextItem aItem) {
    local export editinline Quest_Type quest;
    if (aItem == None) {                                                        //0000 : 07 16 00 72 00 E0 7C 74 14 2A 16 
      return GetDescription(0);                                                 //000B : 04 1C 10 85 75 14 25 16 
    } else {                                                                    //0013 : 06 8F 00 
      if (aItem.Tag == "D") {                                                   //0016 : 07 44 00 7A 19 00 E0 7C 74 14 05 00 00 01 80 20 C5 0F 1F 44 00 16 
        return GetDescription(aItem.Ordinality);                                //002C : 04 1C 10 85 75 14 19 00 E0 7C 74 14 05 00 04 01 30 BF C9 0F 16 
      } else {                                                                  //0041 : 06 8F 00 
        if (aItem.Tag == "Q") {                                                 //0044 : 07 8F 00 7A 19 00 E0 7C 74 14 05 00 00 01 80 20 C5 0F 1F 51 00 16 
          quest = GetQuest();                                                   //005A : 0F 00 A0 9C 76 14 1C 10 D5 74 14 16 
          if (quest != None) {                                                  //0066 : 07 8F 00 77 00 A0 9C 76 14 2A 16 
            return quest.GetActiveText(aItem.SubItem);                          //0071 : 04 19 00 A0 9C 76 14 14 00 00 1B 78 05 00 00 19 00 E0 7C 74 14 05 00 04 01 68 BF 73 14 16 
          }
        }
      }
    }
    return Super.GetActiveText(aItem);                                          //008F : 04 1C 18 5A CB 0F 00 E0 7C 74 14 16 
    //07 16 00 72 00 E0 7C 74 14 2A 16 04 1C 10 85 75 14 25 16 06 8F 00 07 44 00 7A 19 00 E0 7C 74 14 
    //05 00 00 01 80 20 C5 0F 1F 44 00 16 04 1C 10 85 75 14 19 00 E0 7C 74 14 05 00 04 01 30 BF C9 0F 
    //16 06 8F 00 07 8F 00 7A 19 00 E0 7C 74 14 05 00 00 01 80 20 C5 0F 1F 51 00 16 0F 00 A0 9C 76 14 
    //1C 10 D5 74 14 16 07 8F 00 77 00 A0 9C 76 14 2A 16 04 19 00 A0 9C 76 14 14 00 00 1B 78 05 00 00 
    //19 00 E0 7C 74 14 05 00 04 01 68 BF 73 14 16 04 1C 18 5A CB 0F 00 E0 7C 74 14 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return "objective default descriptions must be overriden";                  //0000 : 04 1F 6F 62 6A 65 63 74 69 76 65 20 64 65 66 61 75 6C 74 20 64 65 73 63 72 69 70 74 69 6F 6E 73 20 6D 75 73 74 20 62 65 20 6F 76 65 72 72 69 64 65 6E 00 
    //04 1F 6F 62 6A 65 63 74 69 76 65 20 64 65 66 61 75 6C 74 20 64 65 73 63 72 69 70 74 69 6F 6E 73 
    //20 6D 75 73 74 20 62 65 20 6F 76 65 72 72 69 64 65 6E 00 04 0B 47 
  }


  final function string GetDescription(int aProgress) {
    local string ret;
    if (Len(Description.Text) > 0) {                                            //0000 : 07 25 00 97 7D 36 18 57 4B 11 01 30 37 79 14 16 25 16 
      ret = Description.Text;                                                   //0012 : 0F 00 C8 85 75 14 36 18 57 4B 11 01 30 37 79 14 
    } else {                                                                    //0022 : 06 31 00 
      ret = GetDefaultDescription();                                            //0025 : 0F 00 C8 85 75 14 1B 42 10 00 00 16 
    }
    AppendProgressText(ret,aProgress);                                          //0031 : 1B 38 10 00 00 00 C8 85 75 14 00 C0 61 7C 14 16 
    return ret;                                                                 //0041 : 04 00 C8 85 75 14 
    //07 25 00 97 7D 36 18 57 4B 11 01 30 37 79 14 16 25 16 0F 00 C8 85 75 14 36 18 57 4B 11 01 30 37 
    //79 14 06 31 00 0F 00 C8 85 75 14 1B 42 10 00 00 16 1B 38 10 00 00 00 C8 85 75 14 00 C0 61 7C 14 
    //16 04 00 C8 85 75 14 04 0B 47 
  }


  event bool sv_OnComplete(Game_Pawn aPawn,optional Game_Pawn aTargetPawn) {
    local int eventI;
    if (aTargetPawn == None
      || !aTargetPawn.IsA('Game_NPCPawn')) {        //0000 : 07 2E 00 84 72 00 20 CC 74 14 2A 16 18 14 00 81 19 00 20 CC 74 14 08 00 04 61 2F 21 D0 08 00 00 16 16 16 
      aTargetPawn = aPawn;                                                      //0023 : 0F 00 20 CC 74 14 00 F8 99 76 14 
    }
    eventI = 0;                                                                 //002E : 0F 00 58 46 74 14 25 
    while (eventI < CompleteEvents.Length) {                                    //0035 : 07 A8 00 96 00 58 46 74 14 37 01 A0 80 75 14 16 
      if (CompleteEvents[eventI] != None) {                                     //0045 : 07 9E 00 77 10 00 58 46 74 14 01 A0 80 75 14 2A 16 
        if (!CompleteEvents[eventI].sv_CanExecute(aTargetPawn,aPawn)) {         //0056 : 07 7F 00 81 19 10 00 58 46 74 14 01 A0 80 75 14 10 00 04 1B 76 07 00 00 00 20 CC 74 14 00 F8 99 76 14 16 16 
          return False;                                                         //007A : 04 28 
          goto jl009E;                                                          //007C : 06 9E 00 
        }
        CompleteEvents[eventI].sv_Execute(aTargetPawn,aPawn);                   //007F : 19 10 00 58 46 74 14 01 A0 80 75 14 10 00 00 1B 85 07 00 00 00 20 CC 74 14 00 F8 99 76 14 16 
      }
      eventI++;                                                                 //009E : A5 00 58 46 74 14 16 
    }
    return True;                                                                //00A8 : 04 27 
    //07 2E 00 84 72 00 20 CC 74 14 2A 16 18 14 00 81 19 00 20 CC 74 14 08 00 04 61 2F 21 D0 08 00 00 
    //16 16 16 0F 00 20 CC 74 14 00 F8 99 76 14 0F 00 58 46 74 14 25 07 A8 00 96 00 58 46 74 14 37 01 
    //A0 80 75 14 16 07 9E 00 77 10 00 58 46 74 14 01 A0 80 75 14 2A 16 07 7F 00 81 19 10 00 58 46 74 
    //14 01 A0 80 75 14 10 00 04 1B 76 07 00 00 00 20 CC 74 14 00 F8 99 76 14 16 16 04 28 06 9E 00 19 
    //10 00 58 46 74 14 01 A0 80 75 14 10 00 00 1B 85 07 00 00 00 20 CC 74 14 00 F8 99 76 14 16 A5 00 
    //58 46 74 14 16 06 35 00 04 27 04 0B 47 
  }


  final native function Quest_Type GetQuest();


  final native function int GetIndex();


  final native function bool Active(int aValue);


  final native function bool NearlyDone(int aValue);


  final native function bool Failed(int aValue);


  final native function bool Check(int aValue);


  final native function int ComputeValue(Game_Pawn aPawn);


  final native function bool sv_CanAccept(Game_Pawn aPawn);



*/