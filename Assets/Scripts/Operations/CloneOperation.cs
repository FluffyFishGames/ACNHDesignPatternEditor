public class CloneOperation : IOperation, IPatternOperation, IPatternSelectorOperation
{
	private DesignPattern Pattern;
	private bool _IsFinished = false;

	public CloneOperation(DesignPattern pattern)
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
		Controller.Instance.ConfirmationPopup.Show("<align=\"center\"><#827157>,,<#12c7ce>" + pattern.Name + "<#827157>‘‘\r\nwill be removed by this", () => {
			pattern.CopyFrom(Pattern);
			_IsFinished = true;
		}, () => {
			_IsFinished = true;
		});
	}

	public void Start()
	{
		Controller.Instance.Popup.SetText("To clone your <#FF6666>pattern<#FFFFFF> please select the spot you want to clone it to next.", false, () => { return true; });
	}
}