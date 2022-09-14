using UnityEngine;
using Utility.JsonLoader;

public class jsontest : MonoBehaviour
{
    public TextAsset json;

    void Start()
    {
        var a = JsonHelper.GetJsonArray<test>(json.text);
        
        foreach (var t in a)
        {
            Debug.Log(t.contents);
        }
    }
}
