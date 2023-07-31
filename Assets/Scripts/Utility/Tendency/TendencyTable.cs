using UnityEngine;

namespace Utility.Tendency
{
    [CreateAssetMenu(fileName = "Tendency Table", menuName = "Scriptable Object/Tendency Table", order = int.MaxValue)]
    public class TendencyTable : ScriptableObject
    {
        public TendencyProps[] tendencyProps;
    }
}