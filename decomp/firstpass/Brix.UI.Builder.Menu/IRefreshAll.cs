namespace Brix.UI.Builder.Menu;

public interface IRefreshAll : IRefresh
{
	void ResetAll();

	bool RefreshAll();
}
public interface IRefreshAll<in TSocket> : IRefreshAll, IRefresh where TSocket : struct
{
	void ResetAtSocket(TSocket socket);

	void RefreshAtSocket(TSocket socket);
}
