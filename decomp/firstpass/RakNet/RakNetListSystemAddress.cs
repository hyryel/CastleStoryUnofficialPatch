using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListSystemAddress : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public SystemAddress this[int index]
	{
		get
		{
			return Get((uint)index);
		}
		set
		{
			Replace(value, value, (uint)index, "Not used", 0u);
		}
	}

	internal RakNetListSystemAddress(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListSystemAddress()
		: this(RakNetPINVOKE.new_RakNetListSystemAddress__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListSystemAddress(RakNetListSystemAddress original_copy)
		: this(RakNetPINVOKE.new_RakNetListSystemAddress__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListSystemAddress obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListSystemAddress()
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
					RakNetPINVOKE.delete_RakNetListSystemAddress(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListSystemAddress CopyData(RakNetListSystemAddress original_copy)
	{
		RakNetListSystemAddress result = new RakNetListSystemAddress(RakNetPINVOKE.RakNetListSystemAddress_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public SystemAddress Get(uint position)
	{
		return new SystemAddress(RakNetPINVOKE.RakNetListSystemAddress_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(SystemAddress input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Push(swigCPtr, SystemAddress.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public SystemAddress Pop()
	{
		return new SystemAddress(RakNetPINVOKE.RakNetListSystemAddress_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(SystemAddress input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Insert__SWIG_0(swigCPtr, SystemAddress.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(SystemAddress input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Insert__SWIG_1(swigCPtr, SystemAddress.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(SystemAddress input, SystemAddress filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Replace__SWIG_0(swigCPtr, SystemAddress.getCPtr(input), SystemAddress.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(SystemAddress input)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Replace__SWIG_1(swigCPtr, SystemAddress.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListSystemAddress_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListSystemAddress_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListSystemAddress_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListSystemAddress_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(SystemAddress input)
	{
		uint result = RakNetPINVOKE.RakNetListSystemAddress_GetIndexOf(swigCPtr, SystemAddress.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListSystemAddress_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListSystemAddress_Compress(swigCPtr, file, line);
	}
}
