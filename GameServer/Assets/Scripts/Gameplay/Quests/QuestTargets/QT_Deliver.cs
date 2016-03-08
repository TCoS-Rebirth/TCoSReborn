//==============================================================================
//  QT_Deliver
//==============================================================================

using Database.Static;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Gameplay.Items;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Deliver : QuestTarget
    {
        public SBResource NpcRecipientID;
        public int Amount;
        public Item_Type Cargo;
        public SBResource DeliveryConvID;

        public override int GetCompletedProgressValue() { return 1; }
    }
}

/*
  protected final native function NPC_Type GetTarget();


  event string GetActiveText(Game_ActiveTextItem aItem) {
    local Game_ActiveTextItem pluralityItem;
    local export editinline NPC_Type t;
    if (aItem.Tag == "T") {                                                     //0000 : 07 5C 00 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      t = GetTarget();                                                          //0016 : 0F 00 58 A3 8B 16 1C 70 5E 8C 16 16 
      if (t != None) {                                                          //0022 : 07 4E 00 77 00 58 A3 8B 16 2A 16 
        return t.GetActiveText(aItem.SubItem);                                  //002D : 04 19 00 58 A3 8B 16 14 00 00 1B 78 05 00 00 19 00 60 F9 8A 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //004B : 06 59 00 
        return "?Target?";                                                      //004E : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    } else {                                                                    //0059 : 06 68 01 
      if (aItem.Tag == "Q") {                                                   //005C : 07 81 00 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
        return "" @ string(Amount);                                             //0072 : 04 A8 1F 00 39 53 01 60 CE C6 0F 16 
      } else {                                                                  //007E : 06 68 01 
        if (aItem.Tag == "O") {                                                 //0081 : 07 D1 00 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
          if (Cargo != None) {                                                  //0097 : 07 C3 00 77 01 68 FF C5 0F 2A 16 
            return Cargo.GetActiveText(aItem.SubItem);                          //00A2 : 04 19 01 68 FF C5 0F 14 00 00 1B 78 05 00 00 19 00 60 F9 8A 16 05 00 04 01 68 BF 73 14 16 
          } else {                                                              //00C0 : 06 CE 00 
            return "?Object?";                                                  //00C3 : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
          }
        } else {                                                                //00CE : 06 68 01 
          if (aItem.Tag == "OS") {                                              //00D1 : 07 5C 01 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 4F 53 00 16 
            if (Cargo != None) {                                                //00E8 : 07 4E 01 77 01 68 FF C5 0F 2A 16 
              pluralityItem.Tag = "Q";                                          //00F3 : 0F 19 00 E0 46 C5 0F 05 00 00 01 80 20 C5 0F 1F 51 00 
              pluralityItem.Ordinality = Amount;                                //0105 : 0F 19 00 E0 46 C5 0F 05 00 04 01 30 BF C9 0F 01 60 CE C6 0F 
              pluralityItem.SubItem = aItem.SubItem;                            //0119 : 0F 19 00 E0 46 C5 0F 05 00 04 01 68 BF 73 14 19 00 60 F9 8A 16 05 00 04 01 68 BF 73 14 
              return Cargo.GetActiveText(pluralityItem);                        //0136 : 04 19 01 68 FF C5 0F 0B 00 00 1B 78 05 00 00 00 E0 46 C5 0F 16 
            } else {                                                            //014B : 06 59 01 
              return "?Object?";                                                //014E : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
            }
          } else {                                                              //0159 : 06 68 01 
            return Super.GetActiveText(aItem);                                  //015C : 04 1C 28 7C 74 14 00 60 F9 8A 16 16 
          }
        }
      }
    }
    //07 5C 00 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 0F 00 58 A3 8B 16 1C 70 5E 8C 
    //16 16 07 4E 00 77 00 58 A3 8B 16 2A 16 04 19 00 58 A3 8B 16 14 00 00 1B 78 05 00 00 19 00 60 F9 
    //8A 16 05 00 04 01 68 BF 73 14 16 06 59 00 04 1F 3F 54 61 72 67 65 74 3F 00 06 68 01 07 81 00 7A 
    //19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 04 A8 1F 00 39 53 01 60 CE C6 0F 16 06 68 
    //01 07 D1 00 7A 19 00 60 F9 8A 16 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 C3 00 77 01 68 FF C5 0F 
    //2A 16 04 19 01 68 FF C5 0F 14 00 00 1B 78 05 00 00 19 00 60 F9 8A 16 05 00 04 01 68 BF 73 14 16 
    //06 CE 00 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 68 01 07 5C 01 7A 19 00 60 F9 8A 16 05 00 00 01 80 
    //20 C5 0F 1F 4F 53 00 16 07 4E 01 77 01 68 FF C5 0F 2A 16 0F 19 00 E0 46 C5 0F 05 00 00 01 80 20 
    //C5 0F 1F 51 00 0F 19 00 E0 46 C5 0F 05 00 04 01 30 BF C9 0F 01 60 CE C6 0F 0F 19 00 E0 46 C5 0F 
    //05 00 04 01 68 BF 73 14 19 00 60 F9 8A 16 05 00 04 01 68 BF 73 14 04 19 01 68 FF C5 0F 0B 00 00 
    //1B 78 05 00 00 00 E0 46 C5 0F 16 06 59 01 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 68 01 04 1C 28 7C 
    //74 14 00 60 F9 8A 16 16 04 0B 47 
  }


  function bool sv_OnComplete(Game_Pawn aPawn,optional Game_Pawn aTargetPawn) {
    local Content_Inventory tempInventory;
    if (Super.sv_OnComplete(aPawn,aTargetPawn)) {                               //0000 : 07 54 00 1C A0 45 74 14 00 58 50 8C 16 00 C8 5A 8C 16 16 
      tempInventory = new Class'Content_Inventory';                             //0013 : 0F 00 D8 A0 8B 16 11 0B 0B 0B 20 40 5C 77 01 
      tempInventory.AddItem(Cargo,Amount,0,0);                                  //0022 : 19 00 D8 A0 8B 16 14 00 04 1B F1 10 00 00 01 68 FF C5 0F 01 60 CE C6 0F 24 00 24 00 16 
      if (RemoveItems(aPawn,tempInventory)) {                                   //003F : 07 54 00 1B CC 10 00 00 00 58 50 8C 16 00 D8 A0 8B 16 16 
        return True;                                                            //0052 : 04 27 
      }
    }
    return False;                                                               //0054 : 04 28 
    //07 54 00 1C A0 45 74 14 00 58 50 8C 16 00 C8 5A 8C 16 16 0F 00 D8 A0 8B 16 11 0B 0B 0B 20 40 5C 
    //77 01 19 00 D8 A0 8B 16 14 00 04 1B F1 10 00 00 01 68 FF C5 0F 01 60 CE C6 0F 24 00 24 00 16 07 
    //54 00 1B CC 10 00 00 00 58 50 8C 16 00 D8 A0 8B 16 16 04 27 04 28 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    if (aProgress > Amount) {                                                   //0000 : 07 1A 00 97 00 B8 9C 8B 16 01 60 CE C6 0F 16 
      aProgress = Amount;                                                       //000F : 0F 00 B8 9C 8B 16 01 60 CE C6 0F 
    }
    if (Amount > 1) {                                                           //001A : 07 3C 00 97 01 60 CE C6 0F 26 16 
    }
    //07 1A 00 97 00 B8 9C 8B 16 01 60 CE C6 0F 16 0F 00 B8 9C 8B 16 01 60 CE C6 0F 07 3C 00 97 01 60 
    //CE C6 0F 26 16 0E 61 43 00 20 58 8C 16 70 39 53 00 B8 9C 8B 16 1F 2F 25 51 00 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_DeliverText.Text;                 //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 17 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 17 88 14 04 0B 47 
  }



*/