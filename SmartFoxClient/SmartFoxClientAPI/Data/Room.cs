using System.Collections;

namespace SmartFoxClientAPI.Data
{
	public class Room
	{
		private int id;

		private string name;

		private int maxUsers;

		private int maxSpectators;

		private bool temp;

		private bool game;

		private bool priv;

		private bool limbo;

		private int userCount;

		private int specCount;

		private int myPlayerIndex;

		private Hashtable userList;

		private Hashtable variables;

		public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo)
			: this(id, name, maxUsers, maxSpectators, isTemp, isGame, isPrivate, isLimbo, 0, 0)
		{
		}

		public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo, int userCount)
			: this(id, name, maxUsers, maxSpectators, isTemp, isGame, isPrivate, isLimbo, userCount, 0)
		{
		}

		public Room(int id, string name, int maxUsers, int maxSpectators, bool isTemp, bool isGame, bool isPrivate, bool isLimbo, int userCount, int specCount)
		{
			this.id = id;
			this.name = name;
			this.maxSpectators = maxSpectators;
			this.maxUsers = maxUsers;
			temp = isTemp;
			game = isGame;
			priv = isPrivate;
			limbo = isLimbo;
			this.userCount = userCount;
			this.specCount = specCount;
			userList = new Hashtable();
			variables = new Hashtable();
		}

		public void AddUser(User u, int id)
		{
			userList[id] = u;
			if (game && u.IsSpectator())
			{
				specCount++;
			}
			else
			{
				userCount++;
			}
		}

		public void RemoveUser(int id)
		{
			User user = (User)userList[id];
			if (game && user.IsSpectator())
			{
				specCount--;
			}
			else
			{
				userCount--;
			}
			userList.Remove(id);
		}

		public Hashtable GetUserList()
		{
			return userList;
		}

		public User GetUser(object userId)
		{
			User result = null;
			if (userId.GetType() == typeof(int))
			{
				result = (User)userList[(int)userId];
			}
			else if (userId.GetType() == typeof(string))
			{
				foreach (User value in userList.Values)
				{
					if (value.GetName() == (string)userId)
					{
						result = value;
						break;
					}
				}
			}
			return result;
		}

		public void ClearUserList()
		{
			userList.Clear();
			userCount = 0;
			specCount = 0;
		}

		public object GetVariable(string varName)
		{
			return ((RoomVariable)variables[varName]).GetValue();
		}

		public Hashtable GetVariables()
		{
			return variables;
		}

		public void SetVariables(Hashtable vars)
		{
			variables = vars;
		}

		public void ClearVariables()
		{
			variables.Clear();
		}

		public string GetName()
		{
			return name;
		}

		public int GetId()
		{
			return id;
		}

		public bool IsTemp()
		{
			return temp;
		}

		public bool IsGame()
		{
			return game;
		}

		public bool IsPrivate()
		{
			return priv;
		}

		public int GetUserCount()
		{
			return userCount;
		}

		public int GetSpectatorCount()
		{
			return specCount;
		}

		public int GetMaxUsers()
		{
			return maxUsers;
		}

		public int GetMaxSpectators()
		{
			return maxSpectators;
		}

		public void SetMyPlayerIndex(int id)
		{
			myPlayerIndex = id;
		}

		public int GetMyPlayerIndex()
		{
			return myPlayerIndex;
		}

		public void SetIsLimbo(bool b)
		{
			limbo = b;
		}

		public bool IsLimbo()
		{
			return limbo;
		}

		public void SetUserCount(int n)
		{
			userCount = n;
		}

		public void SetSpectatorCount(int n)
		{
			specCount = n;
		}
	}
}
