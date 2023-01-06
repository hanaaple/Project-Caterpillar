using System;

public enum EXPRESSION
{
  IDLE = 0,
  Laugh = 1,
  Sad = 2,
  Cry = 3,
  Angry = 4,
  Surprise = 5,
  Panic = 6,
  Suspicion = 7,
  Fear = 8,
  Curious = 9,
}

[Serializable]
public class test
{
  public string name;
  public EXPRESSION expression;
  public string anim_name;
  public string contents;
}