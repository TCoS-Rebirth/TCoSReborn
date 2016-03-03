//==============================================================================
//  QT_Reach
//==============================================================================

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Reach : QuestTarget
    {
        int GoalDescription;

        string GoalTag;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 24 00 7A 19 00 20 45 8C 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return GoalDescription.Text;                                              //0016 : 04 36 18 57 4B 11 01 28 5F 8C 16 
    } else {                                                                    //0021 : 06 30 00 
      return Super.GetActiveText(aItem);                                        //0024 : 04 1C 28 7C 74 14 00 20 45 8C 16 16 
    }
    //07 24 00 7A 19 00 20 45 8C 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 36 18 57 4B 11 01 28 5F 8C 
    //16 06 30 00 04 1C 28 7C 74 14 00 20 45 8C 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_ReachText.Text;                   //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1C 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1C 88 14 04 0B 47 
  }

*/