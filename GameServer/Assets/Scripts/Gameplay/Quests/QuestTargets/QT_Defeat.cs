//==============================================================================
//  QT_Defeat
//==============================================================================

using Database.Static;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Defeat : QuestTarget
    {
        public float DefeatFraction;
        public SBResource LastWordsConvID;
        public SBResource NpcTargetID;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 37 00 7A 19 00 50 64 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return Target.GetActiveText(aItem.SubItem);                               //0016 : 04 19 01 18 80 8C 16 14 00 00 1B 78 05 00 00 19 00 50 64 8B 16 05 00 04 01 68 BF 73 14 16 
    } else {                                                                    //0034 : 06 43 00 
      return Super.GetActiveText(aItem);                                        //0037 : 04 1C 28 7C 74 14 00 50 64 8B 16 16 
    }
    //07 37 00 7A 19 00 50 64 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 19 01 18 80 8C 16 14 00 00 
    //1B 78 05 00 00 19 00 50 64 8B 16 05 00 04 01 68 BF 73 14 16 06 43 00 04 1C 28 7C 74 14 00 50 64 
    //8B 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_DefeatText.Text;                  //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 17 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 17 88 14 04 0B 47 
  }

*/