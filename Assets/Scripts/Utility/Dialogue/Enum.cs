namespace Utility.Dialogue
{
  public enum CharacterType
  {
    Dubby = -1,
    Keep = 0,
    None = 1,
    Protagonist = 2,
    Naru = 3,
    Photographer = 4,
    Doctor = 5,
    Dog = 6,
    PhotographerSon = 7,
    PhotographerWife = 8,
  }

  public enum CharacterOption
  {
    None = 0,
    Appear = 1,
    Active = 2,
    Inactive = 3,
    Disappear = 4,
    DisappearActive = 5,
    DisappearInactive = 6
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
    CutScene = 6,
    WaitInteract = 7,
    MiniGame = 8,
    Random = 9,
    RandomEnd = 10,
    ImmediatelyExecute = 11,
    Audio = 12,
    DialogueEnd = 13
  }

  public enum InteractionWaitType
  {
    None,
    ImmediatelyInteract,
  }
}