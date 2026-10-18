using System.Collections;
using Brix.Game.Network;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Login;

public class LoginStatusViewController : MonoBehaviour
{
	public Text lblUsername;

	public MainMenuViewController mainMenu;

	private GameBackend gameBackend;

	private void Awake()
	{
		gameBackend = AbstractBackend.Find().GetComponent<GameBackend>();
	}

	public void Start()
	{
		AbstractBackend.signals.Connect(PlayerBackend.UserInfoReceived, OnUserInfoReceived);
		lblUsername.text = gameBackend.playerBackend.Username;
	}

	private void OnUserInfoReceived(Hashtable data)
	{
		if (data.ContainsKey("username"))
		{
			lblUsername.text = data["username"].ToString();
		}
		else if (data.ContainsKey("objects"))
		{
			ArrayList arrayList = JsonConvert.DeserializeObject<ArrayList>(data["objects"].ToString());
			if (arrayList.Count > 0)
			{
				Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(arrayList[0].ToString());
				lblUsername.text = (string)hashtable["username"];
			}
		}
	}
}
