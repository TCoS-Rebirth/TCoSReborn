//==============================================================================
//  QC_Survival
//==============================================================================

namespace Gameplay.Quests.QuestConditions
{
    public class QC_Survival : QuestCondition
    {
        public float DefeatFraction; //Player health fraction below which quest fails?
    }
}

/*
  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QC_SurvivalText.Text;                //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 20 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 48 20 88 14 04 0B 47 
  }
*/