using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScroll : MonoBehaviour
{
    public float MoveX = 1f;
    public float MoveY = 1f;
    private UnityEngine.UI.RawImage Image;
    void Start()
    {
        Image = GetComponent<UnityEngine.UI.RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        var newX = Image.uvRect.x + MoveX * Time.deltaTime;
        var newY = Image.uvRect.y + MoveY * Time.deltaTime;
        if (newX > 1)
            newX -= 1f;
        if (newY > 1f)
            newY -= 1f;
        if (newX < 1)
            newX += 1f;
        if (newY < 1f)
            newY += 1f;
        Image.uvRect = new Rect(newX, newY, Image.uvRect.width, Image.uvRect.height);
    }
}
