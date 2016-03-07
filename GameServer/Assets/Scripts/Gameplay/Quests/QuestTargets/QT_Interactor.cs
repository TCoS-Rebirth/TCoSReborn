//==============================================================================
//  QT_Interactor
//==============================================================================

using Common;
using Database.Static;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Interactor : QuestTarget
    {
        public int Amount; //Amount of item picked up?
        public ERadialMenuOptions Option; //Interact menu option?
        public SBLocalizedString TargetDescription;

        public string TargetTag;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 24 00 7A 19 00 F0 AD 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return TargetDescription.Text;                                            //0016 : 04 36 18 57 4B 11 01 B0 7C C4 0F 
    } else {                                                                    //0021 : 06 55 00 
      if (aItem.Tag == "Q") {                                                   //0024 : 07 49 00 7A 19 00 F0 AD 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
        return "" @ string(Amount);                                             //003A : 04 A8 1F 00 39 53 01 38 7C C4 0F 16 
      } else {                                                                  //0046 : 06 55 00 
        return Super.GetActiveText(aItem);                                      //0049 : 04 1C 28 7C 74 14 00 F0 AD 8B 16 16 
      }
    }
    //07 24 00 7A 19 00 F0 AD 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 36 18 57 4B 11 01 B0 7C C4 
    //0F 06 55 00 07 49 00 7A 19 00 F0 AD 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 04 A8 1F 00 39 53 
    //01 38 7C C4 0F 16 06 55 00 04 1C 28 7C 74 14 00 F0 AD 8B 16 16 04 0B 47 
  }


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    local Actor act;
    act = Actor(aObject);                                                       //0000 : 0F 00 38 AB 8B 16 2E 10 81 D2 00 00 70 74 8C 16 
    if (act != None && act.Tag == TargetTag) {                                  //0010 : 07 47 00 82 77 00 38 AB 8B 16 2A 16 18 16 00 FE 19 00 38 AB 8B 16 05 00 04 01 A0 29 4C 11 01 68 7E C4 0F 16 16 
      aSubOptions[aSubOptions.Length] = Option;                                 //0035 : 0F 10 37 00 C0 3E 8C 16 00 C0 3E 8C 16 01 A0 70 8C 16 
    }
    //0F 00 38 AB 8B 16 2E 10 81 D2 00 00 70 74 8C 16 07 47 00 82 77 00 38 AB 8B 16 2A 16 18 16 00 FE 
    //19 00 38 AB 8B 16 05 00 04 01 A0 29 4C 11 01 68 7E C4 0F 16 16 0F 10 37 00 C0 3E 8C 16 00 C0 3E 
    //8C 16 01 A0 70 8C 16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 38 7C C4 0F 26 16 
    }
    //07 22 00 97 01 38 7C C4 0F 26 16 0E 61 43 00 50 72 8C 16 70 39 53 00 C8 72 8C 16 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_InteractText.Text;                //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1A 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1A 88 14 04 0B 47 
  }



*/