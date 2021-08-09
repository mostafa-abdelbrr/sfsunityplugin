using System;
using System.Collections;
using System.Xml;
using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Util;

namespace SmartFoxClientAPI.Handlers
{
	public class SysHandler : IMessageHandler
	{
		private SmartFoxClient sfs;

		public SysHandler(SmartFoxClient sfs)
		{
			this.sfs = sfs;
		}

		public void HandleMessage(object msgObj, string type)
		{
			XmlDocument node = (XmlDocument)msgObj;
			XmlNode singleNode = XmlUtil.GetSingleNode(node, "/msg/.");
			string @string = XmlUtil.GetString(singleNode, "body/@action");
			switch (@string)
			{
			case "apiOK":
				HandleApiOK(singleNode);
				break;
			case "apiKO":
				HandleApiKO(singleNode);
				break;
			case "logOK":
				HandleLoginOk(singleNode);
				break;
			case "logKO":
				HandleLoginKo(singleNode);
				break;
			case "logout":
				HandleLogout(singleNode);
				break;
			case "rmList":
				HandleRoomList(singleNode);
				break;
			case "uCount":
				HandleUserCountChange(singleNode);
				break;
			case "joinOK":
				HandleJoinOk(singleNode);
				break;
			case "joinKO":
				HandleJoinKo(singleNode);
				break;
			case "uER":
				HandleUserEnterRoom(singleNode);
				break;
			case "userGone":
				HandleUserLeaveRoom(singleNode);
				break;
			case "pubMsg":
				HandlePublicMessage(singleNode);
				break;
			case "prvMsg":
				HandlePrivateMessage(singleNode);
				break;
			case "dmnMsg":
				HandleAdminMessage(singleNode);
				break;
			case "modMsg":
				HandleModMessage(singleNode);
				break;
			case "dataObj":
				HandleSFSObject(singleNode);
				break;
			case "rVarsUpdate":
				HandleRoomVarsUpdate(singleNode);
				break;
			case "roomAdd":
				HandleRoomAdded(singleNode);
				break;
			case "roomDel":
				HandleRoomDeleted(singleNode);
				break;
			case "rndK":
				HandleRandomKey(singleNode);
				break;
			case "roundTripRes":
				HandleRoundTripBench(singleNode);
				break;
			case "uVarsUpdate":
				HandleUserVarsUpdate(singleNode);
				break;
			case "createRmKO":
				HandleCreateRoomError(singleNode);
				break;
			case "bList":
				HandleBuddyList(singleNode);
				break;
			case "bUpd":
				HandleBuddyListUpdate(singleNode);
				break;
			case "bAdd":
				HandleBuddyAdded(singleNode);
				break;
			case "roomB":
				HandleBuddyRoom(singleNode);
				break;
			case "leaveRoom":
				HandleLeaveRoom(singleNode);
				break;
			case "swSpec":
				HandleSpectatorSwitched(singleNode);
				break;
			case "bPrm":
				HandleAddBuddyPermission(singleNode);
				break;
			case "remB":
				HandleRemoveBuddy(singleNode);
				break;
			case "swPl":
				HandlePlayerSwitched(singleNode);
				break;
			default:
				sfs.DebugMessage("Unknown sys command: " + @string);
				break;
			}
		}

