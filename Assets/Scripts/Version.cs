using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Version : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;
    // Start is called before the first frame update
    void Start()
    {
        Text.text = Application.version;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
