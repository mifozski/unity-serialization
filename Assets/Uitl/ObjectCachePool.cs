﻿using System;
using System.Collections.Generic;
using Utils;

namespace Collections
{
    /// <summary>
    /// Creates a pool that will cache instances of objects for later use so that you don't have to construct them again.
    /// There is a max cache size, if set to 0 or less, it's considered endless in size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCachePool<T> : ICachePool<T> where T : class
    {

        #region Fields

        private HashSet<T> _inactive;

        private int _cacheSize;
        private Func<T> _constructorDelegate;
        private Action<T> _resetObjectDelegate;
        private bool _resetOnGet;

        #endregion

        #region CONSTRUCTOR

        public ObjectCachePool(int cacheSize)
        {
            _cacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = this.SimpleConstructor;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate)
        {
            _cacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate, Action<T> resetObjectDelegate)
        {
            _cacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
            _resetObjectDelegate = resetObjectDelegate;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate, Action<T> resetObjectDelegate, bool resetOnGet)
        {
            _cacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
            _resetObjectDelegate = resetObjectDelegate;
            _resetOnGet = resetOnGet;
        }

        private T SimpleConstructor()
        {
            return Activator.CreateInstance<T>();
        }

        #endregion

        #region Properties

        public int CacheSize
        {
            get { return _cacheSize; }
            set
            {
                _cacheSize = value;
            }
        }

        public bool ResetOnGet
        {
            get { return _resetOnGet; }
            set { _resetOnGet = value; }
        }

        public int InactiveCount
        {
            get { return _inactive.Count; }
        }

        #endregion

        #region Methods

        public bool TryGetInstance(out T result)
        {
            result = null;
            lock(_inactive)
            {
                if(_inactive.Count > 0)
                {
                    result = _inactive.Pop();
                }
            }
            if (result != null)
            {
                if(_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public T GetInstance()
        {
            T result = null;
            lock(_inactive)
            {
                if(_inactive.Count > 0)
                {
                    result = _inactive.Pop();
                }
            }
            if (result != null)
            {
                if (_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return result;
            }
            else
            {
                return _constructorDelegate();
            }
        }

        public bool Release(T obj)
        {
            if (obj == null) throw new System.ArgumentNullException("obj");

            int cacheSize = _cacheSize > 0 ? _cacheSize : 1024;
            if (!_resetOnGet && _resetObjectDelegate != null && _inactive.Count < cacheSize) _resetObjectDelegate(obj);

            lock(_inactive)
            {
                if(_inactive.Count < cacheSize)
                {
                    _inactive.Add(obj);
                    return true;
                }
            }

            return false;
        }

        void ICachePool<T>.Release(T obj)
        {
            this.Release(obj);
        }

        public bool IsTreatedAsInactive(T obj)
        {
            return _inactive.Contains(obj);
        }

        #endregion
    }

	static class ArrayUtils
	{
		public static T Pop<T>(this HashSet<T> set)
		{
			if (set == null) throw new System.ArgumentNullException("set");

			var e = set.GetEnumerator();
			if(e.MoveNext())
			{
				set.Remove(e.Current);
				return e.Current;
			}

			throw new System.ArgumentException("HashSet must not be empty.");
		}
	}
}
