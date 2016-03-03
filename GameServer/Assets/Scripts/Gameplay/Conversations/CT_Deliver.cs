//==============================================================================
//  CT_Deliver
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_Deliver : CT_Target
    {
    }
}

/*
  event bool sv_OnFinish(Game_Pawn aSpeaker,Game_Pawn aPartner) {
    local export editinline Quest_Type quest;
    local export editinline QT_Deliver Target;
    Target = QT_Deliver(Outer);                                                 //0000 : 0F 00 88 3E 8B 16 2E 50 18 AB 01 01 38 75 4B 11 
    quest = Quest_Type(Target.Outer);                                           //0010 : 0F 00 80 CE 8B 16 2E 88 92 78 01 19 00 88 3E 8B 16 05 00 04 01 38 75 4B 11 
    UCASSERT(Target != None,"Quest deliver conversation is not part of a target");//0029 : 1C 48 12 49 11 77 00 88 3E 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 6C 69 76 65 72 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 74 61 72 67 65 74 00 16 
    UCASSERT(quest != None,"Quest deliver target is not part of a quest");      //006B : 1C 48 12 49 11 77 00 80 CE 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 6C 69 76 65 72 20 74 61 72 67 65 74 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 71 75 65 73 74 00 16 
    if (Game_PlayerPawn(aPartner).questLog.sv_SetTargetAsCompleted(Target,aSpeaker)) {//00A6 : 07 E4 00 19 19 2E 68 3C 77 01 00 D0 CB 8B 16 05 00 04 01 80 6B C7 0F 10 00 04 1C 68 AD 7F 14 00 88 3E 8B 16 00 40 C0 8B 16 16 
      return Super.sv_OnFinish(aSpeaker,aPartner);                              //00D0 : 04 1C F0 10 CA 0F 00 40 C0 8B 16 00 D0 CB 8B 16 16 
    } else {                                                                    //00E1 : 06 E6 00 
      return False;                                                             //00E4 : 04 28 
    }
    //0F 00 88 3E 8B 16 2E 50 18 AB 01 01 38 75 4B 11 0F 00 80 CE 8B 16 2E 88 92 78 01 19 00 88 3E 8B 
    //16 05 00 04 01 38 75 4B 11 1C 48 12 49 11 77 00 88 3E 8B 16 2A 16 1F 51 75 65 73 74 20 64 65 6C 
    //69 76 65 72 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 
    //20 61 20 74 61 72 67 65 74 00 16 1C 48 12 49 11 77 00 80 CE 8B 16 2A 16 1F 51 75 65 73 74 20 64 
    //65 6C 69 76 65 72 20 74 61 72 67 65 74 20 69 73 20 6E 6F 74 20 70 61 72 74 20 6F 66 20 61 20 71 
    //75 65 73 74 00 16 07 E4 00 19 19 2E 68 3C 77 01 00 D0 CB 8B 16 05 00 04 01 80 6B C7 0F 10 00 04 
    //1C 68 AD 7F 14 00 88 3E 8B 16 00 40 C0 8B 16 16 04 1C F0 10 CA 0F 00 40 C0 8B 16 00 D0 CB 8B 16 
    //16 06 E6 00 04 28 04 0B 47 
  }

*/