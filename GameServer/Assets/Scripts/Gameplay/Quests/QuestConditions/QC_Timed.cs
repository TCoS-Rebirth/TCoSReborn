//==============================================================================
//  QC_Timed
//==============================================================================

namespace Gameplay.Quests.QuestConditions
{
    public class QC_Timed : QuestCondition
    {
        public int Seconds;
    }
}

/*
  event string GetActiveText(Game_ActiveTextItem aItem) {
    if (aItem.Tag == "Q") {                                                     //0000 : 07 25 00 7A 19 00 50 D8 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 
      return GetTimeText(Seconds);                                              //0016 : 04 1B CF 10 00 00 01 B0 CB 8C 16 16 
    } else {                                                                    //0022 : 06 31 00 
      return Super.GetActiveText(aItem);                                        //0025 : 04 1C 28 7C 74 14 00 50 D8 8B 16 16 
    }
    //07 25 00 7A 19 00 50 D8 8B 16 05 00 00 01 80 20 C5 0F 1F 51 00 16 04 1B CF 10 00 00 01 B0 CB 8C 
    //16 16 06 31 00 04 1C 28 7C 74 14 00 50 D8 8B 16 16 04 0B 47 
  }


  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QC_TimedText.Text;                   //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 20 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 20 88 14 04 0B 47 
  }


*/