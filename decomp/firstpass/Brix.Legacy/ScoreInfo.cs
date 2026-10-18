namespace Brix.Legacy;

public class ScoreInfo
{
	public string nom;

	public string data1;

	public string data2;

	public int playerIndex;

	public ScoreInfo(string nom, string data1, string data2, int playerIndex)
	{
		this.nom = nom;
		this.data1 = data1;
		this.data2 = data2;
		this.playerIndex = playerIndex;
	}
}
