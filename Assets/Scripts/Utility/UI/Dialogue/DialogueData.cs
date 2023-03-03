using System;
using Utility.JsonLoader;

namespace Utility.UI.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        public int index;
        public DialogueElement[] dialogueElements;

        public void Load(string json)
        {
            index = 0;
            dialogueElements = JsonHelper.GetJsonArray<DialogueElement>(json);
        }
        
        public void Reset()
        {
            index = 0;
            dialogueElements = null;
        }
    }

    [Serializable]
    public struct DialogueElement
    {
        public CharacterType name;
        public string subject;
        public string contents;
        public DialogueType dialogueType;
        
        public Expression expression;
        public string[] option;
    }
}