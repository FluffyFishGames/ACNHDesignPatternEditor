using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.ExecuteInEditMode]
public class StripeScaler : MonoBehaviour
{
    public float Factor = 0.95f;
    public bool OnlyEditor = true;
    
    void Update()
    {
        if (Application.isEditor || !OnlyEditor)
        {
            var transform = GetComponent<RectTransform>();
            var image = GetComponent<UnityEngine.UI.RawImage>();
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            var w = Mathf.Abs(corners[2].x - corners[0].x) * Factor;
            var h = Mathf.Abs(corners[2].y - corners[0].y) * Factor;
            image.uvRect = new Rect(0, 0, w, h);
        }
    }
}
