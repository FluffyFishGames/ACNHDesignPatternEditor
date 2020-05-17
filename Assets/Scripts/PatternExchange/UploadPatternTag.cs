using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class UploadPatternTag : MonoBehaviour
{
    public UnityEngine.UI.Button RemoveButton;
    public TMPro.TextMeshProUGUI Label;
    public UploadPattern UploadPattern;

    void Start()
    {
        RemoveButton.onClick.AddListener(this.RemoveButton_Click);
    }

    private void RemoveButton_Click()
    {
        this.UploadPattern.RemoveTag(this.Label.text);
    }

    public void SetTag(UploadPattern uploadPattern, string name)
    {
        this.Label.text = name;
        this.UploadPattern = uploadPattern;
    }
}
