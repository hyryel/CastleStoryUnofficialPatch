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
