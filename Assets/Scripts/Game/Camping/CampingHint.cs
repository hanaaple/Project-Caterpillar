using UnityEngine;

public class CampingHint : MonoBehaviour
{
    [SerializeField]
    private GameObject on;
    
    [SerializeField]
    private GameObject off;


    public void SetHint(bool isOpen)
    {
        on.SetActive(isOpen);
        off.SetActive(!isOpen);
    }
}