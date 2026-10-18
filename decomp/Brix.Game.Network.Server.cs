using System;

namespace Brix.Game.Network;

public class Server
{
	public int Id { get; set; }

	public string Name { get; set; }

	public string Motd { get; set; }

	public string Url { get; set; }

	public int NumPlayers { get; set; }

	public string AdminEmail { get; set; }

	public DateTime LastPing { get; set; }

	public DateTime CreatedSince { get; set; }
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
