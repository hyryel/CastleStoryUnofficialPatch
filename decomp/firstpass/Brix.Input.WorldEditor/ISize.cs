namespace Brix.Input.WorldEditor;

public interface ISize
{
	float Size { get; set; }

	int ConvertedSize { get; }

	string SliderString { get; }
}
