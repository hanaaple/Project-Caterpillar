using UnityEngine;

namespace Utility.Interaction.Ending
{
    [CreateAssetMenu(fileName = "Ending Table", menuName = "Scriptable Object/Ending Table", order = int.MaxValue)]
    public class EndingTable : ScriptableObject
    {
        public EndingProps[] endingProps;
    }
}