namespace SmartFoxClientAPI.Data
{
	public class RoomVariable
	{
		private string name;

		private object value;

		private bool isPrivate;

		private bool isPersistent;

		public RoomVariable(string name, object value)
			: this(name, value, isPrivate: false, isPersistent: false)
		{
		}

		public RoomVariable(string name, object value, bool isPrivate)
			: this(name, value, isPrivate, isPersistent: false)
		{
		}

		public RoomVariable(string name, object value, bool isPrivate, bool isPersistent)
		{
			this.name = name;
			this.value = value;
			this.isPrivate = isPrivate;
			this.isPersistent = isPersistent;
		}

		public string GetName()
		{
			return name;
		}

		public object GetValue()
		{
			return value;
		}

		public bool IsPrivate()
		{
			return isPrivate;
		}

		public bool IsPersistent()
		{
			return isPersistent;
		}
	}
}
