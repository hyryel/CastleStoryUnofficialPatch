using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListTableRow : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public Row this[int index]
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

	internal RakNetListTableRow(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListTableRow()
		: this(RakNetPINVOKE.new_RakNetListTableRow__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListTableRow(RakNetListTableRow original_copy)
		: this(RakNetPINVOKE.new_RakNetListTableRow__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListTableRow obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListTableRow()
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
					RakNetPINVOKE.delete_RakNetListTableRow(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListTableRow CopyData(RakNetListTableRow original_copy)
	{
		RakNetListTableRow result = new RakNetListTableRow(RakNetPINVOKE.RakNetListTableRow_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public Row Get(uint position)
	{
		return new Row(RakNetPINVOKE.RakNetListTableRow_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(Row input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Push(swigCPtr, Row.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public Row Pop()
	{
		return new Row(RakNetPINVOKE.RakNetListTableRow_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(Row input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Insert__SWIG_0(swigCPtr, Row.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(Row input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Insert__SWIG_1(swigCPtr, Row.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(Row input, Row filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Replace__SWIG_0(swigCPtr, Row.getCPtr(input), Row.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(Row input)
	{
		RakNetPINVOKE.RakNetListTableRow_Replace__SWIG_1(swigCPtr, Row.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListTableRow_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListTableRow_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListTableRow_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListTableRow_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListTableRow_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListTableRow_Compress(swigCPtr, file, line);
	}
}
