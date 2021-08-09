using System.Collections;

namespace SmartFoxClientAPI.Data
{
	public class User
	{
		private int id;

		private string name;

		private Hashtable variables;

		private bool isSpec;

		private bool isMod;

		private int pId;

		public User(int id, string name)
		{
			this.id = id;
			this.name = name;
			variables = new Hashtable();
			isSpec = false;
			isMod = false;
		}

		public int GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public object GetVariable(string varName)
		{
			return variables[varName];
		}

		public Hashtable GetVariables()
		{
			return variables;
		}

		public void SetVariables(Hashtable o)
		{
			foreach (string key in o.Keys)
			{
				object obj = o[key];
				if (obj != null)
				{
					variables[key] = obj;
				}
				else
				{
					variables.Remove(key);
				}
			}
		}

		internal void ClearVariables()
		{
			variables.Clear();
		}

		public void SetIsSpectator(bool b)
		{
			isSpec = b;
		}

		public bool IsSpectator()
		{
			return isSpec;
		}

		public void SetModerator(bool b)
		{
			isMod = b;
		}

		public bool IsModerator()
		{
			return isMod;
		}

		public int GetPlayerId()
		{
			return pId;
		}

		public void SetPlayerId(int pid)
		{
			pId = pid;
		}
	}
}
