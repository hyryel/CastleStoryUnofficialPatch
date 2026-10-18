using System.Collections;
using Brix.Game.Network;
using Brix.Network;
using Brix.ThirdParty.Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Login;

public class LoginViewController : MonoBehaviour
{
	public InputField inputUsername;

	public InputField inputPassword;

	public Text errorMessageLabel;

	public Text descriptionLabel;

	public Button loginButton;

	public GameObject loadingStatusContainer;

	private GameBackend gameBackend;

	private PlayerBackend playerBackend;

	private void Start()
	{
		playerBackend = AbstractBackend.Find().GetComponent<PlayerBackend>();
		gameBackend = AbstractBackend.Find().GetComponent<GameBackend>();
	}

	public void OnLoginClicked()
	{
		AbstractBackend.signals.ConnectOnce(AbstractBackend.InternalAuthenticateReceived, OnAuthenticateReceived);
		playerBackend.Username = inputUsername.text;
		playerBackend.Authenticate(inputUsername.text, inputPassword.text);
		loadingStatusContainer.SetActive(value: true);
		loginButton.gameObject.SetActive(value: false);
	}

	private void OnAuthenticateReceived(Hashtable data)
	{
		loadingStatusContainer.SetActive(value: false);
		loginButton.gameObject.SetActive(value: true);
		if (!data.ContainsKey("error"))
		{
			inputUsername.text = string.Empty;
			inputPassword.text = string.Empty;
			playerBackend.accessToken = data["access_token"].ToString();
			try
			{
				playerBackend.LinkThirdPartyAccount(playerBackend.UserId, SteamworksPlugin.GetSteamID().ToString(), "steam", gameBackend.authTicket);
				return;
			}
			catch
			{
				return;
			}
		}
		errorMessageLabel.text = (string)data["error"];
		descriptionLabel.gameObject.SetActive(value: false);
		errorMessageLabel.gameObject.SetActive(value: true);
		inputPassword.text = string.Empty;
	}
}
