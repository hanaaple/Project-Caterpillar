namespace Dialogue
{
    [System.Serializable]
    public class DialogueProps
    {
        public int index;
        public DialogueItemProps[] datas;
    }
    
    [System.Serializable]
    public struct DialogueItemProps
    {
        public string subject;
        public string contents;
        public DialogueType dialogueType;
    }
}