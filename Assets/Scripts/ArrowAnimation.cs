using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
	private float Phase = 0f;
	private RectTransform MyTransform;
	private float StartY;

    // Start is called before the first frame update
    void Start()
    {
		MyTransform = GetComponent<RectTransform>();
		StartY = MyTransform.anchoredPosition.y;
    }

	// Update is called once per frame
    void Update()
    {
		Phase += Time.deltaTime * 4f;
		while (Phase > 2f)
			Phase -= 2f;
		var p = 0f;
		if (Phase < 1f)
			p = EasingFunction.EaseInOutBack(0f, 1f, Phase);
		else
			p = EasingFunction.EaseInOutBack(1f, 0f, Phase - 1f);

		var scale = (p * 0.2f) + 0.8f;
		var pos = p * 5f;

		MyTransform.anchoredPosition = new Vector2(MyTransform.anchoredPosition.x, StartY + pos);
		MyTransform.localScale = new Vector3(1f, scale, 1f);
	}
}
