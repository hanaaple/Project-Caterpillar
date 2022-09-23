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

    [SerializeField] private Button audioButton;

    [SerializeField] private GameObject audioPanel;

    [SerializeField] private Button displayButton;

    [SerializeField] private GameObject displayPanel;
    
    [SerializeField] private Button controlButton;

    [SerializeField] private GameObject controlPanel;
    
    [SerializeField] private TMPro.TMP_Dropdown resolutionDropdown;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        preferenceInputAction.Enable();
    }
    
    private void OnDisable()
    {
        preferenceInputAction.Disable();
    }

    private bool _isAlreadyOpen;
    void OpenPreferencePanel()
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

        audioButton.onClick.AddListener(() =>
        {
            audioPanel.SetActive(true);
            displayPanel.SetActive(false);
            controlPanel.SetActive(false);
        });

        displayButton.onClick.AddListener(() =>
        {
            displayPanel.SetActive(true);
            controlPanel.SetActive(false);
            audioPanel.SetActive(false);
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
        
        controlButton.onClick.AddListener(() =>
        {
            controlPanel.SetActive(true);
            displayPanel.SetActive(false);
            audioPanel.SetActive(false);
        });
    }
}
