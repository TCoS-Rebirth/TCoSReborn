//==============================================================================
//  QT_Challenge
//==============================================================================

using Common;
using Gameplay.Items;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Challenge : QuestTarget
    {
        public string CompletionTag;
        public string FailureTag;
        public Item_Type Pass; //Item_Type player needs to fulfil target?

        public MapIDs TargetWorld; //TODO:verify
    }
}

/*

  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "O") {                                                     //0000 : 07 50 00 7A 19 00 48 A7 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
      if (Pass != None) {                                                       //0016 : 07 42 00 77 01 A0 4C 8C 16 2A 16 
        return Pass.GetActiveText(aItem.SubItem);                               //0021 : 04 19 01 A0 4C 8C 16 14 00 00 1B 78 05 00 00 19 00 48 A7 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //003F : 06 4D 00 
        return "?Object?";                                                      //0042 : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
      }
    } else {                                                                    //004D : 06 5C 00 
      return Super.GetActiveText(aItem);                                        //0050 : 04 1C 28 7C 74 14 00 48 A7 8B 16 16 
    }
    //07 50 00 7A 19 00 48 A7 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 42 00 77 01 A0 4C 8C 16 2A 
    //16 04 19 01 A0 4C 8C 16 14 00 00 1B 78 05 00 00 19 00 48 A7 8B 16 05 00 04 01 68 BF 73 14 16 06 
    //4D 00 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 5C 00 04 1C 28 7C 74 14 00 48 A7 8B 16 16 04 0B 47 
  }


*/