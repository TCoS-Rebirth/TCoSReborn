//==============================================================================
//  QT_Take
//==============================================================================

using Common;
using Database.Static;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Take : QuestTarget
    {
        int Amount; //Amount to take?

        public Content_Inventory Cargo;
        public ERadialMenuOptions Option; //Radial menu option?
        public SBLocalizedString SourceDescription;
        public string SourceTag;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 24 00 7A 19 00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return SourceDescription.Text;                                            //0016 : 04 36 18 57 4B 11 01 98 C9 8B 16 
    } else {                                                                    //0021 : 06 8C 00 
      if (aItem.Tag == "O") {                                                   //0024 : 07 5B 00 7A 19 00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
        return Cargo.GetActiveText(aItem.SubItem);                              //003A : 04 19 01 C8 E3 8C 16 14 00 00 1B 78 05 00 00 19 00 88 6F 18 0C 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //0058 : 06 8C 00 
        if (aItem.Tag == "Q") {                                                 //005B : 07 80 00 7A 19 00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 51 00 16 
          return "" @ string(Amount);                                           //0071 : 04 A8 1F 00 39 53 01 50 CC 8B 16 16 
        } else {                                                                //007D : 06 8C 00 
          return Super.GetActiveText(aItem);                                    //0080 : 04 1C 28 7C 74 14 00 88 6F 18 0C 16 
        }
      }
    }
    //07 24 00 7A 19 00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 36 18 57 4B 11 01 98 C9 8B 
    //16 06 8C 00 07 5B 00 7A 19 00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 4F 00 16 04 19 01 C8 E3 8C 
    //16 14 00 00 1B 78 05 00 00 19 00 88 6F 18 0C 05 00 04 01 68 BF 73 14 16 06 8C 00 07 80 00 7A 19 
    //00 88 6F 18 0C 05 00 00 01 80 20 C5 0F 1F 51 00 16 04 A8 1F 00 39 53 01 50 CC 8B 16 16 06 8C 00 
    //04 1C 28 7C 74 14 00 88 6F 18 0C 16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 50 CC 8B 16 26 16 
    }
    //07 22 00 97 01 50 CC 8B 16 26 16 0E 61 43 00 B0 6B C6 0F 70 39 53 00 C0 6A C6 0F 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_TakeText.Text;                    //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1D 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1D 88 14 04 0B 47 
  }


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    local Actor act;
    act = Actor(aObject);                                                       //0000 : 0F 00 60 92 8B 16 2E 10 81 D2 00 00 B8 D2 8C 16 
    if (act != None && act.Tag == SourceTag) {                                  //0010 : 07 47 00 82 77 00 60 92 8B 16 2A 16 18 16 00 FE 19 00 60 92 8B 16 05 00 04 01 A0 29 4C 11 01 20 C8 8B 16 16 16 
      aSubOptions[aSubOptions.Length] = Option;                                 //0035 : 0F 10 37 00 78 CF 8B 16 00 78 CF 8B 16 01 28 D4 8C 16 
    }
    //0F 00 60 92 8B 16 2E 10 81 D2 00 00 B8 D2 8C 16 07 47 00 82 77 00 60 92 8B 16 2A 16 18 16 00 FE 
    //19 00 60 92 8B 16 05 00 04 01 A0 29 4C 11 01 20 C8 8B 16 16 16 0F 10 37 00 78 CF 8B 16 00 78 CF 
    //8B 16 01 28 D4 8C 16 04 0B 47 
  }


*/