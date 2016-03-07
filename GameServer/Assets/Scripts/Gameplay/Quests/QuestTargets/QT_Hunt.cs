//==============================================================================
//  QT_Hunt
//==============================================================================

using Database.Static;
using Gameplay.Entities.NPCs;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Hunt : QuestTarget
    {
        public int Amount;

        public SBResource Target;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 37 00 7A 19 00 A0 3B 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return Target.GetActiveText(aItem.SubItem);                               //0016 : 04 19 01 C0 7B 8C 16 14 00 00 1B 78 05 00 00 19 00 A0 3B 8B 16 05 00 04 01 68 BF 73 14 16 
    } else {                                                                    //0034 : 06 68 00 
      if (aItem.Tag == "Q") {                                                   //0037 : 07 5C 00 7A 19 00 A0 3B 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
        return "" @ string(Amount);                                             //004D : 04 A8 1F 00 39 53 01 98 3A 8C 16 16 
      } else {                                                                  //0059 : 06 68 00 
        return Super.GetActiveText(aItem);                                      //005C : 04 1C 28 7C 74 14 00 A0 3B 8B 16 16 
      }
    }
    //07 37 00 7A 19 00 A0 3B 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 19 01 C0 7B 8C 16 14 00 00 
    //1B 78 05 00 00 19 00 A0 3B 8B 16 05 00 04 01 68 BF 73 14 16 06 68 00 07 5C 00 7A 19 00 A0 3B 8B 
    //16 05 00 00 01 80 20 C5 0F 1F 51 00 16 04 A8 1F 00 39 53 01 98 3A 8C 16 16 06 68 00 04 1C 28 7C 
    //74 14 00 A0 3B 8B 16 16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 98 3A 8C 16 26 16 
    }
    //07 22 00 97 01 98 3A 8C 16 26 16 0E 61 43 00 70 7D 8C 16 70 39 53 00 E8 7D 8C 16 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_HuntText.Text;                    //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1A 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1A 88 14 04 0B 47 
  }

*/