using System;
using System.Runtime.InteropServices;

namespace ImGuiNET.Unity
{
  class PinnedArray<T>
  {
    public T[] Array { get { return _data; } }
    public IntPtr ArrayPtr { get { return _nativePtr; } }


    T[] _data;


    GCHandle _gcHandle;
    IntPtr _nativePtr;



    public T this[int index]
    {
      get => _data[index];
      set => _data[index] = value;
    }

    public int Length { get { return _data.Length; } }

    public PinnedArray(int size)
    {
      _data = new T[size];

      _gcHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
      _nativePtr = _gcHandle.AddrOfPinnedObject();
    }

    public void Release()
    {
      if (_gcHandle.IsAllocated)
      {
        _gcHandle.Free();
      }
      _nativePtr = IntPtr.Zero;
      _data = null;
    }
  }
}
