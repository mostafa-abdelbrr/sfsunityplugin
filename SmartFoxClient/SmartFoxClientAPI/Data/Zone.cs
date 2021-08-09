using SmartFoxClientAPI.Util;

namespace SmartFoxClientAPI.Data
{
	public class Zone
	{
		private SyncArrayList roomList;

		private string name;

		public Zone(string name)
		{
			this.name = name;
			roomList = new SyncArrayList();
		}

		public Room GetRoom(int id)
		{
			return (Room)roomList.ObjectAt(id);
		}

		public Room GetRoomByName(string name)
		{
			Room result = null;
			bool flag = false;
			foreach (Room room in roomList)
			{
				if (room.GetName() == name)
				{
					result = room;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return result;
			}
			return null;
		}
	}
}
