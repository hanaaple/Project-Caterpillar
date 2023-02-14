namespace Utility.UI.Dialogue
{
  public enum CharacterType
  {
    Dubby = -1,
    Keep = 0,
    None = 1
  }

  public enum Expression
  {
    Default = -1,
    Keep = 0,
    Action1 = 1,
    Action2 = 2,
    Action3 = 3,
    Action4 = 4,
    Action5 = 5
  }

  public enum DialogueType
  {
    None = 0,
    Script = 1,
    Choice = 2,
    ChoiceEnd = 3,
    MoveMap = 4,
    Save = 5,
    Character = 6,
  }
}