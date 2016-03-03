//==============================================================================
//  CT_Quest
//==============================================================================

namespace Gameplay.Conversations
{
    public class CT_Quest : ConversationTopic
    {
    }
}

/*

  function Content_Type GetContext() {
    return Quest_Type(Outer);                                                   //0000 : 04 2E 88 92 78 01 01 38 75 4B 11 
    //04 2E 88 92 78 01 01 38 75 4B 11 04 0B 47 
  }


  function string GetText() {
    local export editinline Quest_Type quest;
    quest = Quest_Type(GetContext());                                           //0000 : 0F 00 A8 5E 8B 16 2E 88 92 78 01 1B 9F 10 00 00 16 
    UCASSERT(quest != None,"Quest conversation context is not a quest");        //0011 : 1C 48 12 49 11 77 00 A8 5E 8B 16 2A 16 1F 51 75 65 73 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 63 6F 6E 74 65 78 74 20 69 73 20 6E 6F 74 20 61 20 71 75 65 73 74 00 16 
    if (Len(TopicText.Text) != 0) {                                             //004A : 07 66 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 16 25 16 
      return Super.GetText();                                                   //005C : 04 1C D0 C3 83 14 16 
    } else {                                                                    //0063 : 06 76 00 
      return quest.GetName();                                                   //0066 : 04 19 00 A8 5E 8B 16 06 00 00 1B F9 03 00 00 16 
    }
    //0F 00 A8 5E 8B 16 2E 88 92 78 01 1B 9F 10 00 00 16 1C 48 12 49 11 77 00 A8 5E 8B 16 2A 16 1F 51 
    //75 65 73 74 20 63 6F 6E 76 65 72 73 61 74 69 6F 6E 20 63 6F 6E 74 65 78 74 20 69 73 20 6E 6F 74 
    //20 61 20 71 75 65 73 74 00 16 07 66 00 9B 7D 36 18 57 4B 11 01 68 6F 7C 14 16 25 16 04 1C D0 C3 
    //83 14 16 06 76 00 04 19 00 A8 5E 8B 16 06 00 00 1B F9 03 00 00 16 04 0B 47 
  }


*/