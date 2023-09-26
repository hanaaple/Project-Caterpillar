using System;

namespace Utility.Interaction.Ending
{
    public enum EndingType
    {
        공백,
        혼돈,
        엔딩3,
        엔딩4,
        엔딩5,
        엔딩6,
        엔딩7,
        엔딩8,
    }
    
    public enum OperatorType
    {
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }
    
    [Serializable]
    public class EndingProps
    {
        public EndingType endingType;
        public int ascent;
        public OperatorType ascentOperator;
        public int descent;
        public OperatorType descentOperator;
        public int activation;
        public OperatorType activationOperator;
        public int inactive;
        public OperatorType inactiveOperator;
        
        // 5가지 Equal or >= or > or < or <= 
    }
}