using System;
using System.Collections.Generic;

namespace EasyObjectPool
{
    /// <summary>
    /// Poolを使うために必要なinterface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolCollection<T>
    {
        void SetCapacity(int capacity);
        int GetCount();
        void Push(T value);
        T Pop();
        void Clear();
    }

    /// <summary>
    /// Stack型のコレクション
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StackPool<T> : IPoolCollection<T>
    {
        private readonly Stack<T> _stack;
        public StackPool() => _stack = new Stack<T>();
        public void SetCapacity(int capacity) => _stack.EnsureCapacity(capacity);
        public int GetCount() => _stack.Count;
        public void Push(T value) => _stack.Push(value);
        public T Pop() => _stack.Pop();
        public void Clear() => _stack.Clear();
    }

    /// <summary>
    /// LinkedList型のコレクション
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedPool<T> : IPoolCollection<T>
    {
        private readonly LinkedList<T> _list;
        private int _capacity = default;
        public LinkedPool() => _list = new LinkedList<T>();
        public void SetCapacity(int capacity) => _capacity = capacity;
        public int GetCount() => _list.Count;
        public void Push(T value)
        {
            if (_capacity >= _list.Count)
            {
                return;
            }

            _list.AddLast(value);
        }
        public T Pop()
        {
            var last = _list.Last!.Value;
            _list.RemoveLast();
            return last;
        }
        public void Clear() => _list.Clear();
    }

    /// <summary>
    /// 汎用的なオブジェクトプール
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IDisposable
    {
        public IPoolCollection<T> PoolCollection { get; private set; }
        public bool IsDispose { get; private set; }


        public const int MaxPoolLimit = int.MaxValue;
        public int Count => PoolCollection == null ? 0 : PoolCollection.GetCount();


        private readonly Func<T>? CreateInstance;
        private readonly Action<T>? OnReturn;
        private readonly Action<T>? OnBorrow;
        private readonly Action? OnClear;


        /// <summary>
        /// オブジェクトプールの作成
        /// </summary>
        /// <param name="CreateInstance">プールに作成されるオブジェクトを作成する関数</param>
        /// <param name="OnReturn">返却時のイベント</param>
        /// <param name="OnBorrow">貸与時のイベント</param>
        /// <param name="OnClear">削除時のイベント</param>
        /// <param name="PoolCollection">プールの種類</param>
        /// <param name="capacity">容量</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ObjectPool(Func<T> CreateInstance, Action<T>? OnReturn = null, Action<T>? OnBorrow = null, Action? OnClear = null, IPoolCollection<T>? PoolCollection = null, int capacity = 256)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            this.PoolCollection = PoolCollection ?? new StackPool<T>();
            this.PoolCollection.SetCapacity(capacity);
            this.CreateInstance = CreateInstance ?? throw new Exception("Can't Create Instance.");

            if (OnReturn != null)
                this.OnReturn = OnReturn;
            if (OnBorrow != null)
                this.OnBorrow = OnBorrow;
            if (OnClear != null)
                this.OnClear = OnClear;

            for (int i = 0; i < capacity; i++)
            {
                this.PoolCollection.Push(CreateInstance.Invoke());
            }
        }

        /// <summary>
        /// オブジェクトを返却します
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Return(T obj)
        {
            IsDisposed();
            if (obj == null)
            {
                throw new ArgumentException(nameof(obj) + "is null");
            }

            if ((PoolCollection.GetCount() + 1) == MaxPoolLimit)
            {
                throw new InvalidOperationException("Reaced Max Poolsize");
            }

            OnReturn?.Invoke(obj);
            PoolCollection.Push(obj);
        }

        /// <summary>
        /// オブジェクトを借りる
        /// </summary>
        /// <returns></returns>
        public T Borrow()
        {
            Borrow(out T obj);
            return obj;
        }

        /// <summary>
        /// オブジェクトを借りる
        /// </summary>
        /// <returns></returns>
        public void Borrow(out T obj)
        {
            IsDisposed();
            obj = PoolCollection.GetCount() == 0 ? CreateInstance.Invoke() : PoolCollection.Pop();
            OnBorrow?.Invoke(obj);
        }

        /// <summary>
        /// プールの削除
        /// </summary>
        public void Clear()
        {
            IsDisposed();
            OnClear?.Invoke();
            PoolCollection?.Clear();
        }

        private void IsDisposed()
        {
            if (IsDispose)
            {
                throw new ArgumentNullException("ObjectPool is Disposed.");
            }
        }

        /// <summary>
        /// プールのメモリ解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDispose)
                throw new ObjectDisposedException(nameof(ObjectPool<T>));

            if (disposing)
            {
                Clear();
            }

            IsDispose = true;
        }

        ~ObjectPool()
        {
            Dispose(false);
        }
    }
}
