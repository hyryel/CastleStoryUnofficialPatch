using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetGUID : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ulong g
	{
		get
		{
			return RakNetPINVOKE.RakNetGUID_g_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetGUID_g_set(swigCPtr, value);
		}
	}

	public ushort systemIndex
	{
		get
		{
			return RakNetPINVOKE.RakNetGUID_systemIndex_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetGUID_systemIndex_set(swigCPtr, value);
		}
	}

	internal RakNetGUID(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetGUID()
		: this(RakNetPINVOKE.new_RakNetGUID__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetGUID(ulong _g)
		: this(RakNetPINVOKE.new_RakNetGUID__SWIG_1(_g), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(RakNetGUID obj)
	{
		return (!(obj == null)) ? obj.swigCPtr : new HandleRef(null, IntPtr.Zero);
	}

	~RakNetGUID()
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
					RakNetPINVOKE.delete_RakNetGUID(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public override int GetHashCode()
	{
		return (int)ToUint32(this);
	}

	public static bool operator ==(RakNetGUID a, RakNetGUID b)
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

	public static bool operator !=(RakNetGUID a, RakNetGUID b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator <(RakNetGUID a, RakNetGUID b)
	{
		return a.OpLess(b);
	}

	public static bool operator >(RakNetGUID a, RakNetGUID b)
	{
		return a.OpGreater(b);
	}

	public static bool operator <=(RakNetGUID a, RakNetGUID b)
	{
		return a.OpLess(b) || a == b;
	}

	public static bool operator >=(RakNetGUID a, RakNetGUID b)
	{
		return a.OpGreater(b) || a == b;
	}

	public void ToString(out string dest)
	{
		dest = ToString();
	}

	public override string ToString()
	{
		return RakNetPINVOKE.RakNetGUID_ToString(swigCPtr);
	}

	public bool FromString(string source)
	{
		return RakNetPINVOKE.RakNetGUID_FromString(swigCPtr, source);
	}

	public static uint ToUint32(RakNetGUID g)
	{
		uint result = RakNetPINVOKE.RakNetGUID_ToUint32(getCPtr(g));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakNetGUID CopyData(RakNetGUID input)
	{
		RakNetGUID result = new RakNetGUID(RakNetPINVOKE.RakNetGUID_CopyData(swigCPtr, getCPtr(input)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static int size()
	{
		return RakNetPINVOKE.RakNetGUID_size();
	}

	public bool Equals(RakNetGUID right)
	{
		bool result = RakNetPINVOKE.RakNetGUID_Equals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(RakNetGUID right)
	{
		bool result = RakNetPINVOKE.RakNetGUID_OpNotEqual(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreater(RakNetGUID right)
	{
		bool result = RakNetPINVOKE.RakNetGUID_OpGreater(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLess(RakNetGUID right)
	{
		bool result = RakNetPINVOKE.RakNetGUID_OpLess(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
