using System.Net.NetworkInformation;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Utils;

public class Authentication : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(Authentication).FullName);

	public string MacAddress;

	private void Start()
	{
		MacAddress = GetMacAddress();
	}

	private void Update()
	{
	}

	private string GetMacAddress()
	{
		string result = string.Empty;
		long num = -1L;
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			logger.Info(string.Concat("Found MAC Address: ", networkInterface.GetPhysicalAddress(), " Type: ", networkInterface.NetworkInterfaceType));
			string text = networkInterface.GetPhysicalAddress().ToString();
			if (networkInterface.Speed > num && !string.IsNullOrEmpty(text) && text.Length >= 12)
			{
				logger.Info("New Max Speed = " + networkInterface.Speed + ", MAC: " + text);
				num = networkInterface.Speed;
				result = text;
			}
		}
		return result;
	}
}
