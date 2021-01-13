using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdvancedDLSupport;

namespace DigBuildPlatformCS.Util
{
    [NativeSymbols(Prefix = "db_buffer_", SymbolTransformationMethod = SymbolTransformationMethod.Underscore)]
    internal interface INativeBuffer
    {
        IntPtr Create(uint initialCapacity, uint elementSize, ref IntPtr ptr, ref uint capacity);

        void Reserve(IntPtr instance, uint minCapacity, ref IntPtr ptr, ref uint capacity);
    }

    internal static class NativeBuffer
    {
        private const uint GrowthRate = 16;

        internal static readonly INativeBuffer Native = null!; //NativeLib.Get<INativeBuffer>();

        internal static uint CalculateCapacity(uint capacity)
        {
            return GrowthRate * (uint) MathF.Ceiling((capacity + 1f) / GrowthRate);
        }
    }

    public sealed unsafe class NativeBuffer<T> : IEnumerable<T>, IDisposable where T : unmanaged
    {
        private readonly NativeHandle _instance;

        private T* _ptr;
        private uint _capacity, _count;
        private bool _valid = true;

        public NativeBuffer(uint initialCapacity = 0)
        {
            var ptr = IntPtr.Zero;
            _instance = new NativeHandle(
                NativeBuffer.Native.Create(
                    NativeBuffer.CalculateCapacity(initialCapacity),
                    (uint) sizeof(T),
                    ref ptr, ref _capacity
                )
            );
            _ptr = (T*) ptr.ToPointer();
        }

        public void Dispose()
        {
            _instance.Dispose();
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
                Reserve(NativeBuffer.CalculateCapacity(_capacity + 1));

            _ptr[_count] = value;
            _count++;
        }

        public void Add(params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (_count + values.Length >= _capacity)
                Reserve(NativeBuffer.CalculateCapacity(_capacity + (uint) values.Length));

            for (var i = 0; i < values.Length; i++)
                _ptr[_count + i] = values[i];
            _count += (uint) values.Length;
        }

        public void Set(uint index, T value)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (index >= _capacity)
                Reserve(NativeBuffer.CalculateCapacity(index + 1));

            _ptr[_count] = value;
            _count = System.Math.Max(_count, index + 1);
        }

        public void Set(uint index, params T[] values)
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            if (index + values.Length >= _capacity)
                Reserve(NativeBuffer.CalculateCapacity(index + (uint) values.Length));

            for (var i = 0; i < values.Length; i++)
                _ptr[index + i] = values[i];
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
            var ptr = IntPtr.Zero;
            NativeBuffer.Native.Reserve(_instance, minCapacity, ref ptr, ref _capacity);
            _ptr = (T*) ptr.ToPointer();
        }

        public T this[uint i]
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(NativeBuffer<T>));
                if (i >= _capacity)
                    throw new IndexOutOfRangeException("Not enough space in buffer.");
                return i < _count ? _ptr[i] : default;
            }
            set
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(NativeBuffer<T>));
                if (i >= _capacity)
                    throw new IndexOutOfRangeException("Not enough space in buffer.");
                _ptr[i] = value;
                if (_count <= i)
                    _count = i + 1;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(NativeBuffer<T>));
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private readonly NativeBuffer<T> _buffer;
            private uint _index;

            public Enumerator(NativeBuffer<T> buffer)
            {
                this._buffer = buffer;
            }

            public bool MoveNext()
            {
                _index++;
                return _index >= _buffer.Count;
            }

            public void Reset()
            {
                _index = 0;
            }

            public T Current => _buffer[_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }

    internal class NativeBufferPool<T> where T : unmanaged
    {
        private static readonly ConditionalWeakTable<NativeBufferPool, Lazy<NativeBufferPool<T>>> Store = new();

        internal static NativeBufferPool<T> Of(NativeBufferPool pool)
        {
            return Store.GetOrCreateValue(pool).Value;
        }

        internal readonly Queue<NativeBuffer<T>> Buffers = new();
    }

    public sealed class NativeBufferPool
    {
        public PooledNativeBuffer<T> Request<T>() where T : unmanaged
        {
            Queue<NativeBuffer<T>> buffers = NativeBufferPool<T>.Of(this).Buffers;
            var buffer = buffers.Count > 0 ? buffers.Dequeue() : new NativeBuffer<T>();
            return new PooledNativeBuffer<T>(buffer, buffers);
        }
    }

    public sealed class PooledNativeBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly NativeBuffer<T> _buffer;
        private readonly Queue<NativeBuffer<T>> _queue;
        private bool _valid = true;

        internal PooledNativeBuffer(NativeBuffer<T> buffer, Queue<NativeBuffer<T>> queue)
        {
            _buffer = buffer;
            _queue = queue;
        }

        public void Dispose()
        {
            if (!_valid)
                throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
            _valid = false;
            _buffer.Clear();
            _queue.Enqueue(_buffer);
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

        public T this[uint i]
        {
            get
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                return _buffer[i];
            }
            set
            {
                if (!_valid)
                    throw new ObjectDisposedException(nameof(PooledNativeBuffer<T>));
                _buffer[i] = value;
            }
        }
    }
}