using System;

namespace Brix.UI.Builder.Panel;

public abstract class BaseSocketLayoutPanelComponent : BaseLayoutPanelComponent
{
	public static readonly Exception TmpChildNotFoundException = new Exception("Child not found in sockets.");
}
