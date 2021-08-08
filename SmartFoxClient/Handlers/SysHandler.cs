using System;
using System.Collections;
using System.Xml;
using SmartFoxClientAPI.Util;
using SmartFoxClientAPI.Data;

namespace SmartFoxClientAPI.Handlers {
	/**
	 * <summary>SysHandler class: handles "sys" type messages.</summary>
	 * 
	 * <remarks>
	 * <para><b>Version:</b><br/>
	 * 1.1.0</para>
	 * 
	 * <para><b>Author:</b><br/>
	 * Thomas Hentschel Lund<br/>
	 * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
	 * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
	 * (c) 2008,2009 gotoAndPlay()<br/>
	 *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
	 * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
	 * </para>
	 * </remarks>
	 */
	public class SysHandler : IMessageHandler {
		private SmartFoxClient sfs;

		/**
		 * <summary>
		 * SysHandler constructor.
		 * </summary>
		 * 
		 * <param name="sfs">the smart fox client</param>
		 */
		public SysHandler(SmartFoxClient sfs) {
			this.sfs = sfs;
		}

		/**
		 * <summary>
		 * Handle messages
		 * </summary>
		 * 
		 * <param name="msgObj">the message object to handle</param>
		 * <param name="type">type of message</param>
		 */
		public void HandleMessage(object msgObj, string type) {
			XmlDocument xmlDoc = (XmlDocument)msgObj;

			XmlNode xml = XmlUtil.GetSingleNode(xmlDoc, "/msg/.");
			string action = XmlUtil.GetString(xml, "body/@action");

			// Send message to handler method
			switch ( action ) {
				case "apiOK":
					this.HandleApiOK(xml);
					break;
				case "apiKO":
					this.HandleApiKO(xml);
					break;
				case "logOK":
					this.HandleLoginOk(xml);
					break;
				case "logKO":
					this.HandleLoginKo(xml);
					break;
				case "logout":
					this.HandleLogout(xml);
					break;
				case "rmList":
					this.HandleRoomList(xml);
					break;
				case "uCount":
					this.HandleUserCountChange(xml);
					break;
				case "joinOK":
					this.HandleJoinOk(xml);
					break;
				case "joinKO":
					this.HandleJoinKo(xml);
					break;
				case "uER":
					this.HandleUserEnterRoom(xml);
					break;
				case "userGone":
					this.HandleUserLeaveRoom(xml);
					break;
				case "pubMsg":
					this.HandlePublicMessage(xml);
					break;
				case "prvMsg":
					this.HandlePrivateMessage(xml);
					break;
				case "dmnMsg":
					this.HandleAdminMessage(xml);
					break;
				case "modMsg":
					this.HandleModMessage(xml);
					break;
				case "dataObj":
					this.HandleSFSObject(xml);
					break;
				case "rVarsUpdate":
					this.HandleRoomVarsUpdate(xml);
					break;
				case "roomAdd":
					this.HandleRoomAdded(xml);
					break;
				case "roomDel":
					this.HandleRoomDeleted(xml);
					break;
				case "rndK":
					this.HandleRandomKey(xml);
					break;
				case "roundTripRes":
					this.HandleRoundTripBench(xml);
					break;
				case "uVarsUpdate":
					this.HandleUserVarsUpdate(xml);
					break;
				case "createRmKO":
					this.HandleCreateRoomError(xml);
					break;
				case "bList":
					this.HandleBuddyList(xml);
					break;
				case "bUpd":
					this.HandleBuddyListUpdate(xml);
					break;
				case "bAdd":
					this.HandleBuddyAdded(xml);
					break;
				case "roomB":
					this.HandleBuddyRoom(xml);
					break;
				case "leaveRoom":
					this.HandleLeaveRoom(xml);
					break;
				case "swSpec":
					this.HandleSpectatorSwitched(xml);
					break;
				case "bPrm":
					this.HandleAddBuddyPermission(xml);
					break;
				case "remB":
					this.HandleRemoveBuddy(xml);
					break;
				case "swPl":
					this.HandlePlayerSwitched(xml);
					break;
				default:
					sfs.DebugMessage("Unknown sys command: " + action); // TODO - remove?
					break;
			}
		}

