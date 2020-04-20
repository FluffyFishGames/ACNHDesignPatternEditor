using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LogButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MenuButton>().OnClick = () => {
            string path = Application.persistentDataPath.TrimEnd(new[] { '\\', '/' }); // Mac doesn't like trailing slash
            Process.Start(path);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
