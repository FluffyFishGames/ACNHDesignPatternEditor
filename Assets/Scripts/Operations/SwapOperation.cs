public class SwapOperation : IOperation, IPatternOperation, IPatternSelectorOperation
{
	private DesignPattern Pattern;
	private bool _IsFinished = false;

	public SwapOperation(DesignPattern pattern)
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

	public void SelectPattern(DesignPattern pattern)
	{
		if (pattern == Pattern) return;
		
		if (Pattern is ProDesignPattern)
		{
			var backup = new ProDesignPattern();
			backup.CopyFrom(Controller.Instance.CurrentSavegame.ProDesignPatterns[Pattern.Index]);
			Controller.Instance.CurrentSavegame.ProDesignPatterns[Pattern.Index].CopyFrom(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Index]);
			Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Index].CopyFrom(backup);
		}
		else
		{
			var backup = new SimpleDesignPattern();
			backup.CopyFrom(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[Pattern.Index]);
			Controller.Instance.CurrentSavegame.SimpleDesignPatterns[Pattern.Index].CopyFrom(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Index]);
			Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Index].CopyFrom(backup);
		}
   
		_IsFinished = true;
	}

	public void Start()
	{
		Controller.Instance.Popup.SetText("To swap your <#FF6666>pattern<#FFFFFF> please select the spot you want to swap with.", false, () => { return true; });
	}
}