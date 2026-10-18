using System;

namespace Brix.Game.Network;

public class ServerGame
{
	public int Id { get; set; }

	public int ServerId { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string Level { get; set; }

	public int NumPlayers { get; set; }

	public DateTime CreatedSince { get; set; }
}
