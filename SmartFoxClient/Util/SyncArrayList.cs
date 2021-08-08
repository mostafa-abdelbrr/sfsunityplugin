using System;
using System.Collections;

namespace SmartFoxClientAPI.Util
{
	/// <summary>
	/// Synchronized wrapper around the unsynchronized ArrayList class
	/// </summary>
    /// 
    /// <remarks>
    /// From here: http://www.c-sharpcorner.com/UploadFile/alexfila/ThreadSafe11222005234917PM/ThreadSafe.aspx
    /// </remarks>
	public class SyncArrayList
	{
		private ArrayList me;


		/// <summary>
		/// 
		/// </summary>
		public ArrayList ToArrayList() {
			return me;
		}

        /// <summary>
        /// 
        /// </summary>
        public SyncArrayList()
		{
			//
			// TODO: Add constructor logic here
			//
			me = new ArrayList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public void Add(object item)
		{
			lock(me.SyncRoot)
			{
				me.Add(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public object ObjectAt(int index)
		{
			lock (me.SyncRoot)
			{
				return me[index];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		public void AddRange(ICollection c)
		{
			lock(me.SyncRoot)
			{
				me.AddRange(c);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int Capacity() 
		{
			lock (me.SyncRoot)
			{
				return	me.Capacity;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			lock (me.SyncRoot)
			{
				me.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(object item)
		{
			lock (me.SyncRoot)
			{
				return me.Contains(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int Count() 
		{
			lock (me.SyncRoot)
			{
				return me.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="Value"></param>
		public void Insert(int index, object Value)
		{
			lock (me.SyncRoot)
			{
				me.Insert(index, Value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void Remove(object obj)
		{
			lock (me.SyncRoot)
			{
				me.Remove(obj);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			lock (me.SyncRoot)
			{
				me.RemoveAt(index);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public void RemoveRange(int index, int count)
		{
			lock (me.SyncRoot)
			{
				me.RemoveRange(index, count);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            lock (me.SyncRoot)
            {
                return me.GetEnumerator();
            }
        }
	}
}
