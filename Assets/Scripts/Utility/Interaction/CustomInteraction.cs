using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Utility.Interaction.CustomInteraction))]
public class CustomInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var generator = (Utility.Interaction.CustomInteraction)target;
        if (GUILayout.Button("SetDialogue"))
        {
            generator.SetDialogue();
            EditorUtility.SetDirty(generator);
        }
        
        if (GUILayout.Button("Debug"))
        {
            generator.DebugInteractionData();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif

namespace Utility.Interaction
{
    public class CustomInteraction : Interaction
    {
    }
}