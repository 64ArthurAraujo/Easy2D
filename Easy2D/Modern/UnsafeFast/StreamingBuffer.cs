using Silk.NET.OpenGLES;
using System;
using Easy2D.OpenGL;

namespace Easy2D
{
    /// <summary>
    /// A fast immutable direct pointer to gpu memory which is unsynchronized, requires EXT.BufferStorage GLES 3.1+
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class StreamingBuffer<T> : GLObject where T : unmanaged
    {
        private static Silk.NET.OpenGLES.Extensions.EXT.ExtBufferStorage extBufferStorage;

        public static bool IsSupported => extBufferStorage is not null;

        static StreamingBuffer()
        {
            if (extBufferStorage is null)
                GLController.Instance.TryGetExtension<Silk.NET.OpenGLES.Extensions.EXT.ExtBufferStorage>(out extBufferStorage);
        }

        private static readonly MapBufferAccessMask mapBufferAccessMask = MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapCoherentBit;
        private static readonly BufferStorageMask bufferStorageMask = BufferStorageMask.DynamicStorageBit | (BufferStorageMask)mapBufferAccessMask;

        private BufferTargetARB bufferTarget;
        public uint Capacity { get; private set; }
        public uint SizeInBytes => Capacity * (uint)sizeof(T);

        public T* Pointer { get; private set; }

        public StreamingBuffer(BufferTargetARB bufferTarget, uint itemCapacity)
        {
            if (IsSupported == false)
            {
                Utils.Log($"This device does not support streaming buffers!", LogLevel.Error);
                throw new NotSupportedException();
            }

            this.bufferTarget = bufferTarget;
            Capacity = itemCapacity;
        }

        protected override void bind(int? slot)
        {
            GLController.Instance.BindBuffer(bufferTarget, Handle);
        }

        protected override void delete()
        {
            Pointer = null;

            GLController.Instance.DeleteBuffer(Handle);
            Handle = uint.MaxValue;
        }

        protected override void initialize(int? slot)
        {
            Handle = GLController.Instance.GenBuffer();

            bind(null);

            extBufferStorage.BufferStorage((BufferStorageTarget)bufferTarget, SizeInBytes, null, bufferStorageMask);

            Pointer = (T*)GLController.Instance.MapBufferRange(bufferTarget, 0, SizeInBytes, mapBufferAccessMask);
        }
    }
}
