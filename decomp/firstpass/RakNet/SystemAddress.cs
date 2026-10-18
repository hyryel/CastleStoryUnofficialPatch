using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SystemAddress : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ushort debugPort
	{
		get
		{
			return RakNetPINVOKE.SystemAddress_debugPort_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SystemAddress_debugPort_set(swigCPtr, value);
		}
	}

	public ushort systemIndex
	{
		get
		{
			return RakNetPINVOKE.SystemAddress_systemIndex_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SystemAddress_systemIndex_set(swigCPtr, value);
		}
	}

	internal SystemAddress(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public SystemAddress()
		: this(RakNetPINVOKE.new_SystemAddress__SWIG_0(), cMemoryOwn: true)
	{
	}

	public SystemAddress(string str)
		: this(RakNetPINVOKE.new_SystemAddress__SWIG_1(str), cMemoryOwn: true)
	{
	}

	public SystemAddress(string str, ushort port)
		: this(RakNetPINVOKE.new_SystemAddress__SWIG_2(str, port), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(SystemAddress obj)
	{
		return (!(obj == null)) ? obj.swigCPtr : new HandleRef(null, IntPtr.Zero);
	}

	~SystemAddress()
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
					RakNetPINVOKE.delete_SystemAddress(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public override int GetHashCode()
	{
		return (int)ToInteger(this);
	}

	public static bool operator ==(SystemAddress a, SystemAddress b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(SystemAddress a, SystemAddress b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator <(SystemAddress a, SystemAddress b)
	{
		return a.OpLess(b);
	}

	public static bool operator >(SystemAddress a, SystemAddress b)
	{
		return a.OpGreater(b);
	}

	public static bool operator <=(SystemAddress a, SystemAddress b)
	{
		return a.OpLess(b) || a == b;
	}

	public static bool operator >=(SystemAddress a, SystemAddress b)
	{
		return a.OpGreater(b) || a == b;
	}

	public override string ToString()
	{
		return ToString(writePort: true);
	}

	public void ToString(bool writePort, out string dest)
	{
		dest = ToString(writePort);
	}

	public static int size()
	{
		return RakNetPINVOKE.SystemAddress_size();
	}

	public static uint ToInteger(SystemAddress sa)
	{
		uint result = RakNetPINVOKE.SystemAddress_ToInteger(getCPtr(sa));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public byte GetIPVersion()
	{
		return RakNetPINVOKE.SystemAddress_GetIPVersion(swigCPtr);
	}

	public uint GetIPPROTO()
	{
		return RakNetPINVOKE.SystemAddress_GetIPPROTO(swigCPtr);
	}

	public void SetToLoopback()
	{
		RakNetPINVOKE.SystemAddress_SetToLoopback__SWIG_0(swigCPtr);
	}

	public void SetToLoopback(byte ipVersion)
	{
		RakNetPINVOKE.SystemAddress_SetToLoopback__SWIG_1(swigCPtr, ipVersion);
	}

	public bool IsLoopback()
	{
		return RakNetPINVOKE.SystemAddress_IsLoopback(swigCPtr);
	}

	public string ToString(bool writePort, char portDelineator)
	{
		return RakNetPINVOKE.SystemAddress_ToString__SWIG_0(swigCPtr, writePort, portDelineator);
	}

	public string ToString(bool writePort)
	{
		return RakNetPINVOKE.SystemAddress_ToString__SWIG_1(swigCPtr, writePort);
	}

	public void ToString(bool writePort, string dest, char portDelineator)
	{
		RakNetPINVOKE.SystemAddress_ToString__SWIG_2(swigCPtr, writePort, dest, portDelineator);
	}

	public bool FromString(string str, char portDelineator, int ipVersion)
	{
		return RakNetPINVOKE.SystemAddress_FromString__SWIG_0(swigCPtr, str, portDelineator, ipVersion);
	}

	public bool FromString(string str, char portDelineator)
	{
		return RakNetPINVOKE.SystemAddress_FromString__SWIG_1(swigCPtr, str, portDelineator);
	}

	public bool FromString(string str)
	{
		return RakNetPINVOKE.SystemAddress_FromString__SWIG_2(swigCPtr, str);
	}

	public bool FromStringExplicitPort(string str, ushort port, int ipVersion)
	{
		return RakNetPINVOKE.SystemAddress_FromStringExplicitPort__SWIG_0(swigCPtr, str, port, ipVersion);
	}

	public bool FromStringExplicitPort(string str, ushort port)
	{
		return RakNetPINVOKE.SystemAddress_FromStringExplicitPort__SWIG_1(swigCPtr, str, port);
	}

	public void CopyPort(SystemAddress right)
	{
		RakNetPINVOKE.SystemAddress_CopyPort(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool EqualsExcludingPort(SystemAddress right)
	{
		bool result = RakNetPINVOKE.SystemAddress_EqualsExcludingPort(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ushort GetPort()
	{
		return RakNetPINVOKE.SystemAddress_GetPort(swigCPtr);
	}

	public ushort GetPortNetworkOrder()
	{
		return RakNetPINVOKE.SystemAddress_GetPortNetworkOrder(swigCPtr);
	}

	public void SetPortHostOrder(ushort s)
	{
		RakNetPINVOKE.SystemAddress_SetPortHostOrder(swigCPtr, s);
	}

	public void SetPortNetworkOrder(ushort s)
	{
		RakNetPINVOKE.SystemAddress_SetPortNetworkOrder(swigCPtr, s);
	}

	public bool SetBinaryAddress(string str, char portDelineator)
	{
		return RakNetPINVOKE.SystemAddress_SetBinaryAddress__SWIG_0(swigCPtr, str, portDelineator);
	}

	public bool SetBinaryAddress(string str)
	{
		return RakNetPINVOKE.SystemAddress_SetBinaryAddress__SWIG_1(swigCPtr, str);
	}

	public void ToString_Old(bool writePort, string dest, char portDelineator)
	{
		RakNetPINVOKE.SystemAddress_ToString_Old__SWIG_0(swigCPtr, writePort, dest, portDelineator);
	}

	public void ToString_Old(bool writePort, string dest)
	{
		RakNetPINVOKE.SystemAddress_ToString_Old__SWIG_1(swigCPtr, writePort, dest);
	}

	public void FixForIPVersion(SystemAddress boundAddressToSocket)
	{
		RakNetPINVOKE.SystemAddress_FixForIPVersion(swigCPtr, getCPtr(boundAddressToSocket));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool IsLANAddress()
	{
		return RakNetPINVOKE.SystemAddress_IsLANAddress(swigCPtr);
	}

	public SystemAddress CopyData(SystemAddress input)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.SystemAddress_CopyData(swigCPtr, getCPtr(input)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Equals(SystemAddress right)
	{
		bool result = RakNetPINVOKE.SystemAddress_Equals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(SystemAddress right)
	{
		bool result = RakNetPINVOKE.SystemAddress_OpNotEqual(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreater(SystemAddress right)
	{
		bool result = RakNetPINVOKE.SystemAddress_OpGreater(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLess(SystemAddress right)
	{
		bool result = RakNetPINVOKE.SystemAddress_OpLess(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
