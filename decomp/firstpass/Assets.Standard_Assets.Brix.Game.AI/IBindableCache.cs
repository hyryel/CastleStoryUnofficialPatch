namespace Assets.Standard_Assets.Brix.Game.AI;

public interface IBindableCache<TOwner, TInput, TResult>
{
	IBindableCache<TOwner, TInput, TResult> Bind(TOwner owner);

	void RefreshIfNeeded();

	TResult Get(TInput input);

	TResult GetUnsafe(TInput input);
}