		/**
		 * <summary>
		 * Handle correct API
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleApiOK(XmlNode xml) {
			sfs.SetIsConnected(true);
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("success", true);
			SFSEvent evt = new SFSEvent(SFSEvent.onConnectionEvent, parameters);
			sfs.DispatchEvent(evt);
		}


		/**
		 * <summary>
		 * Handle obsolete API
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleApiKO(XmlNode xml) {
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("success", false);
			parameters.Add("error", "API are obsolete, please upgrade");

			SFSEvent evt = new SFSEvent(SFSEvent.onConnectionEvent, parameters);
			sfs.DispatchEvent(evt);
		}


		/**
		 * <summary>
		 * Handle successful login
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleLoginOk(XmlNode xml) {
			int uid = XmlUtil.GetInt(xml, "body/login/@id");
			int mod = XmlUtil.GetInt(xml, "body/login/@mod");
			string name = XmlUtil.GetString(xml, "body/login/@n");

			sfs.amIModerator = ( mod == 1 );
			sfs.myUserId = uid;
			sfs.myUserName = name;
			sfs.playerId = -1;

			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("success", true);
			parameters.Add("name", name);
			parameters.Add("error", "");

			// Request room list
			sfs.GetRoomList();

			SFSEvent evt = new SFSEvent(SFSEvent.onLoginEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle unsuccessful login
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleLoginKo(XmlNode xml) {

			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("success", false);
			parameters.Add("error", XmlUtil.GetString(xml, "body/login/@e"));

			SFSEvent evt = new SFSEvent(SFSEvent.onLoginEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle successful logout
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleLogout(XmlNode xml) {
			sfs.__Logout();

			SFSEvent evt = new SFSEvent(SFSEvent.onLogoutEvent, null);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Populate the room list for this zone and fire the event
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleRoomList(XmlNode xml) {

			Hashtable roomList = sfs.GetAllRooms();
			Hashtable tempList = new Hashtable();

			foreach ( XmlNode roomXml in XmlUtil.GetNodeList(xml, "body/rmList/rm") ) {
				int roomId = XmlUtil.GetInt(roomXml, "./@id");
				Room room = new Room(roomId,
										XmlUtil.GetString(roomXml, "./n/node()"),
										XmlUtil.GetInt(roomXml, "./@maxu"),
										XmlUtil.GetInt(roomXml, "./@maxs"),
										XmlUtil.GetBool(roomXml, "./@temp"),
										XmlUtil.GetBool(roomXml, "./@game"),
										XmlUtil.GetBool(roomXml, "./@priv"),
										XmlUtil.GetBool(roomXml, "./@lmb"),
										XmlUtil.GetInt(roomXml, "./@ucnt"),
										XmlUtil.GetInt(roomXml, "./@scnt")
									);

				// Handle Room Variables
				if ( XmlUtil.GetSingleNode(roomXml, "./vars") != null ) {
					PopulateVariables(room.GetVariables(), roomXml);
				}

				/*
				* Merge with current room list data, to avoid destroying previous data
				* @since 1.6.0
				*/
				Room oldRoom = roomList[roomId] as Room;
				if (oldRoom != null)
				{
					room.SetVariables( oldRoom.GetVariables() );
					room.SetUserList( oldRoom.GetUserList() );
				}
				
				tempList[roomId] = room;
			}

			sfs.SetRoomList(tempList);
			
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("roomList", tempList);

