//==============================================================================
//  QC_Protect
//==============================================================================

using System.Collections.Generic;
using Gameplay.Entities.NPCs;
using Database.Static;

namespace Gameplay.Quests.QuestConditions
{
    public class QC_Protect : QuestCondition
    {
        public int Slack; //TODO: No idea what this is...
        public List<SBResource> NpcsTargets;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 6D 00 7A 19 00 F8 C7 5F 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      if (aItem.Ordinality <= Targets.Length) {                                 //0016 : 07 5F 00 98 19 00 F8 C7 5F 0C 05 00 04 01 30 BF C9 0F 37 01 A0 D6 8B 16 16 
        return Targets[aItem.Ordinality].GetActiveText(aItem.SubItem);          //002F : 04 19 10 19 00 F8 C7 5F 0C 05 00 04 01 30 BF C9 0F 01 A0 D6 8B 16 14 00 00 1B 78 05 00 00 19 00 F8 C7 5F 0C 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //005C : 06 6A 00 
        return "?Target?";                                                      //005F : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    } else {                                                                    //006A : 06 79 00 
      return Super.GetActiveText(aItem);                                        //006D : 04 1C 28 7C 74 14 00 F8 C7 5F 0C 16 
    }
    //07 6D 00 7A 19 00 F8 C7 5F 0C 05 00 00 01 80 20 C5 0F 1F 54 00 16 07 5F 00 98 19 00 F8 C7 5F 0C 
    //05 00 04 01 30 BF C9 0F 37 01 A0 D6 8B 16 16 04 19 10 19 00 F8 C7 5F 0C 05 00 04 01 30 BF C9 0F 
    //01 A0 D6 8B 16 14 00 00 1B 78 05 00 00 19 00 F8 C7 5F 0C 05 00 04 01 68 BF 73 14 16 06 6A 00 04 
    //1F 3F 54 61 72 67 65 74 3F 00 06 79 00 04 1C 28 7C 74 14 00 F8 C7 5F 0C 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QC_ProtectText.Text;                 //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 21 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 21 88 14 04 0B 47 
  }

*/