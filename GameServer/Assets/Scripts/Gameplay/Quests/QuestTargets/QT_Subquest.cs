//==============================================================================
//  QT_Subquest
//==============================================================================

using Database.Static;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Subquest : QuestTarget
    {
        public SBResource SubQuestID;

        public override int GetCompletedProgressValue() { return 1; }
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 4F 00 7A 19 00 D8 87 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      if (SubQuest != None) {                                                   //0016 : 07 42 00 77 01 F0 D2 8B 16 2A 16 
        return SubQuest.GetActiveText(aItem.SubItem);                           //0021 : 04 19 01 F0 D2 8B 16 14 00 00 1B 78 05 00 00 19 00 D8 87 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //003F : 06 4C 00 
        return "?Quest?";                                                       //0042 : 04 1F 3F 51 75 65 73 74 3F 00 
      }
    } else {                                                                    //004C : 06 5B 00 
      return Super.GetActiveText(aItem);                                        //004F : 04 1C 28 7C 74 14 00 D8 87 8B 16 16 
    }
    //07 4F 00 7A 19 00 D8 87 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 07 42 00 77 01 F0 D2 8B 16 2A 
    //16 04 19 01 F0 D2 8B 16 14 00 00 1B 78 05 00 00 19 00 D8 87 8B 16 05 00 04 01 68 BF 73 14 16 06 
    //4C 00 04 1F 3F 51 75 65 73 74 3F 00 06 5B 00 04 1C 28 7C 74 14 00 D8 87 8B 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_SubQuestText.Text;                //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1C 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1C 88 14 04 0B 47 
  }

*/