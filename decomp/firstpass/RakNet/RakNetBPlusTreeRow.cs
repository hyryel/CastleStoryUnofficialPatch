using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetBPlusTreeRow : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal RakNetBPlusTreeRow(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetBPlusTreeRow()
		: this(RakNetPINVOKE.new_RakNetBPlusTreeRow(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(RakNetBPlusTreeRow obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetBPlusTreeRow()
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
					RakNetPINVOKE.delete_RakNetBPlusTreeRow(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void SetPoolPageSize(int size)
	{
		RakNetPINVOKE.RakNetBPlusTreeRow_SetPoolPageSize(swigCPtr, size);
	}

	public bool Insert(uint key, Row data)
	{
		return RakNetPINVOKE.RakNetBPlusTreeRow_Insert(swigCPtr, key, Row.getCPtr(data));
	}

	public void Clear()
	{
		RakNetPINVOKE.RakNetBPlusTreeRow_Clear(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetBPlusTreeRow_Size(swigCPtr);
	}

	public bool IsEmpty()
	{
		return RakNetPINVOKE.RakNetBPlusTreeRow_IsEmpty(swigCPtr);
	}

	public RakNetPageRow GetListHead()
	{
		IntPtr intPtr = RakNetPINVOKE.RakNetBPlusTreeRow_GetListHead(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new RakNetPageRow(intPtr, cMemoryOwn: false) : null;
	}

	public Row GetDataHead()
	{
		IntPtr intPtr = RakNetPINVOKE.RakNetBPlusTreeRow_GetDataHead(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null;
	}

	public void PrintLeaves()
	{
		RakNetPINVOKE.RakNetBPlusTreeRow_PrintLeaves(swigCPtr);
	}

	public void PrintGraph()
	{
		RakNetPINVOKE.RakNetBPlusTreeRow_PrintGraph(swigCPtr);
	}
}
