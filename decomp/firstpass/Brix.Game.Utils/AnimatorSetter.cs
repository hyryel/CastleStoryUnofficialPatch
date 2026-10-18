namespace Brix.Game.Utils;

public abstract class AnimatorSetter<T> : AnimatorAction
{
	public T Value;

	protected AnimatorSetter(string key, T value)
		: base(key)
	{
		Value = value;
	}

	public AnimatorAction SetValue(T value)
	{
		Value = value;
		return this;
	}

	public AnimatorAction Set(string key, T value)
	{
		SetKey(key);
		SetValue(value);
		return this;
	}
}
