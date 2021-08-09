using System.Collections;

namespace SmartFoxClientAPI.Data
{
	public class Buddy
	{
		private int id;

		private string name;

		private bool isOnline;

		private bool isBlocked;

		private Hashtable variables;

		public Buddy(int id, string name, bool isOnline, bool isBlocked)
			: this(id, name, isOnline, isBlocked, new Hashtable())
		{
		}

		public Buddy(int id, string name, bool isOnline, bool isBlocked, Hashtable variables)
		{
			this.id = id;
			this.name = name;
			this.isOnline = isOnline;
			this.isBlocked = isBlocked;
			this.variables = variables;
		}

		public int GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public bool IsOnline()
		{
			return isOnline;
		}

		public bool IsBlocked()
		{
			return isBlocked;
		}

		public Hashtable GetVariables()
		{
			return variables;
		}

		public object GetVariable(string key)
		{
			return variables[key];
		}

		public void SetVariable(string key, object value)
		{
			variables[key] = value;
		}

		public void SetVariables(Hashtable variables)
		{
			this.variables = variables;
		}

		public void SetBlocked(bool status)
		{
			isBlocked = status;
		}
	}
}
