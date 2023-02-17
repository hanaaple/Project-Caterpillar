using System;

namespace Utility.UI.Dialogue
{
    [Serializable]
    public class DialogueProps
    {
        public int index;
        public DialogueItemProps[] datas;
    }

    [Serializable]
    public struct DialogueItemProps
    {
        public CharacterType name;
        public string subject;
        public string contents;
        public DialogueType dialogueType;
        
        public Expression expression;
        public string[] option;
    }
}