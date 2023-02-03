using System;
using UnityEngine;

public class BeachInteractor : MonoBehaviour
{
    [NonSerialized] public bool interactable;
    [NonSerialized] public bool isStop;

    public Action onInteract;
    
    public void Init()
    {
        onInteract = () => { };
        interactable = true;
        isStop = false;
        gameObject.SetActive(true);
    }
    private void OnMouseUp()
    {
        if (!interactable || isStop)
        {
            return;
        }
        Debug.Log("Interact");

        onInteract?.Invoke();
    }
}
