namespace Brix.UI.Builder.Menu;

public interface IHierarchyName
{
	string Name { get; }

	string HierarchyName { get; }

	string FullName { get; }

	string SuffixName { get; set; }
}