			SFSEvent evt = new SFSEvent(SFSEvent.onRoomListUpdateEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle the user count change in a room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleUserCountChange(XmlNode xml) {
			int uCount = XmlUtil.GetInt(xml, "body/@u");
			int sCount = XmlUtil.GetInt(xml, "body/@s");
			int roomId = XmlUtil.GetInt(xml, "body/@r");

			Room room = (Room)sfs.GetAllRooms()[roomId];

			if ( room != null ) {
				room.SetUserCount(uCount);
				room.SetSpectatorCount(sCount);

				Hashtable parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("room", room);

				SFSEvent evt = new SFSEvent(SFSEvent.onUserCountChangeEvent, parameters);
				sfs.DispatchEvent(evt);
			}
		}


		/**
		 * <summary>
		 * Successfull room Join
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleJoinOk(XmlNode xml) {

			int roomId = XmlUtil.GetInt(xml, "body/@r");
			XmlNode userListXml = XmlUtil.GetSingleNode(xml, "body/uLs");
			int playerId = XmlUtil.GetInt(xml, "body/pid/@id");

			// Set current active room
			sfs.activeRoomId = roomId;

			// get current Room and populates usrList
			Room currRoom = sfs.GetRoom(roomId);

			if ( currRoom == null ) {
				sfs.DebugMessage("WARNING! JoinOk tries to join an unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				// Clear the old data, we need to start from a clean list
				currRoom.ClearUserList();

				// Set the player ID
				// -1 = no game room
				sfs.playerId = playerId;

				// Also set the myPlayerId in the room
				// for multi-room applications
				currRoom.SetMyPlayerIndex(playerId);

				// Handle Room Variables
				if ( XmlUtil.GetSingleNode(xml, "body/vars") != null ) {
					currRoom.ClearVariables();
					PopulateVariables(currRoom.GetVariables(), XmlUtil.GetSingleNode(xml, "body"));
				}

				// Populate Room userList
				foreach ( XmlNode usr in XmlUtil.GetNodeList(userListXml, "./u") ) {
					// grab the user properties
					string name = XmlUtil.GetString(usr, "./n/node()");
					int id = XmlUtil.GetInt(usr, "./@i");
					bool isMod = XmlUtil.GetBool(usr, "./@m");
					bool isSpec = XmlUtil.GetBool(usr, "./@s");
					int pId = XmlUtil.GetSingleNode(usr, "./@p") == null ? -1 : XmlUtil.GetInt(usr, "./@p");

					// Create and populate User
					User user = new User(id, name);
					user.SetModerator(isMod);
					user.SetIsSpectator(isSpec);
					user.SetPlayerId(pId);

					// Handle user variables
					if ( XmlUtil.GetSingleNode(usr, "./vars") != null ) {
						PopulateVariables(user.GetVariables(), usr);
					}

					// Add user
					currRoom.AddUser(user, id);
				}

				// operation completed, release lock
				sfs.changingRoom = false;

				// Fire event!
				Hashtable parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("room", currRoom);

				SFSEvent evt = new SFSEvent(SFSEvent.onJoinRoomEvent, parameters);
				sfs.DispatchEvent(evt);
			}
		}

		/**
		 * <summary>
		 * Failed room Join
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleJoinKo(XmlNode xml) {
			sfs.changingRoom = false;

			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("error", XmlUtil.GetString(xml, "body/error/@msg"));

			SFSEvent evt = new SFSEvent(SFSEvent.onJoinRoomErrorEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * New user enters the room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleUserEnterRoom(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");

			// Get params
			int usrId = XmlUtil.GetInt(xml, "body/u/@i");
			string usrName = XmlUtil.GetString(xml, "body/u/n/node()");
			bool isMod = XmlUtil.GetBool(xml, "body/u/@m");
			bool isSpec = XmlUtil.GetBool(xml, "body/u/@s");
			int pid = XmlUtil.GetInt(xml, "body/u/@p");

			XmlNode varList = XmlUtil.GetSingleNode(xml, "body/u/vars");

			Room currRoom = sfs.GetRoom(roomId);

			if ( currRoom == null ) {
				sfs.DebugMessage("WARNING! UserEnterRoom tries to enter an unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				// Create new user object
				User newUser = new User(usrId, usrName);
				newUser.SetModerator(isMod);
				newUser.SetIsSpectator(isSpec);
				newUser.SetPlayerId(pid);

				// Add user to room
				currRoom.AddUser(newUser, usrId);

				// Populate user vars
				if ( varList != null ) {
					PopulateVariables(newUser.GetVariables(), XmlUtil.GetSingleNode(xml, "body/u"));
				}

				// Fire event!
				Hashtable parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("roomId", roomId);
				parameters.Add("user", newUser);

				SFSEvent evt = new SFSEvent(SFSEvent.onUserEnterRoomEvent, parameters);
				sfs.DispatchEvent(evt);
			}
		}

		/**
		 * <summary>
		 * User leaves a room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleUserLeaveRoom(XmlNode xml) {
			int userId = XmlUtil.GetInt(xml, "body/user/@id");
			int roomId = XmlUtil.GetInt(xml, "body/@r");

			// Get room
			Room theRoom = sfs.GetRoom(roomId);

			if ( theRoom == null ) {
				sfs.DebugMessage("WARNING! UserLeaveRoom tries to leave an unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				// Get user name
				string uName = theRoom.GetUser(userId).GetName();

				// Remove user
				theRoom.RemoveUser(userId);

				// Fire event!
				Hashtable parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("roomId", roomId);
				parameters.Add("userId", userId);
				parameters.Add("userName", uName);

				SFSEvent evt = new SFSEvent(SFSEvent.onUserLeaveRoomEvent, parameters);
				sfs.DispatchEvent(evt);

			}
		}

		/**
		 * <summary>
		 * Handle public message
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandlePublicMessage(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int userId = XmlUtil.GetInt(xml, "body/user/@id");
			string message = XmlUtil.GetString(xml, "body/txt/node()");

			if ( sfs.GetRoom(roomId) == null ) {
				sfs.DebugMessage("WARNING! PublicMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {
				User sender = sfs.GetRoom(roomId).GetUser(userId);

				if ( sender == null ) {
					sfs.DebugMessage("WARNING! PublicMessage received from unknown sender. Command ignored!!");
					// TODO: We bail out here - what to do instead? Anything?

				} else {
					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("message", Entities.DecodeEntities(message));
					parameters.Add("sender", sender);
					parameters.Add("roomId", roomId);

					SFSEvent evt = new SFSEvent(SFSEvent.onPublicMessageEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		/**
		 * <summary>
		 * Handle player switched
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandlePlayerSwitched(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int playerId = XmlUtil.GetInt(xml, "body/pid/@id");
			bool isItMe = true;
			if ( XmlUtil.GetSingleNode(xml, "body/pid/@u") != null ) {
				isItMe = false;
			}

			// Synch user count, if switch successful
			Room theRoom = sfs.GetRoom(roomId);

			if ( theRoom == null ) {
				sfs.DebugMessage("WARNING! PlayerSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				if ( playerId == -1 ) {
					theRoom.SetUserCount(theRoom.GetUserCount() - 1);
					theRoom.SetSpectatorCount(theRoom.GetSpectatorCount() + 1);

					/*
					* Update another user, who was turned into a player
					*/
					if ( !isItMe ) {
						int userId = XmlUtil.GetInt(xml, "body/pid/@u");
						User user = theRoom.GetUser(userId);

						if ( user != null ) {
							user.SetIsSpectator(true);
							user.SetPlayerId(playerId);
						}
					}
				}

