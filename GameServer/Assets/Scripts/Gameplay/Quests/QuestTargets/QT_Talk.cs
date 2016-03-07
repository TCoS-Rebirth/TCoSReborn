//==============================================================================
//  QT_Talk
//==============================================================================

using System;
using Database.Static;
using Gameplay.Entities.NPCs;
using UnityEngine;

namespace Gameplay.Quests.QuestTargets
{
    [Serializable]
    public class QT_Talk : QuestTarget
    {
        public SBResource PersonID;
        public SBResource TopicID;
    }
}

/*
  protected final native function NPC_Type GetTarget();


  event string GetActiveText(Game_ActiveTextItem aItem) {
    local export editinline NPC_Type t;
    if (aItem.Tag == "T") {                                                     //0000 : 07 5C 00 7A 19 00 08 69 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      t = GetTarget();                                                          //0016 : 0F 00 88 68 8B 16 1C 08 82 8C 16 16 
      if (t != None) {                                                          //0022 : 07 4E 00 77 00 88 68 8B 16 2A 16 
        return t.GetActiveText(aItem.SubItem);                                  //002D : 04 19 00 88 68 8B 16 14 00 00 1B 78 05 00 00 19 00 08 69 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //004B : 06 59 00 
        return "?Target?";                                                      //004E : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    } else {                                                                    //0059 : 06 68 00 
      return Super.GetActiveText(aItem);                                        //005C : 04 1C 28 7C 74 14 00 08 69 8B 16 16 
    }
    //07 5C 00 7A 19 00 08 69 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 0F 00 88 68 8B 16 1C 08 82 8C 
    //16 16 07 4E 00 77 00 88 68 8B 16 2A 16 04 19 00 88 68 8B 16 14 00 00 1B 78 05 00 00 19 00 08 69 
    //8B 16 05 00 04 01 68 BF 73 14 16 06 59 00 04 1F 3F 54 61 72 67 65 74 3F 00 06 68 00 04 1C 28 7C 
    //74 14 00 08 69 8B 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_TalkText.Text;                    //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1D 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1D 88 14 04 0B 47 
  }

*/