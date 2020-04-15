using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHorizons.Data.Save
{
	public interface IDesignPatternContainer
	{
		DesignPattern[] DesignPatterns { get; }
		DesignPattern[] ProDesignPatterns { get; }
		PlayerData.PersonalID PersonalID { get; }
		bool Save(IProgress<float> progress);
	}
}
