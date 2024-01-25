using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
//	This class purpose to give access to the part of BlobAsset without actual reference to it
//	Do not use this class in serialization contexts, because internal pointer will be invalidated!
public unsafe struct ExternalBlobPtr<T> where T: unmanaged
{
	[NativeDisableUnsafePtrRestriction]
	internal void* ptr;

	public unsafe static ExternalBlobPtr<T> Create(ref T obj)
	{
		var rv = new ExternalBlobPtr<T>();
		rv.ptr = UnsafeUtility.AddressOf(ref obj);
		return rv;
	}

	public unsafe static ExternalBlobPtr<T> Create(ref BlobPtr<T> obj)
	{
		var rv = new ExternalBlobPtr<T>();
		rv.ptr = obj.GetUnsafePtr();
		return rv;
	}

	public ref T Value
	{
		get
		{
			ValidateNotNull();
			return ref UnsafeUtility.AsRef<T>(ptr);
		}
	}

	public bool IsCreated => ptr != null;

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	public void ValidateNotNull()
	{
		if(ptr == null)
			throw new InvalidOperationException("The BlobAssetReference is null.");
	}
}
}

