namespace Brix.Input.WorldEditor;

public interface IValidate
{
	string ValidationName { get; }

	bool Validate { get; set; }
}
