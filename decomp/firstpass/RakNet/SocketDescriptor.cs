using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SocketDescriptor : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ushort port
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_port_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_port_set(swigCPtr, value);
		}
	}

	public string hostAddress
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_hostAddress_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_hostAddress_set(swigCPtr, value);
		}
	}

	public short socketFamily
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_socketFamily_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_socketFamily_set(swigCPtr, value);
		}
	}

	public ushort remotePortRakNetWasStartedOn_PS3_PSP2
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_remotePortRakNetWasStartedOn_PS3_PSP2_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_remotePortRakNetWasStartedOn_PS3_PSP2_set(swigCPtr, value);
		}
	}

	public int chromeInstance
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_chromeInstance_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_chromeInstance_set(swigCPtr, value);
		}
	}

	public bool blockingSocket
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_blockingSocket_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_blockingSocket_set(swigCPtr, value);
		}
	}

	public uint extraSocketOptions
	{
		get
		{
			return RakNetPINVOKE.SocketDescriptor_extraSocketOptions_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SocketDescriptor_extraSocketOptions_set(swigCPtr, value);
		}
	}

	internal SocketDescriptor(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public SocketDescriptor()
		: this(RakNetPINVOKE.new_SocketDescriptor__SWIG_0(), cMemoryOwn: true)
	{
	}

	public SocketDescriptor(ushort _port, string _hostAddress)
		: this(RakNetPINVOKE.new_SocketDescriptor__SWIG_1(_port, _hostAddress), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(SocketDescriptor obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~SocketDescriptor()
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
					RakNetPINVOKE.delete_SocketDescriptor(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
