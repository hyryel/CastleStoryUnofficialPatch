namespace Brix.Utils.UI;

public class KeyBinding : IKeyBinding
{
	private string ActionName;

	private string CategoryName;

	string IKeyBinding.ActionName => ActionName;

	string IKeyBinding.CategoryName => CategoryName;

	public KeyBinding(string actionName, string categoryName)
	{
		ActionName = actionName;
		CategoryName = categoryName;
	}
}
