using System;
using Dialogue;
using UnityEngine;

public class DialogueTestTest : MonoBehaviour
{
    public TextAsset json;

    private void Start()
    {
        DialogueController.instance.Converse(json.text);
    }
}
