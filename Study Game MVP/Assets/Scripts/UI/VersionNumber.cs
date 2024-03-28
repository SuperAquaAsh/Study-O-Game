using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionNumber : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text.text = "V " + Application.version;
    }
}
