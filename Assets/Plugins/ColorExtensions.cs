using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ColorExtensions
{
	public static int Distance(this UnityEngine.Color color, UnityEngine.Color color2)
	{
		var dr = (int) Mathf.Abs((color2.r - color.r) * 255f);
		var dg = (int) Mathf.Abs((color2.g - color.g) * 255f);
		var db = (int) Mathf.Abs((color2.b - color.b) * 255f);
		var da = (int) Mathf.Abs((color2.a - color.a) * 255f);
		return Mathf.Max(dr, dg, db, da);
	}


	public static UnityEngine.Color AlphaComposite(this UnityEngine.Color background, UnityEngine.Color foreground)
	{
		float alpha = background.a + foreground.a - background.a * foreground.a;
		UnityEngine.Color bMultiplied = background * background.a;
		UnityEngine.Color fMultiplied = foreground * foreground.a;

		if (alpha > 0f)
			return new UnityEngine.Color(
				UnityEngine.Mathf.Clamp01((fMultiplied.r + bMultiplied.r * (1 - foreground.a)) / alpha),
				UnityEngine.Mathf.Clamp01((fMultiplied.g + bMultiplied.g * (1 - foreground.a)) / alpha),
				UnityEngine.Mathf.Clamp01((fMultiplied.b + bMultiplied.b * (1 - foreground.a)) / alpha),
				UnityEngine.Mathf.Clamp01(alpha)
			);
		else
			return new UnityEngine.Color(1f, 1f, 1f, 0f);
	}
}
