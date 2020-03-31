using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHorizons.Data;

public class DeleteOperation : IOperation, IPatternOperation
{
	private DesignPattern Pattern;
	private bool _IsFinished = false;

	public DeleteOperation(DesignPattern pattern)
	{
		this.Pattern = pattern;
	}

	public void Abort()
	{
		_IsFinished = true;
	}

	public string GetName()
	{
		return Pattern.Name;
	}

	public DesignPattern GetPattern()
	{
		return this.Pattern;
	}

	public bool IsFinished()
	{
		return this._IsFinished;
	}

	public void Start()
	{
		Controller.Instance.ConfirmationPopup.Show("<align=\"center\"><#827157>,,<#12c7ce>" + Pattern.Name + "<#827157>‘‘\r\nwill be removed. Continue?", () => {
			Pattern.Empty();
			_IsFinished = true;
		}, () => {
			_IsFinished = true;
		});
	}
}