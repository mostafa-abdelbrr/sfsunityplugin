using System.Collections;

namespace SmartFoxClientAPI.Data
{
	public class NewRoomDescriptor
	{
		public class ExtensionDescriptor
		{
			private string name;

			private string script;

			public string Name => name;

			public string Script => script;

			public ExtensionDescriptor(string name, string script)
			{
				this.name = name;
				this.script = script;
			}
		}

		private string name;

		private string password;

		private int maxUsers;

		private int maxSpectators;

		private bool isGame;

		private bool exitCurrentRoom;

		private bool receiveUCount;

		private ArrayList variables;

		private ExtensionDescriptor extension;

		public string Name => name;

		public string Password => password;

		public int MaxUsers => maxUsers;

		public int MaxSpectators => maxSpectators;

		public bool IsGame => isGame;

		public bool ExitCurrentRoom => exitCurrentRoom;

		public bool ReceiveUCount => receiveUCount;

		public ArrayList Variables => variables;

		public ExtensionDescriptor Extension => extension;

		public NewRoomDescriptor(string name, int maxUsers)
			: this(name, maxUsers, isGame: false, 0, new ArrayList(), null, "", exitCurrentRoom: true, receiveUCount: false)
		{
		}

		public NewRoomDescriptor(string name, int maxUsers, bool isGame)
			: this(name, maxUsers, isGame, 0, new ArrayList(), null, "", exitCurrentRoom: true, receiveUCount: false)
		{
		}

		public NewRoomDescriptor(string name, int maxUsers, bool isGame, int maxSpectators, ArrayList variables, ExtensionDescriptor extension)
			: this(name, maxUsers, isGame, maxSpectators, variables, extension, "", exitCurrentRoom: true, receiveUCount: false)
		{
		}

		public NewRoomDescriptor(string name, int maxUsers, bool isGame, int maxSpectators, ArrayList variables, ExtensionDescriptor extension, string password, bool exitCurrentRoom, bool receiveUCount)
		{
			this.name = name;
			this.maxUsers = maxUsers;
			this.maxSpectators = maxSpectators;
			this.isGame = isGame;
			this.variables = variables;
			this.extension = extension;
			this.password = password;
			this.exitCurrentRoom = exitCurrentRoom;
			this.receiveUCount = receiveUCount;
		}
	}
}
