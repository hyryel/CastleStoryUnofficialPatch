using Brix.External.Signals;

namespace Brix.Game.Interfaces;

public interface IEditorSelectable
{
	Signal OnEditorSelected { get; }

	Signal OnEditorDeselected { get; }
}
