using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinTextScript : MonoBehaviour
{

    void Start()
    {
    }
    

    void Update()
    {
       bool empty = this.GetComponent<TextMeshProUGUI>().text.Equals("");
    }
}
