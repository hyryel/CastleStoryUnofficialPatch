using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakWString : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal RakWString(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakWString()
		: this(RakNetPINVOKE.new_RakWString__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakWString(RakString right)
		: this(RakNetPINVOKE.new_RakWString__SWIG_1(RakString.getCPtr(right)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakWString(SWIGTYPE_p_wchar_t input)
		: this(RakNetPINVOKE.new_RakWString__SWIG_2(SWIGTYPE_p_wchar_t.getCPtr(input)), cMemoryOwn: true)
	{
	}

	public RakWString(RakWString right)
		: this(RakNetPINVOKE.new_RakWString__SWIG_3(getCPtr(right)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakWString(string input)
		: this(RakNetPINVOKE.new_RakWString__SWIG_4(input), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(RakWString obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakWString()
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
					RakNetPINVOKE.delete_RakWString(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public SWIGTYPE_p_wchar_t C_String()
	{
		IntPtr intPtr = RakNetPINVOKE.RakWString_C_String(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_wchar_t(intPtr, futureUse: false) : null;
	}

	public RakWString CopyData(RakWString right)
	{
		RakWString result = new RakWString(RakNetPINVOKE.RakWString_CopyData__SWIG_0(swigCPtr, getCPtr(right)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakWString CopyData(RakString right)
	{
		RakWString result = new RakWString(RakNetPINVOKE.RakWString_CopyData__SWIG_1(swigCPtr, RakString.getCPtr(right)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakWString CopyData(SWIGTYPE_p_wchar_t str)
	{
		return new RakWString(RakNetPINVOKE.RakWString_CopyData__SWIG_2(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(str)), cMemoryOwn: false);
	}

	public RakWString CopyData(string str)
	{
		return new RakWString(RakNetPINVOKE.RakWString_CopyData__SWIG_4(swigCPtr, str), cMemoryOwn: false);
	}

	public bool Equals(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_Equals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLess(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_OpLess(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLessEquals(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_OpLessEquals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreater(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_OpGreater(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreaterEquals(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_OpGreaterEquals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(RakWString right)
	{
		bool result = RakNetPINVOKE.RakWString_OpNotEqual(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Set(SWIGTYPE_p_wchar_t str)
	{
		RakNetPINVOKE.RakWString_Set(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(str));
	}

	public bool IsEmpty()
	{
		return RakNetPINVOKE.RakWString_IsEmpty(swigCPtr);
	}

	public uint GetLength()
	{
		return RakNetPINVOKE.RakWString_GetLength(swigCPtr);
	}

	public static uint ToInteger(RakWString rs)
	{
		uint result = RakNetPINVOKE.RakWString_ToInteger(getCPtr(rs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public int StrCmp(RakWString right)
	{
		int result = RakNetPINVOKE.RakWString_StrCmp(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public int StrICmp(RakWString right)
	{
		int result = RakNetPINVOKE.RakWString_StrICmp(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		RakNetPINVOKE.RakWString_Clear(swigCPtr);
	}

	public void Printf()
	{
		RakNetPINVOKE.RakWString_Printf(swigCPtr);
	}

	public void FPrintf(SWIGTYPE_p_FILE fp)
	{
		RakNetPINVOKE.RakWString_FPrintf(swigCPtr, SWIGTYPE_p_FILE.getCPtr(fp));
	}

	public void Serialize(BitStream bs)
	{
		RakNetPINVOKE.RakWString_Serialize__SWIG_0(swigCPtr, BitStream.getCPtr(bs));
	}

	public static void Serialize(SWIGTYPE_p_wchar_t str, BitStream bs)
	{
		RakNetPINVOKE.RakWString_Serialize__SWIG_1(SWIGTYPE_p_wchar_t.getCPtr(str), BitStream.getCPtr(bs));
	}

	public bool Deserialize(BitStream bs)
	{
		return RakNetPINVOKE.RakWString_Deserialize__SWIG_0(swigCPtr, BitStream.getCPtr(bs));
	}

	public static bool Deserialize(SWIGTYPE_p_wchar_t str, BitStream bs)
	{
		return RakNetPINVOKE.RakWString_Deserialize__SWIG_1(SWIGTYPE_p_wchar_t.getCPtr(str), BitStream.getCPtr(bs));
	}
}
