namespace Brix.RuntimeShell;

public interface Drawer<Entry>
{
	void Clear();

	void AddEntry(Entry entry);

	void Refresh();
}
