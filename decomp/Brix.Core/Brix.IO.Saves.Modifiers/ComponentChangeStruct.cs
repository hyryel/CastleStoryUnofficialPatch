namespace Brix.IO.Saves.Modifiers;

internal class ComponentChangeStruct
{
	public string id;

	public string oldType;

	public ComponentChangeStruct(string id, string oldType)
	{
		this.id = id;
		this.oldType = oldType;
	}
}
