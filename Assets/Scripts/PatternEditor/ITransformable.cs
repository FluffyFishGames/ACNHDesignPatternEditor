using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITransformable
{
	int ObjectX { get; set; }
	int ObjectY { get; set; }
	int ObjectWidth { get; set; }
	int ObjectHeight { get; set; }

	void UpdateColors();
}
