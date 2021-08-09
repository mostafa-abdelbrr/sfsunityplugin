using System.Collections;

namespace SmartFoxClientAPI.Util
{
	public class SyncArrayList
	{
		private ArrayList me;

		public ArrayList ToArrayList()
		{
			return me;
		}

		public SyncArrayList()
		{
			me = new ArrayList();
		}

		public void Add(object item)
		{
			lock (me.SyncRoot)
			{
				me.Add(item);
			}
		}

		public object ObjectAt(int index)
		{
			lock (me.SyncRoot)
			{
				return me[index];
			}
		}

		public void AddRange(ICollection c)
		{
			lock (me.SyncRoot)
			{
				me.AddRange(c);
			}
		}

		public int Capacity()
		{
			lock (me.SyncRoot)
			{
				return me.Capacity;
			}
		}

		public void Clear()
		{
			lock (me.SyncRoot)
			{
				me.Clear();
			}
		}

		public bool Contains(object item)
		{
			lock (me.SyncRoot)
			{
				return me.Contains(item);
			}
		}

		public int Count()
		{
			lock (me.SyncRoot)
			{
				return me.Count;
			}
		}

		public void Insert(int index, object Value)
		{
			lock (me.SyncRoot)
			{
				me.Insert(index, Value);
			}
		}

		public void Remove(object obj)
		{
			lock (me.SyncRoot)
			{
				me.Remove(obj);
			}
		}

		public void RemoveAt(int index)
		{
			lock (me.SyncRoot)
			{
				me.RemoveAt(index);
			}
		}

		public void RemoveRange(int index, int count)
		{
			lock (me.SyncRoot)
			{
				me.RemoveRange(index, count);
			}
		}

		public IEnumerator GetEnumerator()
		{
			lock (me.SyncRoot)
			{
				return me.GetEnumerator();
			}
		}
	}
}
