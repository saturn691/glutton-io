using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    private void SubmitName(string name)
    {
        Debug.Log(name);
    }

    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        input.onEndEdit.AddListener(SubmitName);        
    }

    void Update()
    {
        
    }
}