				/*
				* If it's me fire an event
				*/
				if (isItMe) {
					sfs.playerId = playerId;

					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("success", sfs.playerId == -1);
					parameters.Add("newId", sfs.playerId);
					parameters.Add("room", theRoom);

					SFSEvent evt = new SFSEvent(SFSEvent.onPlayerSwitchedEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}


		/**
		 * <summary>
		 * Handle private message
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandlePrivateMessage(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int userId = XmlUtil.GetInt(xml, "body/user/@id");
			string message = XmlUtil.GetString(xml, "body/txt/node()");

			if ( sfs.GetRoom(roomId) == null ) {
				sfs.DebugMessage("WARNING! PrivateMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				User sender = sfs.GetRoom(roomId).GetUser(userId);

				if ( sender == null ) {
					sfs.DebugMessage("WARNING! PrivateMessage received from unknown sender. Command ignored!!");
					// TODO: We bail out here - what to do instead? Anything?

				} else {
					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("message", Entities.DecodeEntities(message));
					parameters.Add("sender", sender);
					parameters.Add("roomId", roomId);
					parameters.Add("userId", userId);

					SFSEvent evt = new SFSEvent(SFSEvent.onPrivateMessageEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		/**
		 * <summary>
		 * Handle admin message
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleAdminMessage(XmlNode xml) {
			string message = XmlUtil.GetString(xml, "body/txt/node()");

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("message", Entities.DecodeEntities(message));

			SFSEvent evt = new SFSEvent(SFSEvent.onAdminMessageEvent, parameters);
			sfs.DispatchEvent(evt);

		}

		/**
		 * <summary>
		 * Handle moderator message
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleModMessage(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int userId = XmlUtil.GetInt(xml, "body/user/@id");
			string message = XmlUtil.GetString(xml, "body/txt/node()");

			User sender = null;
			Room room = sfs.GetRoom(roomId);

			if ( room == null ) {
				sfs.DebugMessage("WARNING! ModMessage received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {
				sender = sfs.GetRoom(roomId).GetUser(userId);

				if ( sender == null ) {
					sfs.DebugMessage("WARNING! ModMessage received from unknown sender. Command ignored!!");
					// TODO: We bail out here - what to do instead? Anything?

				} else {
					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("message", Entities.DecodeEntities(message));
					parameters.Add("sender", sender);

					SFSEvent evt = new SFSEvent(SFSEvent.onModeratorMessageEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		/**
		 * <summary>
		 * Handle SFS object received
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleSFSObject(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int userId = XmlUtil.GetInt(xml, "body/user/@id");
			string xmlStr = XmlUtil.GetString(xml, "body/dataObj/node()");

			if ( sfs.GetRoom(roomId) == null ) {
				sfs.DebugMessage("WARNING! SFSObject received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {
				User sender = sfs.GetRoom(roomId).GetUser(userId);

				if ( sender == null ) {
					sfs.DebugMessage("WARNING! SFSObject received from unknown sender. Command ignored!!");
					// TODO: We bail out here - what to do instead? Anything?

				} else {
					SFSObject asObj = SFSObjectSerializer.GetInstance().Deserialize(xmlStr);

					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("obj", asObj);
					parameters.Add("sender", sender);

					SFSEvent evt = new SFSEvent(SFSEvent.onObjectReceivedEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		/**
		 * <summary>
		 * Handle update of room variables
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleRoomVarsUpdate(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");

			Room currRoom = sfs.GetRoom(roomId);

			if ( currRoom == null ) {
				sfs.DebugMessage("WARNING! RoomVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				Hashtable changedVars = Hashtable.Synchronized(new Hashtable());

				// Handle Room Variables
				if ( XmlUtil.GetSingleNode(xml, "body/vars") != null ) {
					PopulateVariables(currRoom.GetVariables(), XmlUtil.GetSingleNode(xml, "body"), changedVars);
				}

				// Fire event!
				Hashtable parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("room", currRoom);
				parameters.Add("changedVars", changedVars);

				SFSEvent evt = new SFSEvent(SFSEvent.onRoomVariablesUpdateEvent, parameters);
				sfs.DispatchEvent(evt);
			}
		}

		/**
		 * <summary>
		 * Handle update of user variables
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		public void HandleUserVarsUpdate(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int userId = XmlUtil.GetInt(xml, "body/user/@id");

			if ( sfs.GetRoom(roomId) == null ) {
				sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {
				User currUser = sfs.GetRoom(roomId).GetUser(userId);

				if ( currUser == null ) {
					sfs.DebugMessage("WARNING! UserVarsUpdate received for unknown user. Command ignored!!");
					// TODO: We bail out here - what to do instead? Anything?

				} else {
					Hashtable changedVars = Hashtable.Synchronized(new Hashtable());

					if ( XmlUtil.GetSingleNode(xml, "body/vars") != null ) {
						PopulateVariables(currUser.GetVariables(), XmlUtil.GetSingleNode(xml, "body"), changedVars);
					}

					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("user", currUser);
					parameters.Add("changedVars", changedVars);

					SFSEvent evt = new SFSEvent(SFSEvent.onUserVariablesUpdateEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		/**
		 * <summary>
		 * Handle room added
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleRoomAdded(XmlNode xml) {
			int rId = XmlUtil.GetInt(xml, "body/rm/@id");
			string rName = XmlUtil.GetString(xml, "body/rm/name/node()");
			int rMax = XmlUtil.GetInt(xml, "body/rm/@max");
			int rSpec = XmlUtil.GetInt(xml, "body/rm/@spec");
			bool isTemp = XmlUtil.GetBool(xml, "body/rm/@temp");
			bool isGame = XmlUtil.GetBool(xml, "body/rm/@game");
			bool isPriv = XmlUtil.GetBool(xml, "body/rm/@priv");
			bool isLimbo = XmlUtil.GetBool(xml, "body/rm/@limbo");

			// Create room obj
			Room newRoom = new Room(rId, rName, rMax, rSpec, isTemp, isGame, isPriv, isLimbo);

			Hashtable rList = sfs.GetAllRooms();
			rList[rId] = newRoom;

			// Handle Room Variables
			if ( XmlUtil.GetSingleNode(xml, "body/rm/vars") != null )
				PopulateVariables(newRoom.GetVariables(), XmlUtil.GetSingleNode(xml, "body/rm"));

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("room", newRoom);

			SFSEvent evt = new SFSEvent(SFSEvent.onRoomAddedEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle room deleted
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleRoomDeleted(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/rm/@id");

			Hashtable roomList = sfs.GetAllRooms();

			// Pass the last reference to the upper level
			// If there's no other references to this room in the upper level
			// This is the last reference we're keeping

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("room", roomList[roomId]);

			// Remove reference from main room list
			roomList.Remove(roomId);

			SFSEvent evt = new SFSEvent(SFSEvent.onRoomDeletedEvent, parameters);
			sfs.DispatchEvent(evt);
		}


		/**
		 * <summary>
		 * Handle random key received
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleRandomKey(XmlNode xml) {
			string key = XmlUtil.GetString(xml, "body/k/node()");

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("key", key);

			SFSEvent evt = new SFSEvent(SFSEvent.onRandomKeyEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle reound trip benchmark
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleRoundTripBench(XmlNode xml) {
			DateTime now = DateTime.Now;
			TimeSpan res = now - sfs.GetBenchStartTime();

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("elapsed", Convert.ToInt32(res.TotalMilliseconds));

			SFSEvent evt = new SFSEvent(SFSEvent.onRoundTripResponseEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle unsuccessful create room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleCreateRoomError(XmlNode xml) {
			string errMsg = XmlUtil.GetString(xml, "body/room/@e");

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("error", errMsg);

			SFSEvent evt = new SFSEvent(SFSEvent.onCreateRoomErrorEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle buddy list received
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleBuddyList(XmlNode xml) {
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());

			sfs.buddyList.Clear();
			sfs.myBuddyVars.Clear();

			// Get my buddy variables
			if ( XmlUtil.GetSingleNode(xml, "body/mv") != null ) {
				foreach ( XmlNode myVar in XmlUtil.GetNodeList(xml, "body/mv/v") ) {
					sfs.myBuddyVars[XmlUtil.GetString(myVar, "./@n")] = XmlUtil.GetString(myVar, "./node()");
				}
			}

			// Get all buddies and variables
			if ( XmlUtil.GetSingleNode(xml, "body/bList/b") != null ) {
				foreach ( XmlNode b in XmlUtil.GetNodeList(xml, "body/bList/b") ) {
					Buddy buddy = new Buddy(XmlUtil.GetInt(b, "./@i"),
						XmlUtil.GetString(b, "./n/node()"),
						XmlUtil.GetBool(b, "./@s"),
						XmlUtil.GetBool(b, "./@x"),
						Hashtable.Synchronized(new Hashtable()));

					// Runs through buddy variables
					if ( XmlUtil.GetSingleNode(b, "./vs") != null ) {
						foreach ( XmlNode bVar in XmlUtil.GetNodeList(b, "./vs/v") ) {
							buddy.SetVariable(XmlUtil.GetString(bVar, "./@n"), XmlUtil.GetString(bVar, "./node()"));
						}
					}

					sfs.buddyList.Add(buddy);
				}

				// Fire event!
				parameters.Add("list", sfs.buddyList);
				SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
				sfs.DispatchEvent(evt);
			}

			else {
				// Either list is empty
				if ( XmlUtil.GetNodeList(xml, "body/bList/b").Count == 0 ) {
					// Fire event!
					parameters.Add("list", sfs.buddyList);
					SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
					sfs.DispatchEvent(evt);
					
				// or throw buddy List load error!			
				} else {
					// Fire event!
					parameters.Add("error", XmlUtil.GetString(xml, "body/err/node()"));
					SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListErrorEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}


		/**
		 * <summary>
		 * Handle update of buddy list
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleBuddyListUpdate(XmlNode xml) {
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			SFSEvent evt = null;

			if ( XmlUtil.GetSingleNode(xml, "body/b") != null ) {
				Buddy buddy = new Buddy(XmlUtil.GetInt(xml, "body/b/@i"),
					XmlUtil.GetString(xml, "body/b/n/node()"),
					XmlUtil.GetBool(xml, "body/b/@s"),
					XmlUtil.GetBool(xml, "body/b/@x"));

				// Runs through buddy variables
				XmlNode bVars = XmlUtil.GetSingleNode(xml, "body/b/vs");

				bool found = false;

				foreach ( Buddy tempB in sfs.buddyList ) {
					if ( tempB.GetName() == buddy.GetName() ) {
						buddy.SetBlocked(tempB.IsBlocked());
						buddy.SetVariables(tempB.GetVariables());

						// add/modify variables
						if ( bVars != null ) {
							foreach ( XmlNode bVar in XmlUtil.GetNodeList(bVars, "./v") ) {
								buddy.SetVariable(XmlUtil.GetString(bVar, "./@n"), XmlUtil.GetString(bVar, "./node()"));
							}
						}

						// swap objects
						sfs.buddyList.Remove(tempB);
						sfs.buddyList.Add(buddy);

						found = true;
						break;
					}
				}

				// Fire event!
				if ( found ) {
					parameters.Add("buddy", buddy);

					evt = new SFSEvent(SFSEvent.onBuddyListUpdateEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}

			// Buddy List load error!
			else {
				// Fire event!
				parameters.Add("error", XmlUtil.GetString(xml, "body/err/node()"));
				evt = new SFSEvent(SFSEvent.onBuddyListErrorEvent, parameters);
				sfs.DispatchEvent(evt);
			}
		}

		/**
		 * <summary>
		 * Handle permission to add buddy
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleAddBuddyPermission(XmlNode xml) {
			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("sender", XmlUtil.GetString(xml, "body/n/node()"));
			parameters.Add("message", "");

			if ( XmlUtil.GetSingleNode(xml, "body/txt") != null )
				parameters.Add("message", Entities.DecodeEntities(XmlUtil.GetString(xml, "body/txt")));

			SFSEvent evt = new SFSEvent(SFSEvent.onBuddyPermissionRequestEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle buddy added
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleBuddyAdded(XmlNode xml) {
			// Check for duplicate buddy before adding
			int buddyId = XmlUtil.GetInt(xml, "body/b/@i");
			bool duplicateFound = false;
			foreach ( Buddy myBuddy in sfs.buddyList ) {
				if ( myBuddy.GetId() == buddyId ) {
					duplicateFound = true;
					break;
				}
			}
			
			if (duplicateFound) {
				sfs.DebugMessage("WARNING! Server attempted to add duplicate buddy for buddy id: " + buddyId);
				return;
			}			
			
			Buddy buddy = new Buddy(buddyId,
				XmlUtil.GetString(xml, "body/b/n/node()"),
				XmlUtil.GetBool(xml, "body/b/@s"),
				XmlUtil.GetBool(xml, "body/b/@x"));

			// Runs through buddy variables
			if ( XmlUtil.GetSingleNode(xml, "body/b/vs") != null ) {
				foreach ( XmlNode bVar in XmlUtil.GetNodeList(xml, "body/b/vs/v") ) {
					buddy.SetVariable(XmlUtil.GetString(bVar, "./@n"), XmlUtil.GetString(bVar, "./node()"));
				}
			}

			sfs.buddyList.Add(buddy);

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("list", sfs.buddyList);

			SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle remove buddy
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleRemoveBuddy(XmlNode xml) {
			string buddyName = XmlUtil.GetString(xml, "body/n/node()");

			foreach ( Buddy buddy in sfs.buddyList ) {
				if ( buddy.GetName() == buddyName ) {
					sfs.buddyList.Remove(buddy);

					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("list", sfs.buddyList);

					SFSEvent evt = new SFSEvent(SFSEvent.onBuddyListEvent, parameters);
					sfs.DispatchEvent(evt);

					break;
				}
			}
		}

		/**
		 * <summary>
		 * Handle buddy room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleBuddyRoom(XmlNode xml) {
			string roomIds = XmlUtil.GetString(xml, "body/br/@r");
			SyncArrayList ids = new SyncArrayList();
			foreach ( string s in roomIds.Split(",".ToCharArray()) ) {
				ids.Add(int.Parse(s));
			}

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("idList", ids);

			SFSEvent evt = new SFSEvent(SFSEvent.onBuddyRoomEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle leave room
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleLeaveRoom(XmlNode xml) {
			int roomLeft = XmlUtil.GetInt(xml, "body/rm/@id");

			// Fire event!
			Hashtable parameters = Hashtable.Synchronized(new Hashtable());
			parameters.Add("roomId", roomLeft);

			SFSEvent evt = new SFSEvent(SFSEvent.onRoomLeftEvent, parameters);
			sfs.DispatchEvent(evt);
		}

		/**
		 * <summary>
		 * Handle spectator switched
		 * </summary>
		 * 
		 * <param name="xml">message object</param>
		 */
		private void HandleSpectatorSwitched(XmlNode xml) {
			int roomId = XmlUtil.GetInt(xml, "body/@r");
			int playerId = XmlUtil.GetInt(xml, "body/pid/@id");

			// Synch user count, if switch successful
			Room theRoom = sfs.GetRoom(roomId);

			if ( theRoom == null ) {
				sfs.DebugMessage("WARNING! SpectatorSwitched received for unknown room. Command ignored!! Roomlist not up to date?");
				// TODO: We bail out here - what to do instead? Anything?

			} else {

				if ( playerId > 0 ) {
					theRoom.SetUserCount(theRoom.GetUserCount() + 1);
					theRoom.SetSpectatorCount(theRoom.GetSpectatorCount() - 1);
				}

				/*
				* Update another user, who was turned into a player
				*/
				if ( XmlUtil.GetSingleNode(xml, "body/pid/@u") != null ) {
					int userId = XmlUtil.GetInt(xml, "body/pid/@u");
					User user = theRoom.GetUser(userId);

					if ( user != null ) {
						user.SetIsSpectator(false);
						user.SetPlayerId(playerId);
					}
				}

				/*
				* Update myself
				*/
				else {
					sfs.playerId = playerId;

					// Fire event!
					Hashtable parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("success", sfs.playerId > 0);
					parameters.Add("newId", sfs.playerId);
					parameters.Add("room", theRoom);

					SFSEvent evt = new SFSEvent(SFSEvent.onSpectatorSwitchedEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}
		}

		//=======================================================================
		// Other class methods
		//=======================================================================

		/**
		 * <summary><see cref="PopulateVariables(Hashtable, XmlNode, Hashtable)"/></summary>
		 */
		private void PopulateVariables(Hashtable variables, XmlNode xmlData) {
			PopulateVariables(variables, xmlData, null);
		}
		/**
		 * <summary>
		 * Takes an SFS variables XML node and store it in an array<br/>
		 * Usage: for parsing room and user variables
		 * </summary>
		 * 
		 * <param name="variables">variable list to populate</param>
		 * <param name="xmlData">the XML variables node</param>
		 * <param name="changedVars">list of changed variables</param>
		 */
		private void PopulateVariables(Hashtable variables, XmlNode xmlData, Hashtable changedVars) {
			foreach ( XmlNode v in XmlUtil.GetNodeList(xmlData, "vars/var") ) {
				string vName = XmlUtil.GetString(v, "./@n");
				string vType = XmlUtil.GetString(v, "./@t");
				string vValue = XmlUtil.GetString(v, "./node()");

				// Add the vName to the list of changed vars
				// The changed List is an array that can contains all the
				// var names changed with numeric indexes but also contains
				// the var names as keys for faster search
				if ( changedVars != null ) {
					changedVars.Add(vName, true);
				}

				if ( vType == "b" )
					variables[vName] = (string)vValue == "1" ? true : false;

				else if ( vType == "n" )
					variables[vName] = double.Parse(vValue);

				else if ( vType == "s" )
					variables[vName] = (string)vValue;

				else if ( vType == "x" )
					variables.Remove(vName);

			}
		}


		/**
		 * <summary>
		 * Handle disconnects
		 * </summary>
		 */
		public void DispatchDisconnection() {
			SFSEvent evt = new SFSEvent(SFSEvent.onConnectionLostEvent, null);
			sfs.DispatchEvent(evt);
		}
	}
}