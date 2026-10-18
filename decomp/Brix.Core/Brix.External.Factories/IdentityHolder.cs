namespace Brix.External.Factories;

public interface IdentityHolder
{
	string Id { get; set; }

	bool Registered { get; set; }
}
