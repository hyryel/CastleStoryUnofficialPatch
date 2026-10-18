namespace Brix.Game.Network;

public interface Registry<T>
{
	int RegisterObject(T t);

	void ForgetObject(int id);

	T GetObjectById(int id);

	bool TryGetObject(int id, out T t);

	void SetObjectWithId(T t, int id);
}
