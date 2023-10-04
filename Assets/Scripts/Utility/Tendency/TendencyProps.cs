using System;

namespace Utility.Tendency
{
    public enum TendencyType
    {
        변덕,
        완고,
        다정,
        이성,
        다감,
        철저,
        감내,
        극단,
        결의,
        애정,
        소모,
        인내,
        용기,
        도피,
        정석,
        모험,
        쟁취,
        융통,
        감인
    }

    [Serializable]
    public class TendencyProps
    {
        public TendencyType tendencyType;
        public int ascent;
        public int descent;
        public int activation;
        public int inactive;
    }
}