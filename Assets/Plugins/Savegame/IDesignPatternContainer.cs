using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IDesignPatternContainer
{
	ProDesignPattern[] ProDesignPatterns { get; set; }
	SimpleDesignPattern[] SimpleDesignPatterns { get; set; }
	PersonalID PersonalID { get; }
	void Save();
	void Dispose();
}
