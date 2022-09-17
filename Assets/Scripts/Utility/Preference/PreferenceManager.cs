using UnityEngine;
using UnityEngine.UI;

public class PreferenceManager : MonoBehaviour
{
    public static PreferenceManager instance { get; private set; }
    [Header("슬라이더")]
    public Slider brightnessSlider;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;   
        }
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        brightnessSlider.onValueChanged.AddListener(value =>
        {
        });
    }
    
}
