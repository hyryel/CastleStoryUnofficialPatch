namespace Brix.UI.Builder.Widget;

public class LuiWidget
{
	private bool _isDirty;

	public bool IsDirty()
	{
		return _isDirty;
	}

	public void SetDirty()
	{
		_isDirty = true;
	}

	public void ResetDirty()
	{
		_isDirty = false;
	}
}
