namespace Brix.Core;

public interface IJoueur
{
	string _nom { get; set; }

	int index { get; set; }

	string guid { get; set; }

	void Mort();
}
