//==============================================================================
//  QT_UseOn
//==============================================================================

using Common;
using Database.Static;
using Gameplay.Entities.NPCs;
using Gameplay.Items;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_UseOn : QuestTarget
    {
        public int Amount;

        public Item_Type Item;
        public ERadialMenuOptions Option; //Radial menu option?
        public SBResource NpcTarget;
    }
}

/*
  protected final native function NPC_Type GetTarget();


  event string GetActiveText(Game_ActiveTextItem aItem) {
    local export editinline NPC_Type t;
    if (aItem.Tag == "T") {                                                     //0000 : 07 5C 00 7A 19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      t = GetTarget();                                                          //0016 : 0F 00 50 89 8B 16 1C 10 E3 8C 16 16 
      if (t != None) {                                                          //0022 : 07 4E 00 77 00 50 89 8B 16 2A 16 
        return t.GetActiveText(aItem.SubItem);                                  //002D : 04 19 00 50 89 8B 16 14 00 00 1B 78 05 00 00 19 00 00 0B 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //004B : 06 59 00 
        return "?Target?";                                                      //004E : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    } else {                                                                    //0059 : 06 DD 00 
      if (aItem.Tag == "O") {                                                   //005C : 07 AC 00 7A 19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
        if (Item != None) {                                                     //0072 : 07 9E 00 77 01 70 D1 8B 16 2A 16 
          return Item.GetActiveText(aItem.SubItem);                             //007D : 04 19 01 70 D1 8B 16 14 00 00 1B 78 05 00 00 19 00 00 0B 8B 16 05 00 04 01 68 BF 73 14 16 
        } else {                                                                //009B : 06 A9 00 
          return "?Object?";                                                    //009E : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
        }
      } else {                                                                  //00A9 : 06 DD 00 
        if (aItem.Tag == "Q") {                                                 //00AC : 07 D1 00 7A 19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
          return "" @ string(Amount);                                           //00C2 : 04 A8 1F 00 39 53 01 78 D0 8B 16 16 
        } else {                                                                //00CE : 06 DD 00 
          return Super.GetActiveText(aItem);                                    //00D1 : 04 1C 28 7C 74 14 00 00 0B 8B 16 16 
        }
      }
    }
    //07 5C 00 7A 19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 0F 00 50 89 8B 16 1C 10 E3 8C 
    //16 16 07 4E 00 77 00 50 89 8B 16 2A 16 04 19 00 50 89 8B 16 14 00 00 1B 78 05 00 00 19 00 00 0B 
    //8B 16 05 00 04 01 68 BF 73 14 16 06 59 00 04 1F 3F 54 61 72 67 65 74 3F 00 06 DD 00 07 AC 00 7A 
    //19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 9E 00 77 01 70 D1 8B 16 2A 16 04 19 01 
    //70 D1 8B 16 14 00 00 1B 78 05 00 00 19 00 00 0B 8B 16 05 00 04 01 68 BF 73 14 16 06 A9 00 04 1F 
    //3F 4F 62 6A 65 63 74 3F 00 06 DD 00 07 D1 00 7A 19 00 00 0B 8B 16 05 00 00 01 80 20 C5 0F 1F 51 
    //00 16 04 A8 1F 00 39 53 01 78 D0 8B 16 16 06 DD 00 04 1C 28 7C 74 14 00 00 0B 8B 16 16 04 0B 47 
    //
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (Amount > 1) {                                                           //0000 : 07 22 00 97 01 78 D0 8B 16 26 16 
    }
    //07 22 00 97 01 78 D0 8B 16 26 16 0E 61 43 00 08 DA 8C 16 70 39 53 00 20 E2 8C 16 1F 2F 25 51 00 
    //16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_UseOnText.Text;                   //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1F 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1F 88 14 04 0B 47 
  }


  event RadialMenuCollect(Game_Pawn aPlayerPawn,Object aObject,byte aMainOption,out array<byte> aSubOptions) {
    if (aObject != None && aObject == Target) {                                 //0000 : 07 2E 00 82 77 00 00 CE 8B 16 2A 16 18 0D 00 72 00 00 CE 8B 16 01 A8 6D C6 0F 16 16 
      aSubOptions[aSubOptions.Length] = Option;                                 //001C : 0F 10 37 00 50 CB 8B 16 00 50 CB 8B 16 01 08 83 C6 0F 
    }
    //07 2E 00 82 77 00 00 CE 8B 16 2A 16 18 0D 00 72 00 00 CE 8B 16 01 A8 6D C6 0F 16 16 0F 10 37 00 
    //50 CB 8B 16 00 50 CB 8B 16 01 08 83 C6 0F 04 0B 47 
  }

*/