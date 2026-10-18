using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class LogCommandParser : CommandParserInterface
{
	private HandleRef swigCPtr;

	internal LogCommandParser(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.LogCommandParser_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public LogCommandParser()
		: this(RakNetPINVOKE.new_LogCommandParser(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(LogCommandParser obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~LogCommandParser()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_LogCommandParser(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static LogCommandParser GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.LogCommandParser_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new LogCommandParser(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(LogCommandParser i)
	{
		RakNetPINVOKE.LogCommandParser_DestroyInstance(getCPtr(i));
	}

	public override bool OnCommand(string command, uint numParameters, string[] parameterList, TransportInterface transport, SystemAddress systemAddress, string originalString)
	{
		bool result = RakNetPINVOKE.LogCommandParser_OnCommand(swigCPtr, command, numParameters, parameterList, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress), originalString);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override string GetName()
	{
		return RakNetPINVOKE.LogCommandParser_GetName(swigCPtr);
	}

	public override void SendHelp(TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.LogCommandParser_SendHelp(swigCPtr, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void AddChannel(string channelName)
	{
		RakNetPINVOKE.LogCommandParser_AddChannel(swigCPtr, channelName);
	}

	public void WriteLog(string channelName, string format)
	{
		RakNetPINVOKE.LogCommandParser_WriteLog(swigCPtr, channelName, format);
	}

	public override void OnNewIncomingConnection(SystemAddress systemAddress, TransportInterface transport)
	{
		RakNetPINVOKE.LogCommandParser_OnNewIncomingConnection(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void OnConnectionLost(SystemAddress systemAddress, TransportInterface transport)
	{
		RakNetPINVOKE.LogCommandParser_OnConnectionLost(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void OnTransportChange(TransportInterface transport)
	{
		RakNetPINVOKE.LogCommandParser_OnTransportChange(swigCPtr, TransportInterface.getCPtr(transport));
	}
}
