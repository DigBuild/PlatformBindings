using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdvancedDLSupport;

namespace DigBuild.Platform.Util
{
    [NativeSymbols(Prefix = "dbp_native_buffer_", SymbolTransformationMethod = SymbolTransformationMethod.Underscore)]
    internal interface INativeBufferBindings
    {
        IntPtr Create(uint initialCapacity, ref IntPtr ptr, ref uint capacity);

        void Reserve(IntPtr instance, uint minCapacity, ref IntPtr ptr, ref uint capacity);
    }

    internal sealed class NativeBuffer : IDisposable
    {
        private const uint GrowthRate = 16;

        internal static readonly INativeBufferBindings Bindings = NativeLib.Get<INativeBufferBindings>();

        internal static uint CalculateCapacity(uint capacity)
        {
            return GrowthRate * (uint)MathF.Ceiling((capacity + 1f) / GrowthRate);
        }
        
        private readonly NativeHandle _instance;

        internal IntPtr Ptr { get; private set; }
        internal uint Capacity { get; private set; }
        private bool _valid = true;

        public NativeBuffer(uint initialCapacity = 0)
        {
            var ptr = IntPtr.Zero;
            var capacity = 0u;
            _instance = new NativeHandle(
                Bindings.Create(
                    initialCapacity,
                    ref ptr, ref capacity
                )
            );
            Ptr = ptr;
            Capacity = capacity;
        }

        internal void Reserve(uint minCapacity)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer));
            var ptr = IntPtr.Zero;
            var capacity = 0u;
            Bindings.Reserve(_instance, minCapacity, ref ptr, ref capacity);
            Ptr = ptr;
            Capacity = capacity;
        }

        internal NativeBuffer<T> Typed<T>() where T : unmanaged
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer));
            return new NativeBuffer<T>(this, true);
        }

        public void Dispose()
        {
            _instance.Dispose();
            _valid = false;
        }
    }

    public interface INativeBuffer<T> : IEnumerable<T> where T : unmanaged
    {
        internal IntPtr Ptr { get; }

        public uint Capacity { get; }
        public uint Count { get; }

        public void Add(T value);
        public void Add(params T[] values);
        public void Add(IEnumerable<T> values);

        public void Set(uint index, T value);
        public void Set(uint index, params T[] values);

        public void Clear();

        public void Reserve(uint minCapacity);

        public ref T this[uint i] { get; }
    }
    
    public sealed unsafe class NativeBuffer<T> : INativeBuffer<T>, IDisposable where T : unmanaged
    {
        private readonly NativeBuffer _buf;
        private readonly bool _borrowed;
        private uint _capacity, _count;
        private bool _valid = true;

        IntPtr INativeBuffer<T>.Ptr => _buf.Ptr;
        private T* TypedPtr => (T*) _buf.Ptr.ToPointer();

        internal NativeBuffer(NativeBuffer buf, bool borrowed)
        {
            _buf = buf;
            _borrowed = borrowed;
            _capacity = buf.Capacity / (uint) sizeof(T);
            _count = 0;
        }

        public NativeBuffer(uint initialCapacity = 0) : this(
            new NativeBuffer(NativeBuffer.CalculateCapacity(initialCapacity) * (uint)sizeof(T)), false)
        {
        }

        public void Dispose()
        {
            if (!_borrowed)
                _buf.Dispose();
            _valid = false;
        }

        public uint Capacity
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(NativeBuffer<T>));
                return _capacity;
            }
        }

        public uint Count
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(NativeBuffer<T>));
                return _count;
            }
        }

        public void Add(T value)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (_count == _capacity)
                Reserve(_capacity + 1);

            TypedPtr[_count] = value;
            _count++;
        }

        public void Add(params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (_count + values.Length >= _capacity)
                Reserve(_capacity + (uint)values.Length);

            for (var i = 0; i < values.Length; i++)
                TypedPtr[_count + i] = values[i];
            _count += (uint) values.Length;
        }

        public void Add(IEnumerable<T> values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));

            foreach (var v in values)
            {
                if (_count + 1 >= _capacity)
                    Reserve(_capacity + 1);
                TypedPtr[_count] = v;
                _count++;
            }
        }

        public void Set(uint index, T value)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (index >= _capacity)
                Reserve(index + 1);

            TypedPtr[_count] = value;
            _count = System.Math.Max(_count, index + 1);
        }

        public void Set(uint index, params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (index + values.Length >= _capacity)
                Reserve(index + (uint) values.Length);

            for (var i = 0; i < values.Length; i++)
                TypedPtr[index + i] = values[i];
            _count = System.Math.Max(_count, index + (uint) values.Length);
        }

        public void Clear()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            _count = 0;
        }

        public void Reserve(uint minCapacity)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            _buf.Reserve(NativeBuffer.CalculateCapacity(minCapacity) * (uint)sizeof(T));
            _capacity = _buf.Capacity / (uint)sizeof(T);
        }

        public ref T this[uint i]
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(NativeBuffer<T>));
                if (i >= _capacity)
                    throw new IndexOutOfRangeException("Not enough space in buffer.");
                if (i >= _count)
                    throw new IndexOutOfRangeException("No element at the specified index.");
                return ref TypedPtr[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetUnsafe(uint i)
        {
            return TypedPtr[i];
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            
            for (var i = 0u; i < Count; i++)
                yield return GetUnsafe(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class NativeBufferPool
    {
        private readonly ConcurrentQueue<NativeBuffer> _buffers = new();

        public PooledNativeBuffer<T> Request<T>() where T : unmanaged
        {
            if (!_buffers.TryDequeue(out var backingBuffer))
                backingBuffer = new NativeBuffer();
            return new PooledNativeBuffer<T>(backingBuffer, _buffers);
        }
    }

    public sealed class PooledNativeBuffer<T> : INativeBuffer<T>, IDisposable where T : unmanaged
    {
        private readonly NativeBuffer _backingBuffer;
        private readonly ConcurrentQueue<NativeBuffer> _queue;
        private readonly NativeBuffer<T> _buffer;
        private bool _valid = true;

        IntPtr INativeBuffer<T>.Ptr => _backingBuffer.Ptr;

        internal PooledNativeBuffer(NativeBuffer backingBuffer, ConcurrentQueue<NativeBuffer> queue)
        {
            _backingBuffer = backingBuffer;
            _queue = queue;
            _buffer = backingBuffer.Typed<T>();
        }

        public void Dispose()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _valid = false;
            _buffer.Dispose();
            _queue.Enqueue(_backingBuffer);
        }

        public NativeBuffer<T> Unpooled
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                return _buffer;
            }
        }

        public uint Capacity
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                return _buffer.Capacity;
            }
        }

        public uint Count
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                return _buffer.Count;
            }
        }

        public void Add(T value)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Add(value);
        }

        public void Add(params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Add(values);
        }

        public void Add(IEnumerable<T> values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Add(values);
        }

        public void Set(uint index, T value)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Set(index, value);
        }

        public void Set(uint index, params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Set(index, values);
        }

        public void Clear()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Clear();
        }

        public void Reserve(uint minCapacity)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _buffer.Reserve(minCapacity);
        }

        public ref T this[uint i]
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                return ref _buffer[i];
            }
        }

        public IEnumerator<T> GetEnumerator() => _buffer.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}