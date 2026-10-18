namespace Brix.Utils;

internal static class EmptyArray<TElem>
{
	private static readonly TElem[] collection = new TElem[0];

	public static TElem[] Request()
	{
		return collection;
	}
}
