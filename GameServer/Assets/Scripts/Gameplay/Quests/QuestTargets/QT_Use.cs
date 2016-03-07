//==============================================================================
//  QT_Use
//==============================================================================

using Common;
using Gameplay.Items;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Use : QuestTarget
    {
        public int Amount;

        public Item_Type Item;
        public ERadialMenuOptions Option; //menuOption
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "O") {                                                     //0000 : 07 50 00 7A 19 00 C0 2A 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
      if (Item != None) {                                                       //0016 : 07 42 00 77 01 08 BC 8B 16 2A 16 
        return Item.GetActiveText(aItem.SubItem);                               //0021 : 04 19 01 08 BC 8B 16 14 00 00 1B 78 05 00 00 19 00 C0 2A 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //003F : 06 4D 00 
        return "?Object?";                                                      //0042 : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
      }
    } else {                                                                    //004D : 06 81 00 
      if (aItem.Tag == "Q") {                                                   //0050 : 07 75 00 7A 19 00 C0 2A 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
        return "" @ string(Amount);                                             //0066 : 04 A8 1F 00 39 53 01 F0 C3 8B 16 16 
      } else {                                                                  //0072 : 06 81 00 
        return Super.GetActiveText(aItem);                                      //0075 : 04 1C 28 7C 74 14 00 C0 2A 8B 16 16 
      }
    }
    //07 50 00 7A 19 00 C0 2A 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 42 00 77 01 08 BC 8B 16 2A 
    //16 04 19 01 08 BC 8B 16 14 00 00 1B 78 05 00 00 19 00 C0 2A 8B 16 05 00 04 01 68 BF 73 14 16 06 
    //4D 00 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 81 00 07 75 00 7A 19 00 C0 2A 8B 16 05 00 00 01 80 20 
    //C5 0F 1F 51 00 16 04 A8 1F 00 39 53 01 F0 C3 8B 16 16 06 81 00 04 1C 28 7C 74 14 00 C0 2A 8B 16 
    //16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 F0 C3 8B 16 26 16 
    }
    //07 22 00 97 01 F0 C3 8B 16 26 16 0E 61 43 00 30 FD 8C 16 70 39 53 00 10 F9 8C 16 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_UseText.Text;                     //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1E 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1E 88 14 04 0B 47 
  }


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    if (aObject == None) {                                                      //0000 : 07 1D 00 72 00 90 0A 8D 16 2A 16 
      aSubOptions[aSubOptions.Length] = Option;                                 //000B : 0F 10 37 00 78 C1 8B 16 00 78 C1 8B 16 01 B8 0D 8D 16 
    }
    //07 1D 00 72 00 90 0A 8D 16 2A 16 0F 10 37 00 78 C1 8B 16 00 78 C1 8B 16 01 B8 0D 8D 16 04 0B 47 
    //
  }


*/