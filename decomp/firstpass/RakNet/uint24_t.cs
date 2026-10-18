using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class uint24_t : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public uint val
	{
		get
		{
			return RakNetPINVOKE.uint24_t_val_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.uint24_t_val_set(swigCPtr, value);
		}
	}

	internal uint24_t(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public uint24_t()
		: this(RakNetPINVOKE.new_uint24_t__SWIG_0(), cMemoryOwn: true)
	{
	}

	public uint24_t(uint24_t a)
		: this(RakNetPINVOKE.new_uint24_t__SWIG_1(getCPtr(a)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public uint24_t(uint a)
		: this(RakNetPINVOKE.new_uint24_t__SWIG_2(a), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(uint24_t obj)
	{
		return (!(obj == null)) ? obj.swigCPtr : new HandleRef(null, IntPtr.Zero);
	}

	~uint24_t()
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
					RakNetPINVOKE.delete_uint24_t(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public override int GetHashCode()
	{
		return (int)val;
	}

	public static implicit operator uint24_t(uint inUint)
	{
		return new uint24_t(inUint);
	}

	public static bool operator ==(uint24_t a, uint24_t b)
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

	public static bool operator !=(uint24_t a, uint24_t b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator <(uint24_t a, uint24_t b)
	{
		return a.OpLess(b);
	}

	public static bool operator >(uint24_t a, uint24_t b)
	{
		return a.OpGreater(b);
	}

	public static uint24_t operator +(uint24_t a, uint24_t b)
	{
		return a.OpPlus(b);
	}

	public static uint24_t operator ++(uint24_t a)
	{
		return a.OpPlusPlus();
	}

	public static uint24_t operator --(uint24_t a)
	{
		return a.OpMinusMinus();
	}

	public static uint24_t operator *(uint24_t a, uint24_t b)
	{
		return a.OpMultiply(b);
	}

	public static uint24_t operator /(uint24_t a, uint24_t b)
	{
		return a.OpDivide(b);
	}

	public static uint24_t operator -(uint24_t a, uint24_t b)
	{
		return a.OpMinus(b);
	}

	public static bool operator ==(uint24_t a, uint b)
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

	public static bool operator !=(uint24_t a, uint b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator <(uint24_t a, uint b)
	{
		return a.OpLess(b);
	}

	public static bool operator >(uint24_t a, uint b)
	{
		return a.OpGreater(b);
	}

	public static uint24_t operator +(uint24_t a, uint b)
	{
		return a.OpPlus(b);
	}

	public static uint24_t operator *(uint24_t a, uint b)
	{
		return a.OpMultiply(b);
	}

	public static uint24_t operator /(uint24_t a, uint b)
	{
		return a.OpDivide(b);
	}

	public static uint24_t operator -(uint24_t a, uint b)
	{
		return a.OpMinus(b);
	}

	public override string ToString()
	{
		return val.ToString();
	}

	private uint24_t OpPlusPlus()
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpPlusPlus(swigCPtr), cMemoryOwn: true);
	}

	private uint24_t OpMinusMinus()
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpMinusMinus(swigCPtr), cMemoryOwn: true);
	}

	public uint24_t CopyData(uint24_t a)
	{
		uint24_t result = new uint24_t(RakNetPINVOKE.uint24_t_CopyData__SWIG_0(swigCPtr, getCPtr(a)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Equals(uint24_t right)
	{
		bool result = RakNetPINVOKE.uint24_t_Equals__SWIG_0(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(uint24_t right)
	{
		bool result = RakNetPINVOKE.uint24_t_OpNotEqual__SWIG_0(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreater(uint24_t right)
	{
		bool result = RakNetPINVOKE.uint24_t_OpGreater__SWIG_0(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLess(uint24_t right)
	{
		bool result = RakNetPINVOKE.uint24_t_OpLess__SWIG_0(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint24_t OpPlus(uint24_t other)
	{
		uint24_t result = new uint24_t(RakNetPINVOKE.uint24_t_OpPlus__SWIG_0(swigCPtr, getCPtr(other)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint24_t OpMinus(uint24_t other)
	{
		uint24_t result = new uint24_t(RakNetPINVOKE.uint24_t_OpMinus__SWIG_0(swigCPtr, getCPtr(other)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint24_t OpDivide(uint24_t other)
	{
		uint24_t result = new uint24_t(RakNetPINVOKE.uint24_t_OpDivide__SWIG_0(swigCPtr, getCPtr(other)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint24_t OpMultiply(uint24_t other)
	{
		uint24_t result = new uint24_t(RakNetPINVOKE.uint24_t_OpMultiply__SWIG_0(swigCPtr, getCPtr(other)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public uint24_t CopyData(uint a)
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_CopyData__SWIG_1(swigCPtr, a), cMemoryOwn: false);
	}

	public bool Equals(uint right)
	{
		return RakNetPINVOKE.uint24_t_Equals__SWIG_1(swigCPtr, right);
	}

	private bool OpNotEqual(uint right)
	{
		return RakNetPINVOKE.uint24_t_OpNotEqual__SWIG_1(swigCPtr, right);
	}

	private bool OpGreater(uint right)
	{
		return RakNetPINVOKE.uint24_t_OpGreater__SWIG_1(swigCPtr, right);
	}

	private bool OpLess(uint right)
	{
		return RakNetPINVOKE.uint24_t_OpLess__SWIG_1(swigCPtr, right);
	}

	private uint24_t OpPlus(uint other)
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpPlus__SWIG_1(swigCPtr, other), cMemoryOwn: true);
	}

	private uint24_t OpMinus(uint other)
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpMinus__SWIG_1(swigCPtr, other), cMemoryOwn: true);
	}

	private uint24_t OpDivide(uint other)
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpDivide__SWIG_1(swigCPtr, other), cMemoryOwn: true);
	}

	private uint24_t OpMultiply(uint other)
	{
		return new uint24_t(RakNetPINVOKE.uint24_t_OpMultiply__SWIG_1(swigCPtr, other), cMemoryOwn: true);
	}
}
