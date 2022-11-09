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

    [SerializeField] private Button preferenceExitButton;

    [SerializeField] private GameObject[] pagePanels;
    
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    [SerializeField] private TMP_Text pageText;
    
    [SerializeField] private TMP_Dropdown resolutionDropdown;

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
    }
    
    private void OnDisable()
    {
        preferenceInputAction.Disable();
    }

    private void OpenPreferencePanel()
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

    void Start()
    {
        preferenceInputAction.performed += _ => OpenPreferencePanel();
        
        preferenceButton.onClick.AddListener(OpenPreferencePanel);

        preferenceExitButton.onClick.AddListener(OpenPreferencePanel);

        
        leftButton.onClick.AddListener(() =>
        {
            var nextIdx = (_pageIndex - 1) % pagePanels.Length;
            UpdateUI(nextIdx);
        });
        
        rightButton.onClick.AddListener(() =>
        {
            var nextIdx = (_pageIndex + 1) % pagePanels.Length;
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
    }

    private void UpdateUI(int nextIdx)
    {
        pagePanels[_pageIndex].SetActive(false);
        pagePanels[nextIdx].SetActive(true);

        _pageIndex = nextIdx;
        pageText.text = _pageIndex + " / " + pagePanels.Length;
    }
}
