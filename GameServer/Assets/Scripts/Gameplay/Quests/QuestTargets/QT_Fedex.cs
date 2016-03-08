//==============================================================================
//  QT_Fedex
//==============================================================================

using Database.Static;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Fedex : QuestTarget
    {
        public SBResource NpcRecipientID;
        public Content_Inventory Cargo;
        public int Price;
        public SBResource ThanksConvID;

        public override int GetCompletedProgressValue() { return 1; }
    }
}

/*
  protected final native function NPC_Type GetTarget();


  event string GetActiveText(Game_ActiveTextItem aItem) {
    local export editinline NPC_Type A;
    if (aItem.Tag == "T") {                                                     //0000 : 07 7F 00 7A 19 00 B8 15 C6 0F 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      A = Address;                                                              //0016 : 0F 00 F8 1A 8B 16 01 40 7B 8C 16 
      if (A == None) {                                                          //0021 : 07 45 00 72 00 F8 1A 8B 16 2A 16 
        A = Quest_Type(Outer).Finisher;                                         //002C : 0F 00 F8 1A 8B 16 19 2E 88 92 78 01 01 38 75 4B 11 05 00 04 01 80 30 79 14 
      }
      if (A != None) {                                                          //0045 : 07 71 00 77 00 F8 1A 8B 16 2A 16 
        return A.GetActiveText(aItem.SubItem);                                  //0050 : 04 19 00 F8 1A 8B 16 14 00 00 1B 78 05 00 00 19 00 B8 15 C6 0F 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //006E : 06 7C 00 
        return "?Target?";                                                      //0071 : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    } else {                                                                    //007C : 06 0A 01 
      if (aItem.Tag == "O") {                                                   //007F : 07 FE 00 7A 19 00 B8 15 C6 0F 05 00 00 01 80 20 C5 0F 1F 4F 00 16 
        if (Cargo.Count() >= aItem.Ordinality) {                                //0095 : 07 F0 00 99 19 01 90 3C 8C 16 06 00 04 1C E8 02 87 14 16 19 00 B8 15 C6 0F 05 00 04 01 30 BF C9 0F 16 
          return Cargo.GetItem(aItem.Ordinality).GetActiveText(aItem.SubItem);  //00B7 : 04 19 19 01 90 3C 8C 16 14 00 04 1C 48 13 6E 14 19 00 B8 15 C6 0F 05 00 04 01 30 BF C9 0F 16 14 00 00 1B 78 05 00 00 19 00 B8 15 C6 0F 05 00 04 01 68 BF 73 14 16 
        } else {                                                                //00ED : 06 FB 00 
          return "?Object?";                                                    //00F0 : 04 1F 3F 4F 62 6A 65 63 74 3F 00 
        }
      } else {                                                                  //00FB : 06 0A 01 
        return Super.GetActiveText(aItem);                                      //00FE : 04 1C 28 7C 74 14 00 B8 15 C6 0F 16 
      }
    }
    //07 7F 00 7A 19 00 B8 15 C6 0F 05 00 00 01 80 20 C5 0F 1F 54 00 16 0F 00 F8 1A 8B 16 01 40 7B 8C 
    //16 07 45 00 72 00 F8 1A 8B 16 2A 16 0F 00 F8 1A 8B 16 19 2E 88 92 78 01 01 38 75 4B 11 05 00 04 
    //01 80 30 79 14 07 71 00 77 00 F8 1A 8B 16 2A 16 04 19 00 F8 1A 8B 16 14 00 00 1B 78 05 00 00 19 
    //00 B8 15 C6 0F 05 00 04 01 68 BF 73 14 16 06 7C 00 04 1F 3F 54 61 72 67 65 74 3F 00 06 0A 01 07 
    //FE 00 7A 19 00 B8 15 C6 0F 05 00 00 01 80 20 C5 0F 1F 4F 00 16 07 F0 00 99 19 01 90 3C 8C 16 06 
    //00 04 1C E8 02 87 14 16 19 00 B8 15 C6 0F 05 00 04 01 30 BF C9 0F 16 04 19 19 01 90 3C 8C 16 14 
    //00 04 1C 48 13 6E 14 19 00 B8 15 C6 0F 05 00 04 01 30 BF C9 0F 16 14 00 00 1B 78 05 00 00 19 00 
    //B8 15 C6 0F 05 00 04 01 68 BF 73 14 16 06 FB 00 04 1F 3F 4F 62 6A 65 63 74 3F 00 06 0A 01 04 1C 
    //28 7C 74 14 00 B8 15 C6 0F 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_FedexText.Text;                   //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 19 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 19 88 14 04 0B 47 
  }


*/