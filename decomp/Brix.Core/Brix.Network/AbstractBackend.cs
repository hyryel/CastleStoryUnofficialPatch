using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Brix.External.Signals;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.Network;

public abstract class AbstractBackend : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger();

	public string CLIENT_ID = "64462e14de2b26b977303c16bc3d68";

	public string CLIENT_SECRET = "a9491f5363f50ce3d21d0894fe5135";

	public string BACKEND_URL;

	public string BACKEND_API = "/api/v1/";

	public string GAME_CLIENT_USERNAME = "castlestory.game.anonymous";

	public string GAME_CLIENT_PASSWORD = "p42iIl84oHu2c31";

	public static SignalStore signals;

	public static Signal<Hashtable> InternalAuthenticateReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> AuthenticateReceived = new Signal<Hashtable>();

	public static Signal<Hashtable> AuthorizeUserReceived = new Signal<Hashtable>();

	public static Signal<bool> AuthenticationChanged = new Signal<bool>();

	public static Signal LostInternetConnection = new Signal();

	public string accessToken;

	protected string username;

	public string Username
	{
		get
		{
			return username;
		}
		set
		{
			username = value;
		}
	}

	public static AbstractBackend Find()
	{
		return UnityEngine.Object.FindObjectOfType<AbstractBackend>();
	}

	public void AuthorizeUser(string steamId, int userId, string ticket)
	{
		if (!IsClientValid())
		{
			throw new BackendInvalidClientException("The client isn't valid.");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(CLIENT_ID + ":" + CLIENT_SECRET)));
		string url = BACKEND_URL + "/accountlinking/authorize?client_id=" + CLIENT_ID + "&ticket=" + ticket + "&steam_id=" + steamId + "&user_id=" + userId;
		SendRequest("accountlinking/authorize", AuthorizeUserReceived, "GET", null, hashtable, url, "v1", needsToken: false);
	}

	public void Authenticate(string username, string password)
	{
		if (!IsClientValid())
		{
			throw new BackendInvalidClientException("The client isn't valid.");
		}
		this.username = username;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(CLIENT_ID + ":" + CLIENT_SECRET)));
		string url = BACKEND_URL + "/oauth2/token?grant_type=password&client_id=" + CLIENT_ID + "&password=" + password + "&username=" + username;
		SendRequest("oauth2", InternalAuthenticateReceived, "GET", null, hashtable, url, "v1", needsToken: false);
	}

	public void Authenticate(string code)
	{
		if (!IsClientValid())
		{
			throw new BackendInvalidClientException("The client isn't valid.");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(CLIENT_ID + ":" + CLIENT_SECRET)));
		string url = BACKEND_URL + "/oauth2/token?grant_type=authorization_code&client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET + "&code=" + code + "&redirect_uri=" + BACKEND_URL;
		SendRequest("oauth2", InternalAuthenticateReceived, "GET", null, hashtable, url, "v1", needsToken: false);
	}

	public void OnAuthenticateReceived(Hashtable response)
	{
		object[] array = new object[1] { response };
		if (response != null && string.IsNullOrEmpty(accessToken))
		{
			accessToken = response["access_token"].ToString();
			AuthenticationChanged.Invoke(a: true);
		}
		else
		{
			Hashtable hashtable = new Hashtable();
			hashtable["error"] = "Invalid Username/Password";
			array[0] = hashtable;
		}
		AuthenticateReceived.Invoke(response);
	}

	public bool IsAuthenticated()
	{
		return !string.IsNullOrEmpty(accessToken);
	}

	public void InvalidateToken()
	{
		accessToken = null;
		AuthenticationChanged.Invoke(a: false);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod)
	{
		SendRequest(webservice, requestMethod, "GET", null, null, "", "v1", needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType)
	{
		SendRequest(webservice, requestMethod, requestType, null, null, "", "v1", needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType, Hashtable parameters)
	{
		SendRequest(webservice, requestMethod, requestType, parameters, null, "", "v1", needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType, Hashtable parameters, Hashtable headers)
	{
		SendRequest(webservice, requestMethod, requestType, parameters, headers, "", "v1", needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType, Hashtable parameters, Hashtable headers, string url)
	{
		SendRequest(webservice, requestMethod, requestType, parameters, headers, url, "v1", needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType, Hashtable parameters, Hashtable headers, string url, string apiVersion)
	{
		SendRequest(webservice, requestMethod, requestType, parameters, headers, url, apiVersion, needsToken: true);
	}

	protected void SendRequest(string webservice, Signal<Hashtable> requestMethod, string requestType, Hashtable parameters, Hashtable headers, string url, string apiVersion, bool needsToken)
	{
		if (!IsClientValid())
		{
			throw new BackendInvalidClientException("The client isn't valid.");
		}
		if (needsToken && string.IsNullOrEmpty(accessToken))
		{
			throw new BackendNotAuthorizedException("You don't have a token anymore, please authenticate again.");
		}
		ServicePointManager.ServerCertificateValidationCallback = Validator;
		HttpWebRequest request = CreateBasicWebRequest(requestType, BuildUrlString(webservice, url, apiVersion, parameters));
		AddHeadersToRequest(headers, request);
		PreparePostRequest(requestType, parameters, request);
		HandleWebRequestResponse(requestMethod, request);
	}

	protected void Start()
	{
		Init();
	}

	protected virtual void Init()
	{
		signals = GetComponent<SignalStore>();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	protected void Update()
	{
		signals.Poll();
	}

	public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}

	private bool IsClientValid()
	{
		return !string.IsNullOrEmpty(CLIENT_ID) && !string.IsNullOrEmpty(CLIENT_SECRET) && !string.IsNullOrEmpty(BACKEND_URL) && !string.IsNullOrEmpty(BACKEND_API);
	}

	private string BuildUrlString(string webservice, string url, string apiVersion, Hashtable parameters)
	{
		if (string.IsNullOrEmpty(url))
		{
			url = BACKEND_URL + "/api/" + apiVersion + "/" + webservice + "/?format=json&bearer_token=" + accessToken;
		}
		url = AddParametersToUrl(parameters, url);
		logger.Info("BACKEND url: " + url);
		return url;
	}

	private string AddParametersToUrl(Hashtable parameters, string url)
	{
		if (parameters != null)
		{
			foreach (DictionaryEntry parameter in parameters)
			{
				url = url + "&" + parameter.Key.ToString() + "=" + parameter.Value.ToString();
			}
		}
		return url;
	}

	private HttpWebRequest CreateBasicWebRequest(string requestType, string url)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
		httpWebRequest.Method = requestType;
		httpWebRequest.ContentType = "application/json";
		return httpWebRequest;
	}

	private void AddHeadersToRequest(Hashtable headers, HttpWebRequest request)
	{
		if (headers == null)
		{
			return;
		}
		foreach (DictionaryEntry header in headers)
		{
			request.Headers.Add(header.Key.ToString(), header.Value.ToString());
		}
	}

	private void PreparePostRequest(string requestType, Hashtable parameters, HttpWebRequest request)
	{
		if (requestType == "POST")
		{
			string s = JsonConvert.SerializeObject(parameters, Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
			});
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			request.ContentLength = bytes.Length;
			using Stream stream = request.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
		}
	}

	private void HandleWebRequestResponse(Signal<Hashtable> requestMethod, HttpWebRequest request)
	{
		try
		{
			string text = "";
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse())
			{
				using Stream stream = httpWebResponse.GetResponseStream();
				using StreamReader streamReader = new StreamReader(stream);
				text = streamReader.ReadToEnd();
			}
			logger.Info(text);
			requestMethod.Invoke(JsonConvert.DeserializeObject<Hashtable>(text));
		}
		catch (WebException ex)
		{
			HandleWebException(requestMethod, ex);
		}
		catch (Exception ex2)
		{
			logger.Debug("Other Exception: " + ex2);
			logger.Debug(ex2.Source);
			logger.Debug(ex2.StackTrace);
			logger.Debug(ex2.GetType());
			logger.Debug(ex2.Message);
		}
	}

	private void HandleWebException(Signal<Hashtable> requestMethod, WebException ex)
	{
		if (ex.Response != null)
		{
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response)
			{
				StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream());
				string data = streamReader.ReadToEnd();
				Hashtable a = new Hashtable();
				if (httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
				{
					a = HandleWebExceptionBadRequest(data);
				}
				streamReader.Close();
				requestMethod.Invoke(a);
				return;
			}
		}
		logger.Info("No response returned to HandleWebException. Status Code = " + ex.Status);
		logger.Info("WebExceptionError: " + ex.Message);
	}

	private Hashtable HandleWebExceptionBadRequest(string data)
	{
		Hashtable hashtable = new Hashtable();
		Hashtable hashtable2 = JsonConvert.DeserializeObject<Hashtable>(data);
		if (!hashtable2.ContainsKey("error_description"))
		{
			foreach (DictionaryEntry item in hashtable2)
			{
				logger.Info(item.Value.ToString());
				Hashtable hashtable3 = JsonConvert.DeserializeObject<Hashtable>(item.Value.ToString());
				foreach (DictionaryEntry item2 in hashtable3)
				{
					JArray jArray = (JArray)item2.Value;
					List<string> list = jArray.ToObject<List<string>>();
					hashtable.Add("error", list[0]);
				}
			}
		}
		else
		{
			hashtable.Add("error", (string)hashtable2["error_description"]);
		}
		return hashtable;
	}
}
