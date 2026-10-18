using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SortQuery : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public uint columnIndex
	{
		get
		{
			return RakNetPINVOKE.SortQuery_columnIndex_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SortQuery_columnIndex_set(swigCPtr, value);
		}
	}

	public Table.SortQueryType operation
	{
		get
		{
			return (Table.SortQueryType)RakNetPINVOKE.SortQuery_operation_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.SortQuery_operation_set(swigCPtr, (int)value);
		}
	}

	internal SortQuery(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public SortQuery()
		: this(RakNetPINVOKE.new_SortQuery(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(SortQuery obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~SortQuery()
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
					RakNetPINVOKE.delete_SortQuery(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
