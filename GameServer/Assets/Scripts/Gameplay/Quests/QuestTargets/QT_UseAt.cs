//==============================================================================
//  QT_UseAt
//==============================================================================

using Common;
using Gameplay.Items;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_UseAt : QuestTarget
    {
        int Amount;

        Item_Type Item;
        int LocationDescription;
        ERadialMenuOptions Option; //?
        string UseLocationTag;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 24 00 7A 19 00 30 9D 29 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      return LocationDescription.Text;                                          //0016 : 04 36 18 57 4B 11 01 98 EE 8C 16 
    } else {                                                                    //0021 : 06 A5 00 
      if (aItem.Tag == "O") {                                                   //0024 : 07 74 00 7A 19 00 30 9D 29 0C 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
        if (Item != None) {                                                     //003A : 07 66 00 77 01 E8 C4 8B 16 2A 16 
          return Item.GetActiveText(aItem.SubItem);                             //0045 : 04 19 01 E8 C4 8B 16 14 00 00 1B 78 05 00 00 19 00 30 9D 29 0C 05 00 04 01 68 BF 73 14 16 
        } else {                                                                //0063 : 06 71 00 
          return "?Object?";                                                    //0066 : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
        }
      } else {                                                                  //0071 : 06 A5 00 
        if (aItem.Tag == "Q") {                                                 //0074 : 07 99 00 7A 19 00 30 9D 29 0C 05 00 00 01 80 20 C5 0F 1F 51 00 16 
          return "" @ string(Amount);                                           //008A : 04 A8 1F 00 39 53 01 08 6A 8B 16 16 
        } else {                                                                //0096 : 06 A5 00 
          return Super.GetActiveText(aItem);                                    //0099 : 04 1C 28 7C 74 14 00 30 9D 29 0C 16 
        }
      }
    }
    //07 24 00 7A 19 00 30 9D 29 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 04 36 18 57 4B 11 01 98 EE 8C 
    //16 06 A5 00 07 74 00 7A 19 00 30 9D 29 0C 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 66 00 77 01 E8 
    //C4 8B 16 2A 16 04 19 01 E8 C4 8B 16 14 00 00 1B 78 05 00 00 19 00 30 9D 29 0C 05 00 04 01 68 BF 
    //73 14 16 06 71 00 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 A5 00 07 99 00 7A 19 00 30 9D 29 0C 05 00 
    //00 01 80 20 C5 0F 1F 51 00 16 04 A8 1F 00 39 53 01 08 6A 8B 16 16 06 A5 00 04 1C 28 7C 74 14 00 
    //30 9D 29 0C 16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 08 6A 8B 16 26 16 
    }
    //07 22 00 97 01 08 6A 8B 16 26 16 0E 61 43 00 40 0C 8D 16 70 39 53 00 C8 0B 8D 16 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_UseAtText.Text;                   //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1E 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 1E 88 14 04 0B 47 
  }


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    local Quest_Trigger Area;
    if (aObject == None) {                                                      //0000 : 07 95 00 72 00 D8 06 8D 16 2A 16 
      foreach aPlayerPawn.TouchingActors(Class'Quest_Trigger',Area) {           //000B : 2F 19 00 C0 BF 8B 16 0D 00 00 61 33 20 90 B0 AA 01 00 C0 8B 8B 16 16 94 00 
        UCASSERT(Area.Inside(aPlayerPawn),"Touching quest trigger, but not inside it");//0024 : 1C 48 12 49 11 19 00 C0 8B 8B 16 0B 00 04 1C 88 A1 7F 14 00 C0 BF 8B 16 16 1F 54 6F 75 63 68 69 6E 67 20 71 75 65 73 74 20 74 72 69 67 67 65 72 2C 20 62 75 74 20 6E 6F 74 20 69 6E 73 69 64 65 20 69 74 00 16 
        if (Area.Tag == UseLocationTag) {                                       //0069 : 07 93 00 FE 19 00 C0 8B 8B 16 05 00 04 01 A0 29 4C 11 01 00 E7 8C 16 16 
          aSubOptions[aSubOptions.Length] = Option;                             //0081 : 0F 10 37 00 78 C2 8B 16 00 78 C2 8B 16 01 38 F3 8C 16 
        }
      }
    }
    //07 95 00 72 00 D8 06 8D 16 2A 16 2F 19 00 C0 BF 8B 16 0D 00 00 61 33 20 90 B0 AA 01 00 C0 8B 8B 
    //16 16 94 00 1C 48 12 49 11 19 00 C0 8B 8B 16 0B 00 04 1C 88 A1 7F 14 00 C0 BF 8B 16 16 1F 54 6F 
    //75 63 68 69 6E 67 20 71 75 65 73 74 20 74 72 69 67 67 65 72 2C 20 62 75 74 20 6E 6F 74 20 69 6E 
    //73 69 64 65 20 69 74 00 16 07 93 00 FE 19 00 C0 8B 8B 16 05 00 04 01 A0 29 4C 11 01 00 E7 8C 16 
    //16 0F 10 37 00 78 C2 8B 16 00 78 C2 8B 16 01 38 F3 8C 16 31 30 04 0B 47 
  }



*/