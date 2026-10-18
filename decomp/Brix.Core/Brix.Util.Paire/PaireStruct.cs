namespace Brix.Util.Paire;

public struct PaireStruct<A, B>
{
	public A a;

	public B b;

	public PaireStruct(A a, B b)
	{
		this.a = a;
		this.b = b;
	}
}
