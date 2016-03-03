//==============================================================================
//  CT_FinishQuest
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_FinishQuest : CT_Quest
    {
    }
}

/*
  event bool sv_OnFinish(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local export editinline Quest_Type quest;
    quest = Quest_Type(Outer);                                                  //0000 : 0F 00 38 5C 8B 16 2E 88 92 78 01 01 38 75 4B 11 
    UCASSERT(quest != None,"Quest provide conversation is not part of a quest");//0010 : 1C 48 12 49 11 77 00 38 5C 8B 16 2A 16 1F 51 75 65 73 74 20 70 72 6F 76 69 64 65 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 
    if (FinishQuest(aPartner,quest)) {                                          //0051 : 07 78 00 1C 00 EA 79 14 00 88 E1 8B 16 00 38 5C 8B 16 16 
      return Super.sv_OnFinish(aSpeaker,aPartner);                              //0064 : 04 1C F0 10 CA 0F 00 10 F1 8C 16 00 88 E1 8B 16 16 
    } else {                                                                    //0075 : 06 7A 00 
      return False;                                                             //0078 : 04 28 
    }
    //0F 00 38 5C 8B 16 2E 88 92 78 01 01 38 75 4B 11 1C 48 12 49 11 77 00 38 5C 8B 16 2A 16 1F 51 75 
    //65 73 74 20 70 72 6F 76 69 64 65 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 
    //70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 07 78 00 1C 00 EA 79 14 00 88 E1 8B 16 00 38 
    //5C 8B 16 16 04 1C F0 10 CA 0F 00 10 F1 8C 16 00 88 E1 8B 16 16 06 7A 00 04 28 04 0B 47 
  }


*/