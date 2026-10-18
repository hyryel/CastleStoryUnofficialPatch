namespace Brix.Utils.UI;

public interface IKeyBindingsList<TKeyBinding> where TKeyBinding : IKeyBinding
{
	TKeyBinding[] keyData { get; }
}
