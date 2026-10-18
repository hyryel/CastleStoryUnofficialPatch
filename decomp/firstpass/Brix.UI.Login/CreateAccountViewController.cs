using System.Collections;
using System.Text.RegularExpressions;
using Brix.External.Signals;
using Brix.Game.Network;
using Brix.Network;
using Brix.ThirdParty.Steamworks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Login;

public class CreateAccountViewController : MonoBehaviour
{
	public InputField inputUsername;

	public InputField inputPassword;

	public InputField inputConfirmPassword;

	public InputField inputEmail;

	public InputField inputFirstName;

	public InputField inputLastName;

	public Text lblMessage;

	public Text lblErrorMessage;

	public Button btnSubmit;

	public Button btnBack;

	public GameObject loadingStatus;

	private GameBackend gameBackend;

	private PlayerBackend playerBackend;

	private int userId;

	public static SignalStore signals;

	public static Signal<bool> OnAccountCreationComplete = new Signal<bool>();

	private SignalConnection linkThirdPartySignalConnection = SignalConnection.Empty;

	private void Awake()
	{
		signals = this.GetOrAddComponent<SignalStore>();
	}

	private void Start()
	{
		gameBackend = AbstractBackend.Find().GetComponent<GameBackend>();
		playerBackend = AbstractBackend.Find().GetComponent<PlayerBackend>();
		AbstractBackend.signals.Connect(GameBackend.OnCreateUserReceived, HandleOnCreateUserReceived);
		if (SteamManager.Initialized)
		{
			inputUsername.text = SteamworksPlugin.GetFriendPersonaName(SteamworksPlugin.GetSteamID());
		}
	}

	private void Update()
	{
		signals.Poll();
	}

	public void OnSubmitClicked()
	{
		loadingStatus.SetActive(value: true);
		btnSubmit.gameObject.SetActive(value: false);
		btnBack.gameObject.SetActive(value: false);
		string text = string.Empty;
		if (!inputPassword.text.Equals(inputConfirmPassword.text))
		{
			text += "Password confirmation doesn't match.\n";
		}
		if (SteamworksPlugin.UserHasLicenseForApp(SteamworksPlugin.GetSteamID(), SteamworksPlugin.GetAppID()) == SteamworksPlugin.EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense)
		{
			text += "You don't own a Steam License.\n";
		}
		if (string.IsNullOrEmpty(inputUsername.text))
		{
			text += "Username is empty.\n";
		}
		if (string.IsNullOrEmpty(inputPassword.text))
		{
			text += "Password is empty.\n";
		}
		if (string.IsNullOrEmpty(inputConfirmPassword.text))
		{
			text += "Password Confirmation is empty.\n";
		}
		if (string.IsNullOrEmpty(inputEmail.text))
		{
			text += "Email is empty.\n";
		}
		if (string.IsNullOrEmpty(inputFirstName.text))
		{
			text += "First Name is empty.\n";
		}
		if (string.IsNullOrEmpty(inputLastName.text))
		{
			text += "Last Name is empty.\n";
		}
		Regex regex = new Regex("^([\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,4})+)$");
		if (!regex.Match(inputEmail.text).Success)
		{
			text += "Your email is not valid.\n";
		}
		if (!string.IsNullOrEmpty(text))
		{
			lblErrorMessage.text = text;
			lblErrorMessage.gameObject.SetActive(value: true);
			lblMessage.gameObject.SetActive(value: false);
			loadingStatus.SetActive(value: false);
			btnSubmit.gameObject.SetActive(value: true);
		}
		else
		{
			gameBackend.CreateUser(inputUsername.text, inputPassword.text, inputEmail.text, inputFirstName.text, inputLastName.text);
		}
	}

	private void HandleOnUserInfoReceived(Hashtable data)
	{
		ArrayList arrayList = JsonConvert.DeserializeObject<ArrayList>(data["objects"].ToString());
		if (arrayList.Count == 0)
		{
			gameBackend.CreateUser(inputUsername.text, inputPassword.text, inputEmail.text, inputFirstName.text, inputLastName.text);
			return;
		}
		lblErrorMessage.text = "That username is already taken.";
		lblErrorMessage.gameObject.SetActive(value: true);
		lblMessage.gameObject.SetActive(value: false);
		loadingStatus.SetActive(value: false);
		btnSubmit.gameObject.SetActive(value: true);
	}

	private void HandleOnCreateUserReceived(Hashtable data)
	{
		if (!string.IsNullOrEmpty((string)data["error"]))
		{
			lblErrorMessage.text = (string)data["error"];
			lblErrorMessage.gameObject.SetActive(value: true);
			lblMessage.gameObject.SetActive(value: false);
			loadingStatus.SetActive(value: false);
			btnSubmit.gameObject.SetActive(value: true);
			btnBack.gameObject.SetActive(value: true);
			return;
		}
		inputUsername.text = string.Empty;
		inputPassword.text = string.Empty;
		inputConfirmPassword.text = string.Empty;
		inputEmail.text = string.Empty;
		inputFirstName.text = string.Empty;
		inputLastName.text = string.Empty;
		loadingStatus.SetActive(value: false);
		btnSubmit.gameObject.SetActive(value: true);
		btnBack.gameObject.SetActive(value: true);
		userId = int.Parse(data["id"].ToString());
		playerBackend.UserId = userId;
		AbstractBackend.signals.ConnectOnce(AbstractBackend.AuthorizeUserReceived, gameBackend.OnAuthorizeUserReceived);
		AbstractBackend.signals.ConnectOnce(AbstractBackend.AuthenticateReceived, OnAuthenticateReceived);
		playerBackend.AuthorizeUser(SteamworksPlugin.GetSteamID().ToString(), userId, gameBackend.authTicket);
	}

	private void OnAuthenticateReceived(Hashtable data)
	{
		try
		{
			if (!gameBackend.IsAuthenticated() || data.ContainsKey("error"))
			{
				lblErrorMessage.text = "Could not authenticate the user for steam account linking.";
				lblErrorMessage.gameObject.SetActive(value: true);
				lblMessage.gameObject.SetActive(value: false);
			}
			else
			{
				linkThirdPartySignalConnection = AbstractBackend.signals.Connect(PlayerBackend.LinkThirdPartyAccountReceived, OnLinkThirdPartyAccountReceived);
				playerBackend.LinkThirdPartyAccount(userId, SteamworksPlugin.GetSteamID().ToString(), "steam", gameBackend.authTicket);
			}
		}
		catch
		{
		}
	}

	private void OnLinkThirdPartyAccountReceived(Hashtable data)
	{
		if (string.IsNullOrEmpty(data["id"].ToString()))
		{
			lblErrorMessage.text = "Could not link the account at the moment. Please contact support@sauropodstudio.com if the problem persists.";
			lblErrorMessage.gameObject.SetActive(value: true);
			lblMessage.gameObject.SetActive(value: false);
		}
		else
		{
			linkThirdPartySignalConnection.Disconnect();
			OnAccountCreationComplete.Invoke(a: true);
		}
	}
}
