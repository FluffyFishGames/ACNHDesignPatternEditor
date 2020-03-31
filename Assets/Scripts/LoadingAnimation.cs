using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
	private RectTransform[] Circles;
	private float[] Scales;
	private float Phase = 0f;

    // Start is called before the first frame update
    void Start()
    {
		Circles = new RectTransform[this.transform.childCount];
		Scales = new float[this.transform.childCount];
		for (int i = 0; i < this.transform.childCount; i++)
		{
			Circles[i] = this.transform.GetChild(i).GetComponent<RectTransform>();
			Scales[i] = 1f;
		}
    }

    // Update is called once per frame
    void Update()
    {
		Phase += Time.deltaTime;
		while (Phase > 1f)
			Phase -= 1f;
		float p = 0f;
		float per = ((float) 1f) / ((float) Circles.Length);
		for (int i = 0; i < Circles.Length; i++)
		{
			if (Phase > p && Phase <= p + per)
				Scales[i] = 2f;
			else
				Scales[i] = Mathf.Max(1f, Scales[i] - Time.deltaTime * 3f);
			p += per;
			Circles[i].localScale = new Vector3(Scales[i], Scales[i], 1f);
		}
    }
}
