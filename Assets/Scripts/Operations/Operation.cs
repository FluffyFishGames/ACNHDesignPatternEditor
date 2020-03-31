using MyHorizons.Data;
using MyHorizons.Data.Save;
using SFB;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZXing;
using Color = UnityEngine.Color;

public interface IOperation
{
	void Start();
	bool IsFinished();
	void Abort();
}

public interface IChangeNameOperation
{
	void SetName(string name);
}

public interface IPatternOperation
{
	DesignPattern GetPattern();
}

public interface IPatternSelectorOperation
{
	void SelectPattern(DesignPattern pattern);
}