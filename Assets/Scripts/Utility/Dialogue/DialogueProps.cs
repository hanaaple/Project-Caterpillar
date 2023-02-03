namespace Utility.Dialogue
{
    public enum DialogueType
    {
        None = 0,
        Script = 1,
        Choice = 2,
        ChoiceEnd = 3,
        MoveMap = 4,
        Save = 5,
    }
    
    [System.Serializable]
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