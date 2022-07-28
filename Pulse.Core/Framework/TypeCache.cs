using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    public static class TypeCache<T>
    {
        public static readonly Type Type = typeof(T);
    }

    public static class UnsafeTypeCache<T>
    {
        public static readonly int UnsafeSize = GetSize();
        public static readonly UIntPtr ArrayTypePointer = GetArrayTypePointer();

        private static int GetSize()
        {
            DynamicMethod dynamicMethod = new("SizeOf", typeof(int), Type.EmptyTypes);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Sizeof, TypeCache<T>.Type);
            generator.Emit(OpCodes.Ret);

            return ((Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>)))();
        }

        private static unsafe UIntPtr GetArrayTypePointer()
        {
            T[] result = new T[1];
            using (SafeGCHandle handle = new(result, GCHandleType.Pinned))
                return *(((UIntPtr*)handle.AddrOfPinnedObject().ToPointer()) - 2);
        }

        public static IDisposable ChangeArrayType(Array array, int oldElementSize)
        {
            unsafe
            {
                void* pinned;
                return ChangeArrayType(array, oldElementSize, out pinned);
            }
        }

        public static unsafe IDisposable ChangeArrayType(Array array, int oldElementSize, out void* pointer)
        {
            if (array.Length < 1)
                throw new NotSupportedException();

            SafeGCHandle handle = new(array, GCHandleType.Pinned);
            try
            {
                pointer = handle.AddrOfPinnedObject().ToPointer();
                UIntPtr* arrayPointer = (UIntPtr*)pointer;
                UIntPtr arrayLength = *(arrayPointer - 1);
                UIntPtr arrayType = *(arrayPointer - 2);
                ulong arraySize = ((ulong)arrayLength * (ulong)oldElementSize);

                if (arraySize % (ulong)UnsafeSize != 0)
                    throw new InvalidCastException();

                try
                {
                    *(arrayPointer - 1) = new(arraySize / (ulong)UnsafeSize);
                    *(arrayPointer - 2) = ArrayTypePointer;

                    return new DisposableAction(() =>
                    {
                        *(arrayPointer - 1) = arrayLength;
                        *(arrayPointer - 2) = arrayType;
                        handle.Dispose();
                    });
                }
                catch
                {
                    *(arrayPointer - 1) = arrayLength;
                    *(arrayPointer - 2) = arrayType;
                    throw;
                }
            }
            catch
            {
                handle.SafeDispose();
                throw;
            }
        }
    }
}