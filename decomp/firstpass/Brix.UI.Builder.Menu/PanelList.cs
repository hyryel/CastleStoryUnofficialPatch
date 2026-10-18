using Brix.Utils;

namespace Brix.UI.Builder.Menu;

public sealed class PanelList : PooledList<IPanel>
{
	public static PanelList Request()
	{
		return CollectionPool.RequestCollection<PanelList, IPanel>();
	}

	public static void Release(PanelList panelList)
	{
		CollectionPool.Release<PanelList, IPanel>(ref panelList);
	}
}
