//==============================================================================
//  QT_Kill
//==============================================================================

using System.Collections.Generic;
using Gameplay.Entities.NPCs;
using Database.Static;

namespace Gameplay.Quests.QuestTargets
{
    public class QT_Kill : QuestTarget
    {
        public List<SBResource> NpcTargetIDs;

        public override int GetCompletedProgressValue() {

            int output = 0;
            
            //Uses bit flag for each subtarget NPC
            //So completed progress value has a bit set to 1 for each subtarget
            for (int n = 0; n < NpcTargetIDs.Count; n++)
            {
                output = output | (1 << n);
            }

            return output;

        }
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "T") {                                                     //0000 : 07 6A 00 7A 19 00 C8 09 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 
      if (aItem.Ordinality <= Targets.Length) {                                 //0016 : 07 5F 00 98 19 00 C8 09 8B 16 05 00 04 01 30 BF C9 0F 37 01 18 20 8B 16 16 
        return Targets[aItem.Ordinality].GetActiveText(aItem.SubItem);          //002F : 04 19 10 19 00 C8 09 8B 16 05 00 04 01 30 BF C9 0F 01 18 20 8B 16 14 00 00 1B 78 05 00 00 19 00 C8 09 8B 16 05 00 04 01 68 BF 73 14 16 
      } else {                                                                  //005C : 06 6A 00 
        return "?Target?";                                                      //005F : 04 1F 3F 54 61 72 67 65 74 3F 00 
      }
    }
    if (aItem.Tag == "Q") {                                                     //006A : 07 90 00 7A 19 00 C8 09 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
      return "" @ string(Targets.Length);                                       //0080 : 04 A8 1F 00 39 53 37 01 18 20 8B 16 16 
    } else {                                                                    //008D : 06 9C 00 
      return Super.GetActiveText(aItem);                                        //0090 : 04 1C 28 7C 74 14 00 C8 09 8B 16 16 
    }
    //07 6A 00 7A 19 00 C8 09 8B 16 05 00 00 01 80 20 C5 0F 1F 54 00 16 07 5F 00 98 19 00 C8 09 8B 16 
    //05 00 04 01 30 BF C9 0F 37 01 18 20 8B 16 16 04 19 10 19 00 C8 09 8B 16 05 00 04 01 30 BF C9 0F 
    //01 18 20 8B 16 14 00 00 1B 78 05 00 00 19 00 C8 09 8B 16 05 00 04 01 68 BF 73 14 16 06 6A 00 04 
    //1F 3F 54 61 72 67 65 74 3F 00 07 90 00 7A 19 00 C8 09 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
    //04 A8 1F 00 39 53 37 01 18 20 8B 16 16 06 9C 00 04 1C 28 7C 74 14 00 C8 09 8B 16 16 04 0B 47 
  }


  protected function AppendProgressText(out string aDescription,int aProgress) {
    local int ti;
    local int killcount;
    if (Targets.Length > 1) {                                                   //0000 : 07 60 00 97 37 01 18 20 8B 16 26 16 
      ti = 0;                                                                   //000C : 0F 00 58 3D 8B 16 25 
      while (ti < Targets.Length) {                                             //0013 : 07 49 00 96 00 58 3D 8B 16 37 01 18 20 8B 16 16 
        if ((aProgress & 1 << ti) != 0) {                                       //0023 : 07 3F 00 9B 9C 00 10 81 8C 16 94 26 00 58 3D 8B 16 16 16 25 16 
          ++killcount;                                                          //0038 : A3 00 20 36 8C 16 16 
        }
        ++ti;                                                                   //003F : A3 00 58 3D 8B 16 16 
      }
    }
    //07 60 00 97 37 01 18 20 8B 16 26 16 0F 00 58 3D 8B 16 25 07 49 00 96 00 58 3D 8B 16 37 01 18 20 
    //8B 16 16 07 3F 00 9B 9C 00 10 81 8C 16 94 26 00 58 3D 8B 16 16 16 25 16 A3 00 20 36 8C 16 16 A3 
    //00 58 3D 8B 16 16 06 13 00 0E 61 43 00 98 80 8C 16 70 39 53 00 20 36 8C 16 1F 2F 25 51 00 16 16 
    //04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QT_KillText.Text;                    //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1B 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 1B 88 14 04 0B 47 
  }


*/