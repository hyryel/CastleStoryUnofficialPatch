namespace Brix.UI.GameSelector;

public static class SelectionTypeExtender
{
	public static SelectionType ToType(this SelectionLayer t)
	{
		return (SelectionType)(1 << (int)t);
	}
}
