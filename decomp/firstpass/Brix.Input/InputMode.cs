using System;

namespace Brix.Input;

[Flags]
public enum InputMode
{
	none = 0,
	command = 1,
	blueprint = 2,
	planning = 4,
	worldEditor = 8,
	storage = 0x10,
	pause = 0x20,
	menu = 0x40,
	editMode = 0x16
}
