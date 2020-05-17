using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class PatternExchangeTag : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Label;

    public void SetTag(string name)
    {
        this.Label.text = name;
    }
}
