//==============================================================================
//  QC_Stealth
//==============================================================================

using System.Collections.Generic;
using Gameplay.Entities.NPCs;
using Database.Static;

namespace Gameplay.Quests.QuestConditions
{
    public class QC_Stealth : QuestCondition
    {
        public List<SBResource> FactionsGroupedTargets;
        public List<SBResource> NpcsNamedTargets;
    }
}

/*
  protected function string GetDefaultDescription() {
    return Class'StringReferences'.default.QC_StealthText.Text;                 //0000 : 04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 21 88 14 
    //04 36 18 57 4B 11 12 20 48 23 7D 01 05 00 0C 02 C8 21 88 14 04 0B 47 
  }

*/