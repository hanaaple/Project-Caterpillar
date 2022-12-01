using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PreferenceManager : MonoBehaviour
{
    public static PreferenceManager instance { get; private set; }

    [SerializeField] private InputAction preferenceInputAction;

    [SerializeField] private Button preferenceButton;

    [SerializeField] private GameObject preferencePanel;
    
    [SerializeField] private Image preferenceFrontPanel;

    [SerializeField] private Button preferenceExitButton;
    
    [SerializeField] private GameObject checkRebindPanel;
    
    [SerializeField] private Button resetButton;
    
    [SerializeField] private Button saveButton;
    
    [SerializeField] private Button rebindButton;
    
    [SerializeField] private Button notRebindButton;

    [SerializeField] private GameObject[] pagePanels;
    
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    [SerializeField] private TMP_Text pageText;
    
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private InputController[] _inputController;
    
    private int _pageIndex;
    private bool _isAlreadyOpen;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
        DontDestroyOnLoad(preferencePanel.transform.root);
    }

    private void OnEnable()
    {
        preferenceInputAction.Enable();
        InputManager.RebindComplete += EnableSaveButton;
        InputManager.RebindEnd += DisableSaveButton;
    }

    private void OnDisable()
    {
        preferenceInputAction.Disable();
        InputManager.RebindComplete -= EnableSaveButton;
        InputManager.RebindEnd -= DisableSaveButton;
    }

    private void EnableSaveButton()
    {
        saveButton.gameObject.SetActive(true);
    }

    private void DisableSaveButton()
    {
        saveButton.gameObject.SetActive(false);
    }

    private void OpenPreferencePanel()
    {
        if (InputManager.IsChanged())
        {
            checkRebindPanel.SetActive(true);
        }
        else
        {
            if (_isAlreadyOpen)
            {

                Time.timeScale = 1;
                preferenceButton.gameObject.SetActive(true);
                preferencePanel.SetActive(false);
            }

            else
            {
                Time.timeScale = 0;
                preferenceButton.gameObject.SetActive(false);
                preferencePanel.SetActive(true);
            }

            _isAlreadyOpen = !_isAlreadyOpen;   
        }
    }

    private void Start()
    {
        resetButton.onClick.AddListener(() =>
        {
            foreach (var inputController in _inputController)
            {
                inputController.ResetBinding();
            }
        });

        saveButton.onClick.AddListener(() =>
        {
            InputManager.EndChange(true);
        });

        preferenceFrontPanel.alphaHitTestMinimumThreshold = 0.1f;
        UpdateUI(0);
        // preferenceInputAction.performed += _ => OpenPreferencePanel();
        
        preferenceButton.onClick.AddListener(OpenPreferencePanel);

        preferenceExitButton.onClick.AddListener(OpenPreferencePanel);

        
        leftButton.onClick.AddListener(() =>
        {
            var nextIdx = (_pageIndex - 1) % pagePanels.Length;
            if (nextIdx < 0)
            {
                nextIdx = pagePanels.Length - 1;
            }
            UpdateUI(nextIdx);
        });
        
        rightButton.onClick.AddListener(() =>
        {
            var nextIdx = (_pageIndex + 1) % pagePanels.Length;
            if (nextIdx < 0)
            {
                nextIdx = pagePanels.Length - 1;
            }
            UpdateUI(nextIdx);
        });
        
        resolutionDropdown.onValueChanged.AddListener(idx =>
        {
            Debug.Log(resolutionDropdown.options[idx].text);
            var resolution = resolutionDropdown.options[idx].text;
            var x = int.Parse(resolution.Split("x")[0]);
            var y = int.Parse(resolution.Split("x")[1]);
            Screen.SetResolution(x, y, false);
            Debug.Log(resolutionDropdown.options[idx].image);
        });
        
        rebindButton.onClick.AddListener(() =>
        {
            InputManager.EndChange(true);
            checkRebindPanel.SetActive(false);
            Time.timeScale = 1;
            preferenceButton.gameObject.SetActive(true);
            preferencePanel.SetActive(false);
        });
        
        notRebindButton.onClick.AddListener(() =>
        {
            InputManager.EndChange(false);
            checkRebindPanel.SetActive(false);
        });
    }

    private void UpdateUI(int nextIdx)
    {
        Debug.Log("현재" + _pageIndex + "목표" + nextIdx);
        foreach (var pagePanel in pagePanels)
        {
            pagePanel.SetActive(false);
        }
        pagePanels[nextIdx].SetActive(true);

        _pageIndex = nextIdx;
        pageText.text = (_pageIndex + 1) + " / " + pagePanels.Length;
    }
}
