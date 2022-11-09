using Dialogue;
using UnityEngine;

public class SaveNpc : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;
    
    [SerializeField] private GameObject ui;
    [SerializeField] private Vector2 offset;

    
    private void Start()
    {
        SetUIPos();
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Enable();
        playerActions.Interact.performed += delegate
        {
            if (ui.activeSelf)
            {
                ui.SetActive(false);
                DialogueController.instance.onLast.AddListener(() =>
                {
                    SavePanelManager.instance.InitSave();
                    SavePanelManager.instance.SetSaveLoadPanelActive(true);
                    DialogueController.instance.onLast.RemoveAllListeners();
                    SavePanelManager.instance.onSave.AddListener(() =>
                    {
                        SavePanelManager.instance.SetSaveLoadPanelActive(false);
                        DialogueController.instance.InputConverse();
                        SavePanelManager.instance.onSave.RemoveAllListeners();
                    });
                });
                DialogueController.instance.Converse(dialogue.text);
            }
        };
    }

    private void SetUIPos()
    {
        if (!ui)
        {
            return;
        }
        ui.transform.position = transform.position + (Vector3) offset;
    }
    

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ui.SetActive(true);   
        }
    }
    
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ui.SetActive(false);   
        }
    }
}
