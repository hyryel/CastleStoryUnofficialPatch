namespace Motus.Behavior;

public interface IWithResult
{
	bool HasResult { get; }

	object Result { get; }
}
