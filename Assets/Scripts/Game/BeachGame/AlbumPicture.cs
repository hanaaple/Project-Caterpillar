using Game.BeachGame;
using UnityEngine;
using UnityEngine.UI;

public class AlbumPicture : MonoBehaviour
{
    public enum PictureState
    {
        Default,
        Active,
        Clear
    }

    public BeachInteractType beachInteractType;
    
    [SerializeField] protected Button panelButton;
    [SerializeField] private GameObject textImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clearSprite;
    
    [Space(5)]
    [Header("사진 패널")]
    [SerializeField] private GameObject picturePanel;
    [SerializeField] private Button panelExitButton;
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject activePanel;
    [SerializeField] private GameObject clearPanel;
    
    private void Start()
    {
        panelButton.onClick.AddListener(() =>
        {
            picturePanel.SetActive(true);
        });
        panelExitButton.onClick.AddListener(() =>
        {
            picturePanel.SetActive(false);
        });
    }

    public void SetPanel(PictureState state)
    {
        Debug.Log("상태: " + state);
        if (state == PictureState.Default)
        {
            panelButton.image.sprite = defaultSprite;
            defaultPanel.SetActive(true);
            activePanel.SetActive(false);
            clearPanel.SetActive(false);
        }
        else if (state == PictureState.Active)
        {
            if (!clearPanel.activeSelf)
            {
                defaultPanel.SetActive(false);
                activePanel.SetActive(true);
                clearPanel.SetActive(false);
            }
            textImage.SetActive(true);
        }
        else if (state == PictureState.Clear)
        {
            panelButton.image.sprite = clearSprite;
            defaultPanel.SetActive(false);
            activePanel.SetActive(false);
            clearPanel.SetActive(true);
        }
    }

    public virtual void SetPanel(int idx)
    {
        
    }

    public void Init()
    {
        SetPanel(PictureState.Default);
        
        textImage.SetActive(false);
    }
}
