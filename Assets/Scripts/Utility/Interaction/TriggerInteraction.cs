using UnityEngine;

namespace Utility.Interaction
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(TriggerInteraction))]
    public class TriggerInteractionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var generator = (TriggerInteraction)target;
            if (GUILayout.Button("ShowDialogue"))
            {
                generator.ShowDialogue();
            }
        }
    }
#endif

    public class TriggerInteraction : Interaction
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            StartInteraction();
        }
    }
}