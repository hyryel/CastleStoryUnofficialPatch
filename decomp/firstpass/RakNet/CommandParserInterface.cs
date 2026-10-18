using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class CommandParserInterface : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public static byte VARIABLE_NUMBER_OF_PARAMETERS => RakNetPINVOKE.CommandParserInterface_VARIABLE_NUMBER_OF_PARAMETERS_get();

	internal CommandParserInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(CommandParserInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~CommandParserInterface()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_CommandParserInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual string GetName()
	{
		return RakNetPINVOKE.CommandParserInterface_GetName(swigCPtr);
	}

	public virtual void OnNewIncomingConnection(SystemAddress systemAddress, TransportInterface transport)
	{
		RakNetPINVOKE.CommandParserInterface_OnNewIncomingConnection(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnConnectionLost(SystemAddress systemAddress, TransportInterface transport)
	{
		RakNetPINVOKE.CommandParserInterface_OnConnectionLost(swigCPtr, SystemAddress.getCPtr(systemAddress), TransportInterface.getCPtr(transport));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void SendHelp(TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_SendHelp(swigCPtr, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual bool OnCommand(string command, uint numParameters, string[] parameterList, TransportInterface transport, SystemAddress systemAddress, string originalString)
	{
		bool result = RakNetPINVOKE.CommandParserInterface_OnCommand(swigCPtr, command, numParameters, parameterList, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress), originalString);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void OnTransportChange(TransportInterface transport)
	{
		RakNetPINVOKE.CommandParserInterface_OnTransportChange(swigCPtr, TransportInterface.getCPtr(transport));
	}

	public virtual void SendCommandList(TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_SendCommandList(swigCPtr, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void RegisterCommand(byte parameterCount, string command, string commandHelp)
	{
		RakNetPINVOKE.CommandParserInterface_RegisterCommand(swigCPtr, parameterCount, command, commandHelp);
	}

	public virtual void ReturnResult(bool res, string command, TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_ReturnResult__SWIG_0(swigCPtr, res, command, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReturnResult(string res, string command, TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_ReturnResult__SWIG_1(swigCPtr, res, command, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReturnResult(SystemAddress res, string command, TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_ReturnResult__SWIG_2(swigCPtr, SystemAddress.getCPtr(res), command, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReturnResult(int res, string command, TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_ReturnResult__SWIG_3(swigCPtr, res, command, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReturnResult(string command, TransportInterface transport, SystemAddress systemAddress)
	{
		RakNetPINVOKE.CommandParserInterface_ReturnResult__SWIG_4(swigCPtr, command, TransportInterface.getCPtr(transport), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
