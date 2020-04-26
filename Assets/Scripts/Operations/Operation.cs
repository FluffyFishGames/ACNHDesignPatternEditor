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