		public void HandleApiOK(XmlNode xml)
		{
			sfs.SetIsConnected(b: true);
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("success", true);
			SFSEvent evt = new SFSEvent("OnConnection", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleApiKO(XmlNode xml)
		{
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("success", false);
			hashtable.Add("error", "API are obsolete, please upgrade");
			SFSEvent evt = new SFSEvent("OnConnection", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleLoginOk(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/login/@id");
			int int2 = XmlUtil.GetInt(xml, "body/login/@mod");
			string @string = XmlUtil.GetString(xml, "body/login/@n");
			sfs.amIModerator = int2 == 1;
			sfs.myUserId = @int;
			sfs.myUserName = @string;
			sfs.playerId = -1;
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("success", true);
			hashtable.Add("name", @string);
			hashtable.Add("error", "");
			sfs.GetRoomList();
			SFSEvent evt = new SFSEvent("OnLogin", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleLoginKo(XmlNode xml)
		{
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("success", false);
			hashtable.Add("error", XmlUtil.GetString(xml, "body/login/@e"));
			SFSEvent evt = new SFSEvent("OnLogin", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleLogout(XmlNode xml)
		{
			sfs.__Logout();
			SFSEvent evt = new SFSEvent("OnLogout", null);
			sfs.DispatchEvent(evt);
		}

		public void HandleRoomList(XmlNode xml)
		{
			sfs.ClearRoomList();
			Hashtable allRooms = sfs.GetAllRooms();
			foreach (XmlNode node in XmlUtil.GetNodeList(xml, "body/rmList/rm"))
			{
				int @int = XmlUtil.GetInt(node, "./@id");
				Room room = new Room(@int, XmlUtil.GetString(node, "./n/node()"), XmlUtil.GetInt(node, "./@maxu"), XmlUtil.GetInt(node, "./@maxs"), XmlUtil.GetBool(node, "./@temp"), XmlUtil.GetBool(node, "./@game"), XmlUtil.GetBool(node, "./@priv"), XmlUtil.GetBool(node, "./@lmb"), XmlUtil.GetInt(node, "./@ucnt"), XmlUtil.GetInt(node, "./@scnt"));
				if (XmlUtil.GetSingleNode(node, "./vars") != null)
				{
					PopulateVariables(room.GetVariables(), node);
				}
				allRooms[@int] = room;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("roomList", allRooms);
			SFSEvent evt = new SFSEvent("OnRoomListUpdate", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleUserCountChange(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@u");
			int int2 = XmlUtil.GetInt(xml, "body/@s");
			int int3 = XmlUtil.GetInt(xml, "body/@r");
			Room room = (Room)sfs.GetAllRooms()[int3];
			if (room != null)
			{
				room.SetUserCount(@int);
				room.SetSpectatorCount(int2);
				Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
				hashtable.Add("room", room);
				SFSEvent evt = new SFSEvent("OnUserCountChange", hashtable);
				sfs.DispatchEvent(evt);
			}
		}

		public void HandleJoinOk(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			XmlNode singleNode = XmlUtil.GetSingleNode(xml, "body/uLs");
			int int2 = XmlUtil.GetInt(xml, "body/pid/@id");
			sfs.activeRoomId = @int;
			Room room = sfs.GetRoom(@int);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! JoinOk tries to join an unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			room.ClearUserList();
			sfs.playerId = int2;
			room.SetMyPlayerIndex(int2);
			if (XmlUtil.GetSingleNode(xml, "body/vars") != null)
			{
				room.ClearVariables();
				PopulateVariables(room.GetVariables(), XmlUtil.GetSingleNode(xml, "body"));
			}
			foreach (XmlNode node in XmlUtil.GetNodeList(singleNode, "./u"))
			{
				XmlUtil.Dump(node, 0);
				string @string = XmlUtil.GetString(node, "./n/node()");
				int int3 = XmlUtil.GetInt(node, "./@i");
				bool @bool = XmlUtil.GetBool(node, "./@m");
				bool bool2 = XmlUtil.GetBool(node, "./@s");
				int playerId = ((XmlUtil.GetSingleNode(node, "./@p") == null) ? (-1) : XmlUtil.GetInt(node, "./@p"));
				User user = new User(int3, @string);
				user.SetModerator(@bool);
				user.SetIsSpectator(bool2);
				user.SetPlayerId(playerId);
				if (XmlUtil.GetSingleNode(node, "./vars") != null)
				{
					PopulateVariables(user.GetVariables(), node);
				}
				room.AddUser(user, int3);
			}
			sfs.changingRoom = false;
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("room", room);
			SFSEvent evt = new SFSEvent("OnJoinRoom", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleJoinKo(XmlNode xml)
		{
			sfs.changingRoom = false;
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("error", XmlUtil.GetString(xml, "body/error/@msg"));
			SFSEvent evt = new SFSEvent("OnJoinRoomError", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleUserEnterRoom(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/u/@i");
			string @string = XmlUtil.GetString(xml, "body/u/n/node()");
			bool @bool = XmlUtil.GetBool(xml, "body/u/@m");
			bool bool2 = XmlUtil.GetBool(xml, "body/u/@s");
			int int3 = XmlUtil.GetInt(xml, "body/u/@p");
			XmlNode singleNode = XmlUtil.GetSingleNode(xml, "body/u/vars");
			Room room = sfs.GetRoom(@int);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! UserEnterRoom tries to enter an unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			User user = new User(int2, @string);
			user.SetModerator(@bool);
			user.SetIsSpectator(bool2);
			user.SetPlayerId(int3);
			room.AddUser(user, int2);
			if (singleNode != null)
			{
				PopulateVariables(user.GetVariables(), XmlUtil.GetSingleNode(xml, "body/u"));
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("roomId", @int);
			hashtable.Add("user", user);
			SFSEvent evt = new SFSEvent("OnUserEnterRoom", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleUserLeaveRoom(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/user/@id");
			int int2 = XmlUtil.GetInt(xml, "body/@r");
			Room room = sfs.GetRoom(int2);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! UserLeaveRoom tries to leave an unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			string name = room.GetUser(@int).GetName();
			room.RemoveUser(@int);
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("roomId", int2);
			hashtable.Add("userId", @int);
			hashtable.Add("userName", name);
			SFSEvent evt = new SFSEvent("OnUserLeaveRoom", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandlePublicMessage(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			string @string = XmlUtil.GetString(xml, "body/txt/node()");
			if (sfs.GetRoom(@int) == null)
			{
				sfs.DebugMessage("WARNING! PublicMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			User user = sfs.GetRoom(@int).GetUser(int2);
			if (user == null)
			{
				sfs.DebugMessage("WARNING! PublicMessage received from unknown sender. Command ignored!!");
				return;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("message", Entities.DecodeEntities(@string));
			hashtable.Add("sender", user);
			hashtable.Add("roomId", @int);
			SFSEvent evt = new SFSEvent("OnPublicMessage", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandlePlayerSwitched(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/pid/@id");
			bool flag = true;
			if (XmlUtil.GetSingleNode(xml, "body/pid/@u") != null)
			{
				flag = false;
			}
			Room room = sfs.GetRoom(@int);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! PlayerSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			if (int2 == -1)
			{
				room.SetUserCount(room.GetUserCount() - 1);
				room.SetSpectatorCount(room.GetSpectatorCount() + 1);
				if (!flag)
				{
					int int3 = XmlUtil.GetInt(xml, "body/pid/@u");
					User user = room.GetUser(int3);
					if (user != null)
					{
						user.SetIsSpectator(b: true);
						user.SetPlayerId(int2);
					}
				}
			}
			if (flag)
			{
				sfs.playerId = int2;
				Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
				hashtable.Add("success", sfs.playerId == -1);
				hashtable.Add("newId", sfs.playerId);
				hashtable.Add("room", room);
				SFSEvent evt = new SFSEvent("OnPlayerSwitched", hashtable);
				sfs.DispatchEvent(evt);
			}
		}

		public void HandlePrivateMessage(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			string @string = XmlUtil.GetString(xml, "body/txt/node()");
			if (sfs.GetRoom(@int) == null)
			{
				sfs.DebugMessage("WARNING! PrivateMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			User user = sfs.GetRoom(@int).GetUser(int2);
			if (user == null)
			{
				sfs.DebugMessage("WARNING! PrivateMessage received from unknown sender. Command ignored!!");
				return;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("message", Entities.DecodeEntities(@string));
			hashtable.Add("sender", user);
			hashtable.Add("roomId", @int);
			hashtable.Add("userId", int2);
			SFSEvent evt = new SFSEvent("OnPrivateMessage", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleAdminMessage(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			string @string = XmlUtil.GetString(xml, "body/txt/node()");
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("message", Entities.DecodeEntities(@string));
			SFSEvent evt = new SFSEvent("OnAdminMessage", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleModMessage(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			string @string = XmlUtil.GetString(xml, "body/txt/node()");
			User user = null;
			Room room = sfs.GetRoom(@int);
			if (sfs.GetRoom(@int) == null)
			{
				sfs.DebugMessage("WARNING! ModMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			user = sfs.GetRoom(@int).GetUser(int2);
			if (user == null)
			{
				sfs.DebugMessage("WARNING! ModMessage received from unknown sender. Command ignored!!");
				return;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("message", Entities.DecodeEntities(@string));
			hashtable.Add("sender", user);
			SFSEvent evt = new SFSEvent("OnModMessage", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleSFSObject(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			string @string = XmlUtil.GetString(xml, "body/dataObj/node()");
			if (sfs.GetRoom(@int) == null)
			{
				sfs.DebugMessage("WARNING! SFSObject received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			User user = sfs.GetRoom(@int).GetUser(int2);
			if (user == null)
			{
				sfs.DebugMessage("WARNING! SFSObject received from unknown sender. Command ignored!!");
				return;
			}
			SFSObject value = SFSObjectSerializer.GetInstance().Deserialize(@string);
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("obj", value);
			hashtable.Add("sender", user);
			SFSEvent evt = new SFSEvent("OnObjectReceived", hashtable);
			sfs.DispatchEvent(evt);
		}

		public void HandleRoomVarsUpdate(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			Room room = sfs.GetRoom(@int);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! RoomVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			if (XmlUtil.GetSingleNode(xml, "body/vars") != null)
			{
				PopulateVariables(room.GetVariables(), XmlUtil.GetSingleNode(xml, "body"), hashtable);
			}
			Hashtable hashtable2 = Hashtable.Synchronized(new Hashtable());
			hashtable2.Add("room", room);
			hashtable2.Add("changedVars", hashtable);
			SFSEvent evt = new SFSEvent("OnRoomVariablesUpdate", hashtable2);
			sfs.DispatchEvent(evt);
		}

		public void HandleUserVarsUpdate(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/user/@id");
			if (sfs.GetRoom(@int) == null)
			{
				sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			User user = sfs.GetRoom(@int).GetUser(int2);
			if (user == null)
			{
				sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown user. Command ignored!!");
				return;
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			if (XmlUtil.GetSingleNode(xml, "body/vars") != null)
			{
				PopulateVariables(user.GetVariables(), XmlUtil.GetSingleNode(xml, "body"), hashtable);
			}
			Hashtable hashtable2 = Hashtable.Synchronized(new Hashtable());
			hashtable2.Add("user", user);
			hashtable2.Add("changedVars", hashtable);
			SFSEvent evt = new SFSEvent("OnUserVariablesUpdate", hashtable2);
			sfs.DispatchEvent(evt);
		}

		private void HandleRoomAdded(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/rm/@id");
			string @string = XmlUtil.GetString(xml, "body/rm/name/node()");
			int int2 = XmlUtil.GetInt(xml, "body/rm/@max");
			int int3 = XmlUtil.GetInt(xml, "body/rm/@spec");
			bool @bool = XmlUtil.GetBool(xml, "body/rm/@temp");
			bool bool2 = XmlUtil.GetBool(xml, "body/rm/@game");
			bool bool3 = XmlUtil.GetBool(xml, "body/rm/@priv");
			bool bool4 = XmlUtil.GetBool(xml, "body/rm/@limbo");
			Room room = new Room(@int, @string, int2, int3, @bool, bool2, bool3, bool4);
			Hashtable allRooms = sfs.GetAllRooms();
			allRooms[@int] = room;
			if (XmlUtil.GetSingleNode(xml, "body/rm/vars") != null)
			{
				PopulateVariables(room.GetVariables(), XmlUtil.GetSingleNode(xml, "body/rm"));
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("room", room);
			SFSEvent evt = new SFSEvent("OnRoomAdded", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleRoomDeleted(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/rm/@id");
			Hashtable allRooms = sfs.GetAllRooms();
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("room", allRooms[@int]);
			allRooms.Remove(@int);
			SFSEvent evt = new SFSEvent("OnRoomDeleted", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleRandomKey(XmlNode xml)
		{
			string @string = XmlUtil.GetString(xml, "body/k/node()");
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("key", @string);
			SFSEvent evt = new SFSEvent("OnRandomKey", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleRoundTripBench(XmlNode xml)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now - sfs.GetBenchStartTime();
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("elapsed", Convert.ToInt32(timeSpan.TotalSeconds));
			SFSEvent evt = new SFSEvent("OnRoundTripResponse", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleCreateRoomError(XmlNode xml)
		{
			string @string = XmlUtil.GetString(xml, "body/room/@e");
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("error", @string);
			SFSEvent evt = new SFSEvent("OnCreateRoomError", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleBuddyList(XmlNode xml)
		{
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			sfs.buddyList.Clear();
			sfs.myBuddyVars.Clear();
			if (XmlUtil.GetSingleNode(xml, "body/mv") != null)
			{
				foreach (XmlNode node4 in XmlUtil.GetNodeList(xml, "body/mv/v"))
				{
					sfs.myBuddyVars[XmlUtil.GetString(node4, "./@n")] = XmlUtil.GetString(node4, "./node()");
				}
			}
			if (XmlUtil.GetSingleNode(xml, "body/bList/b") != null)
			{
				foreach (XmlNode node5 in XmlUtil.GetNodeList(xml, "body/bList/b"))
				{
					Buddy buddy = new Buddy(XmlUtil.GetInt(node5, "./@i"), XmlUtil.GetString(node5, "./n/node()"), XmlUtil.GetBool(node5, "./@s"), XmlUtil.GetBool(node5, "./@x"), Hashtable.Synchronized(new Hashtable()));
					if (XmlUtil.GetSingleNode(node5, "./vs") != null)
					{
						foreach (XmlNode node6 in XmlUtil.GetNodeList(node5, "./vs/v"))
						{
							buddy.SetVariable(XmlUtil.GetString(node6, "./@n"), XmlUtil.GetString(node6, "./node()"));
						}
					}
					sfs.buddyList.Add(buddy);
				}
				hashtable.Add("list", sfs.buddyList);
				SFSEvent evt = new SFSEvent("OnBuddyList", hashtable);
				sfs.DispatchEvent(evt);
			}
			else
			{
				hashtable.Add("error", XmlUtil.GetString(xml, "body/err"));
				SFSEvent evt = new SFSEvent("OnBuddyListError", hashtable);
				sfs.DispatchEvent(evt);
			}
		}

		private void HandleBuddyListUpdate(XmlNode xml)
		{
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			SFSEvent sFSEvent = null;
			if (XmlUtil.GetSingleNode(xml, "body/b") != null)
			{
				Buddy buddy = new Buddy(XmlUtil.GetInt(xml, "body/b/@i"), XmlUtil.GetString(xml, "body/b/n/node()"), XmlUtil.GetBool(xml, "body/b/@s"), XmlUtil.GetBool(xml, "body/b/@x"));
				XmlNode singleNode = XmlUtil.GetSingleNode(xml, "body/b/vs");
				bool flag = false;
				foreach (Buddy buddy2 in sfs.buddyList)
				{
					if (!(buddy2.GetName() == buddy.GetName()))
					{
						continue;
					}
					buddy.SetBlocked(buddy2.IsBlocked());
					buddy.SetVariables(buddy2.GetVariables());
					if (singleNode != null)
					{
						foreach (XmlNode node in XmlUtil.GetNodeList(singleNode, "body/b/vs/v"))
						{
							buddy.SetVariable(XmlUtil.GetString(node, "./@n"), XmlUtil.GetString(node, "./node()"));
						}
					}
					sfs.buddyList.Remove(buddy2);
					sfs.buddyList.Add(buddy);
					flag = true;
					break;
				}
				if (flag)
				{
					hashtable.Add("buddy", buddy);
					sFSEvent = new SFSEvent("OnBuddyListUpdate", hashtable);
					sfs.DispatchEvent(sFSEvent);
				}
			}
			else
			{
				hashtable.Add("error", XmlUtil.GetString(xml, "body/err"));
				sFSEvent = new SFSEvent("OnBuddyListError", hashtable);
				sfs.DispatchEvent(sFSEvent);
			}
		}

		private void HandleAddBuddyPermission(XmlNode xml)
		{
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("sender", XmlUtil.GetString(xml, "body/n/node()"));
			hashtable.Add("message", "");
			if (XmlUtil.GetSingleNode(xml, "body/txt") != null)
			{
				hashtable.Add("message", Entities.DecodeEntities(XmlUtil.GetString(xml, "body/txt")));
			}
			SFSEvent evt = new SFSEvent("OnBuddyPermissionRequest", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleBuddyAdded(XmlNode xml)
		{
			Buddy buddy = new Buddy(XmlUtil.GetInt(xml, "body/b/@i"), XmlUtil.GetString(xml, "body/b/n/node()"), XmlUtil.GetBool(xml, "body/b/@s"), XmlUtil.GetBool(xml, "body/b/@x"));
			if (XmlUtil.GetSingleNode(xml, "body/b/vs") != null)
			{
				foreach (XmlNode node in XmlUtil.GetNodeList(xml, "body/b/vs/v"))
				{
					buddy.SetVariable(XmlUtil.GetString(node, "./@n"), XmlUtil.GetString(node, "./node()"));
				}
			}
			sfs.buddyList.Add(buddy);
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("list", sfs.buddyList);
			SFSEvent evt = new SFSEvent("OnBuddyList", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleRemoveBuddy(XmlNode xml)
		{
			string @string = XmlUtil.GetString(xml, "body/n/node()");
			foreach (Buddy buddy in sfs.buddyList)
			{
				if (buddy.GetName() == @string)
				{
					sfs.buddyList.Remove(buddy);
					Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
					hashtable.Add("list", sfs.buddyList);
					SFSEvent evt = new SFSEvent("OnBuddyList", hashtable);
					sfs.DispatchEvent(evt);
					break;
				}
			}
		}

		private void HandleBuddyRoom(XmlNode xml)
		{
			string @string = XmlUtil.GetString(xml, "body/br/@r");
			SyncArrayList syncArrayList = new SyncArrayList();
			string[] array = @string.Split(",".ToCharArray());
			foreach (string s in array)
			{
				syncArrayList.Add(int.Parse(s));
			}
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("idList", syncArrayList);
			SFSEvent evt = new SFSEvent("OnBuddyRoom", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleLeaveRoom(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/rm/@id");
			Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
			hashtable.Add("roomId", @int);
			SFSEvent evt = new SFSEvent("OnRoomLeft", hashtable);
			sfs.DispatchEvent(evt);
		}

		private void HandleSpectatorSwitched(XmlNode xml)
		{
			int @int = XmlUtil.GetInt(xml, "body/@r");
			int int2 = XmlUtil.GetInt(xml, "body/pid/@id");
			Room room = sfs.GetRoom(@int);
			if (room == null)
			{
				sfs.DebugMessage("WARNING! SpectatorSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
				return;
			}
			if (int2 > 0)
			{
				room.SetUserCount(room.GetUserCount() + 1);
				room.SetSpectatorCount(room.GetSpectatorCount() - 1);
			}
			if (XmlUtil.GetSingleNode(xml, "body/pid/@u") != null)
			{
				int int3 = XmlUtil.GetInt(xml, "body/pid/@u");
				User user = room.GetUser(int3);
				if (user != null)
				{
					user.SetIsSpectator(b: false);
					user.SetPlayerId(int2);
				}
			}
			else
			{
				sfs.playerId = int2;
				Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
				hashtable.Add("success", sfs.playerId > 0);
				hashtable.Add("newId", sfs.playerId);
				hashtable.Add("room", room);
				SFSEvent evt = new SFSEvent("OnSpectatorSwitched", hashtable);
				sfs.DispatchEvent(evt);
			}
		}

		private void PopulateVariables(Hashtable variables, XmlNode xmlData)
		{
			PopulateVariables(variables, xmlData, null);
		}

		private void PopulateVariables(Hashtable variables, XmlNode xmlData, Hashtable changedVars)
		{
			foreach (XmlNode node in XmlUtil.GetNodeList(xmlData, "vars/var"))
			{
				string @string = XmlUtil.GetString(node, "./@n");
				string string2 = XmlUtil.GetString(node, "./@t");
				string string3 = XmlUtil.GetString(node, "./node()");
				changedVars?.Add(@string, true);
				switch (string2)
				{
				case "b":
					variables[@string] = ((string3 == "1") ? true : false);
					break;
				case "n":
					variables[@string] = double.Parse(string3);
					break;
				case "s":
					variables[@string] = string3;
					break;
				case "x":
					variables.Remove(@string);
					break;
				}
			}
		}

		public void DispatchDisconnection()
		{
			SFSEvent evt = new SFSEvent("OnConnectionLost", null);
			sfs.DispatchEvent(evt);
		}
	}
}
