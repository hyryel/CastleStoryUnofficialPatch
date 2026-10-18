using System;
using System.Collections;
using Brix.Game.Network;
using Brix.Network;
using Brix.ThirdParty.Steamworks;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Login;

public class MainMenuViewController : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(MainMenuViewController).FullName);

	public Text lblLogin;

	public Text lblUsername;

	public Text lblWelcome;

	public GameObject LoginDisplayView;

	public GameObject LoginView;

	public GameObject LoginStatusView;

	public GameObject CreateAccountView;

	private GameBackend gameBackend;

	private void Awake()
	{
		gameBackend = AbstractBackend.Find().GetComponent<GameBackend>();
	}

	private void Start()
	{
		CreateAccountViewController.signals.Connect(CreateAccountViewController.OnAccountCreationComplete, HandleOnAccountCreationComplete);
		AbstractBackend.signals.Connect(PlayerBackend.UserInfoReceived, OnUserInfoReceived);
		AbstractBackend.signals.Connect(GameBackend.OnAssociationReceived, HandleOnAssociationReceived);
		logger.Info("SteamManager.Initialized: " + SteamManager.Initialized);
		logger.Info("String.IsNullOrEmpty (gameBackend.authTicket): " + string.IsNullOrEmpty(gameBackend.authTicket));
		if (SteamManager.Initialized)
		{
			LoginDisplayView.SetActive(value: true);
		}
		if (SteamManager.Initialized && string.IsNullOrEmpty(gameBackend.authTicket))
		{
			gameBackend.GetAssociation(SteamworksPlugin.GetSteamID().ToString());
		}
		else if (!string.IsNullOrEmpty(gameBackend.playerBackend.Username))
		{
			lblUsername.text = gameBackend.playerBackend.Username;
			lblWelcome.gameObject.SetActive(value: true);
			lblUsername.gameObject.SetActive(value: true);
			lblLogin.gameObject.SetActive(value: false);
		}
	}

	private void HandleOnAssociationReceived(Hashtable data)
	{
		if (!data.ContainsKey("objects"))
		{
			throw new Exception("An error occured while getting the association.");
		}
		ArrayList arrayList = JsonConvert.DeserializeObject<ArrayList>(data["objects"].ToString());
		if (arrayList.Count == 0)
		{
			LoginDisplayView.SetActive(value: true);
		}
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
		if (LoginView != null)
		{
			LoginView.SetActive(value: false);
			CreateAccountView.SetActive(value: false);
			LoginDisplayView.SetActive(value: true);
			lblLogin.gameObject.SetActive(value: false);
			lblWelcome.gameObject.SetActive(value: true);
			lblUsername.gameObject.SetActive(value: true);
		}
	}

	private void HandleOnAccountCreationComplete(bool success)
	{
		if (success)
		{
			CreateAccountView.SetActive(value: false);
			LoginDisplayView.SetActive(value: true);
		}
	}

	public void OnLoginDisplayClick()
	{
		if (!gameBackend.playerBackend.IsAuthenticated())
		{
			LoginView.SetActive(value: true);
		}
		else
		{
			LoginStatusView.SetActive(value: true);
		}
		LoginDisplayView.SetActive(value: false);
	}

	public void OnLoginBackClicked()
	{
		LoginDisplayView.SetActive(value: true);
		LoginView.SetActive(value: false);
	}

	public void OnCreateAccountClicked()
	{
		LoginView.SetActive(value: false);
		CreateAccountView.SetActive(value: true);
	}

	public void OnLoginStatusBackClicked()
	{
		LoginDisplayView.SetActive(value: true);
		LoginStatusView.SetActive(value: false);
	}

	public void OnCreateAccountBackClicked()
	{
		CreateAccountView.SetActive(value: false);
		LoginView.SetActive(value: true);
	}

	public void OnLoginStatusLogoutClicked()
	{
		lblLogin.gameObject.SetActive(value: true);
		lblWelcome.gameObject.SetActive(value: false);
		lblUsername.gameObject.SetActive(value: false);
		LoginDisplayView.SetActive(value: true);
		LoginStatusView.SetActive(value: false);
		gameBackend.playerBackend.InvalidateToken();
	}
}
