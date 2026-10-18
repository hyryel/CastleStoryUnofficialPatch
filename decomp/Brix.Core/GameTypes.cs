using System.Collections.Generic;

public class GameTypes
{
	public static Dictionary<string, int> Messages = new Dictionary<string, int>
	{
		{ "HELLO", 0 },
		{ "WELCOME", 1 },
		{ "SPAWN", 2 },
		{ "DESPAWN", 3 },
		{ "MOVE", 4 },
		{ "PLANMOVE", 5 },
		{ "CLEAR", 6 },
		{ "MODE", 7 },
		{ "HIT", 8 },
		{ "CHOP", 9 },
		{ "UPDATE", 10 },
		{ "CHAT", 11 },
		{ "TIME", 12 },
		{ "EQUIP", 13 },
		{ "UNEQUIP", 14 },
		{ "ACTION", 15 },
		{ "DAMAGE", 16 },
		{ "POPULATION", 17 },
		{ "KILL", 18 },
		{ "LIST", 19 },
		{ "WHO", 20 },
		{ "BLOC", 21 },
		{ "DEBLOC", 22 },
		{ "HP", 23 },
		{ "ADD", 24 },
		{ "AIM", 25 },
		{ "CHECK", 26 },
		{ "PATH", 27 },
		{ "START", 28 },
		{ "ADDLABOR", 29 },
		{ "OPERATION", 30 },
		{ "LOCALSPAWN", 31 }
	};

	public static Dictionary<string, int> Entities = new Dictionary<string, int>
	{
		{ "PLAYER", 1 },
		{ "BRIXTRON", 2 },
		{ "ARBRECOUPE", 30 }
	};
}
