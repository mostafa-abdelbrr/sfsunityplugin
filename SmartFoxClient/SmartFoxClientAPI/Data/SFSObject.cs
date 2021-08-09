using System.Collections;

namespace SmartFoxClientAPI.Data
{
	public class SFSObject
	{
		private Hashtable map;

		public SFSObject()
		{
			map = new Hashtable();
		}

		public void Put(object key, object value)
		{
			map.Add(key.ToString(), value);
		}

		public void PutNumber(object key, double value)
		{
			map.Add(key.ToString(), value);
		}

		public void PutBool(object key, bool value)
		{
			map.Add(key.ToString(), value);
		}

		public void PutList(object key, IList collection)
		{
			PopulateList(new SFSObject(), key.ToString(), collection);
		}

		public void PutDictionary(object key, IDictionary collection)
		{
			PopulateDictionary(new SFSObject(), key.ToString(), collection);
		}

		private void PopulateList(SFSObject aobj, string key, IList collection)
		{
			int num = 0;
			if (aobj != this)
			{
				Put(key, aobj);
			}
			foreach (object item in collection)
			{
				if (item is IList)
				{
					aobj.PutList(num, (IList)item);
				}
				else if (item is IDictionary)
				{
					aobj.PutDictionary(num, (IDictionary)item);
				}
				else
				{
					aobj.Put(num, item);
				}
				num++;
			}
		}

		private void PopulateDictionary(SFSObject aobj, string key, IDictionary collection)
		{
			object obj = null;
			object obj2 = null;
			if (aobj != this)
			{
				Put(key, aobj);
			}
			foreach (DictionaryEntry item in collection)
			{
				obj = item.Key;
				obj2 = item.Value;
				if (obj2 is IList)
				{
					aobj.PutList(obj, (IList)obj2);
				}
				else if (obj2 is IDictionary)
				{
					aobj.PutDictionary(obj, (IDictionary)obj2);
				}
				else
				{
					aobj.Put(obj, obj2);
				}
			}
		}

		public object Get(object key)
		{
			return map[key];
		}

		public string GetString(object key)
		{
			return (string)map[key];
		}

		public double GetNumber(object key)
		{
			return (double)map[key];
		}

		public bool GetBool(object key)
		{
			return (bool)map[key];
		}

		public SFSObject GetObj(object key)
		{
			return (SFSObject)map[key];
		}

		public SFSObject GetObj(int key)
		{
			return (SFSObject)map[key];
		}

		public int Size()
		{
			return map.Count;
		}

		public ICollection Keys()
		{
			return map.Keys;
		}

		public void Remove(object key)
		{
			map.Remove(key);
		}
	}
}
