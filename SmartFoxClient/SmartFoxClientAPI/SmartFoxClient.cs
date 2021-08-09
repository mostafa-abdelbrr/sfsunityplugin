using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using LitJson;
using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Handlers;
using SmartFoxClientAPI.Http;
using SmartFoxClientAPI.Util;

namespace SmartFoxClientAPI
{
	public class SmartFoxClient : IDisposable
	{
		public delegate void OnAdminMessageDelegate(string message);

		public delegate void OnBuddyListDelegate(ArrayList buddyList);

		public delegate void OnBuddyListErrorDelegate(string error);

		public delegate void OnBuddyListUpdateDelegate(Buddy buddy);

		public delegate void OnBuddyPermissionRequestDelegate(string sender, string message);

		public delegate void OnBuddyRoomDelegate(ArrayList idList);

		public delegate void OnConfigLoadFailureDelegate(string message);

		public delegate void OnConfigLoadSuccessDelegate();

		public delegate void OnConnectionDelegate(bool success, string error);

		public delegate void OnConnectionLostDelegate();

		public delegate void OnCreateRoomErrorDelegate(string error);

		public delegate void OnDebugMessageDelegate(string message);

		public delegate void OnExtensionResponseDelegate(object dataObj, string type);

		public delegate void OnJoinRoomDelegate(Room room);

		public delegate void OnJoinRoomErrorDelegate(string error);

		public delegate void OnLoginDelegate(bool success, string name, string error);

		public delegate void OnLogoutDelegate();

		public delegate void OnModeratorMessageDelegate(string message, User sender);

		public delegate void OnObjectReceivedDelegate(SFSObject obj, User sender);

		public delegate void OnPlayerSwitchedDelegate(bool success, int newId, Room room);

		public delegate void OnPrivateMessageDelegate(string message, User sender, int roomId, int userId);

		public delegate void OnPublicMessageDelegate(string message, User sender, int roomId);

		public delegate void OnRandomKeyDelegate(string key);

		public delegate void OnRoomAddedDelegate(Room room);

		public delegate void OnRoomDeletedDelegate(Room room);

		public delegate void OnRoomLeftDelegate(int roomId);

		public delegate void OnRoomListUpdateDelegate(Hashtable roomList);

		public delegate void OnRoomVariablesUpdateDelegate(Room room, Hashtable changedVars);

		public delegate void OnRoundTripResponseDelegate(int elapsed);

		public delegate void OnSpectatorSwitchedDelegate(bool success, int newId, Room room);

		public delegate void OnUserCountChangeDelegate(Room room);

		public delegate void OnUserEnterRoomDelegate(int roomId, User user);

		public delegate void OnUserLeaveRoomDelegate(int roomId, int userId, string userName);

		public delegate void OnUserVariablesUpdateDelegate(User user, Hashtable changedVars);

		internal const string onAdminMessageEvent = "OnAdminMessage";

		internal const string onBuddyListEvent = "OnBuddyList";

		internal const string onBuddyListErrorEvent = "OnBuddyListError";

		internal const string onBuddyListUpdateEvent = "OnBuddyListUpdate";

		internal const string onBuddyPermissionRequestEvent = "OnBuddyPermissionRequest";

		internal const string onBuddyRoomEvent = "OnBuddyRoom";

		internal const string onConfigLoadFailureEvent = "OnConfigLoadFailure";

		internal const string onConfigLoadSuccessEvent = "OnConfigLoadSuccess";

		internal const string onConnectionEvent = "OnConnection";

		internal const string onConnectionLostEvent = "OnConnectionLost";

		internal const string onCreateRoomErrorEvent = "OnCreateRoomError";

		internal const string onDebugMessageEvent = "OnDebugMessage";

		internal const string onExtensionResponseEvent = "OnExtensionResponse";

		internal const string onJoinRoomEvent = "OnJoinRoom";

		internal const string onJoinRoomErrorEvent = "OnJoinRoomError";

		internal const string onLoginEvent = "OnLogin";

		internal const string onLogoutEvent = "OnLogout";

		internal const string onModeratorMessageEvent = "OnModMessage";

		internal const string onObjectReceivedEvent = "OnObjectReceived";

		internal const string onPlayerSwitchedEvent = "OnPlayerSwitched";

		internal const string onPrivateMessageEvent = "OnPrivateMessage";

		internal const string onPublicMessageEvent = "OnPublicMessage";

		internal const string onRandomKeyEvent = "OnRandomKey";

		internal const string onRoomAddedEvent = "OnRoomAdded";

		internal const string onRoomDeletedEvent = "OnRoomDeleted";

		internal const string onRoomLeftEvent = "OnRoomLeft";

		internal const string onRoomListUpdateEvent = "OnRoomListUpdate";

		internal const string onRoomVariablesUpdateEvent = "OnRoomVariablesUpdate";

		internal const string onRoundTripResponseEvent = "OnRoundTripResponse";

		internal const string onSpectatorSwitchedEvent = "OnSpectatorSwitched";

		internal const string onUserCountChangeEvent = "OnUserCountChange";

		internal const string onUserEnterRoomEvent = "OnUserEnterRoom";

		internal const string onUserLeaveRoomEvent = "OnUserLeaveRoom";

		internal const string onUserVariablesUpdateEvent = "OnUserVariablesUpdate";

		private const int EOM = 0;

		private const int MIN_POLL_SPEED = 0;

		private const int DEFAULT_POLL_SPEED = 750;

		private const int MAX_POLL_SPEED = 10000;

		private const string HTTP_POLL_REQUEST = "poll";

		public const string MODMSG_TO_USER = "u";

		public const string MODMSG_TO_ROOM = "r";

		public const string MODMSG_TO_ZONE = "z";

		public const string XTMSG_TYPE_XML = "xml";

		public const string XTMSG_TYPE_STR = "str";

		public const string XTMSG_TYPE_JSON = "json";

		public const string CONNECTION_MODE_DISCONNECTED = "disconnected";

		public const string CONNECTION_MODE_SOCKET = "socket";

		public const string CONNECTION_MODE_HTTP = "http";

		private const int READ_BUFFER_SIZE = 4096;

		public OnAdminMessageDelegate onAdminMessage;

		public OnBuddyListDelegate onBuddyList;

		public OnBuddyListErrorDelegate onBuddyListError;

		public OnBuddyListUpdateDelegate onBuddyListUpdate;

		public OnBuddyPermissionRequestDelegate onBuddyPermissionRequest;

		public OnBuddyRoomDelegate onBuddyRoom;

		public OnConfigLoadFailureDelegate onConfigLoadFailure;

		public OnConfigLoadSuccessDelegate onConfigLoadSuccess;

		public OnConnectionDelegate onConnection;

		public OnConnectionLostDelegate onConnectionLost;

		public OnCreateRoomErrorDelegate onCreateRoomError;

		public OnDebugMessageDelegate onDebugMessage;

		public OnExtensionResponseDelegate onExtensionResponse;

		public OnJoinRoomDelegate onJoinRoom;

		public OnJoinRoomErrorDelegate onJoinRoomError;

		public OnLoginDelegate onLogin;

		public OnLogoutDelegate onLogout;

		public OnModeratorMessageDelegate onModeratorMessage;

		public OnObjectReceivedDelegate onObjectReceived;

		public OnPlayerSwitchedDelegate onPlayerSwitched;

		public OnPrivateMessageDelegate onPrivateMessage;

		public OnPublicMessageDelegate onPublicMessage;

		public OnRandomKeyDelegate onRandomKey;

		public OnRoomAddedDelegate onRoomAdded;

		public OnRoomDeletedDelegate onRoomDeleted;

		public OnRoomLeftDelegate onRoomLeft;

		public OnRoomListUpdateDelegate onRoomListUpdate;

		public OnRoomVariablesUpdateDelegate onRoomVariablesUpdate;

		public OnRoundTripResponseDelegate onRoundTripResponse;

		public OnSpectatorSwitchedDelegate onSpectatorSwitched;

		public OnUserCountChangeDelegate onUserCountChange;

		public OnUserEnterRoomDelegate onUserEnterRoom;

		public OnUserLeaveRoomDelegate onUserLeaveRoom;

		public OnUserVariablesUpdateDelegate onUserVariablesUpdate;

		private static string MSG_XML = "<";

		private static string MSG_JSON = "{";

		private static string MSG_STR = "%";

		private Hashtable messageHandlers = new Hashtable();

		private TcpClient socketConnection;

		private NetworkStream networkStream;

		private byte[] byteBuffer = new byte[4096];

		private string messageBuffer = "";

		private bool connected;

		private bool connecting = false;

		private bool autoConnectOnConfigSuccess = false;

		private Thread thrSocketReader;

		private Thread thrConnect;

		private Thread thrHttpPoll;

		private Queue sfsQueuedEvents = Queue.Synchronized(new Queue());

		private object sfsQueuedEventsLocker = new object();

		private bool isHttpMode = false;

		private int _httpPollSpeed = 750;

		private HttpConnection httpConnection;

		private Hashtable roomList = new Hashtable();

		private DateTime benchStartTime;

		private SysHandler sysHandler;

		private ExtHandler extHandler;

		private int majVersion;

		private int minVersion;

		private int subVersion;

		private bool isDisposed = false;

		public string ipAddress;

		public int port = 9339;

		public string defaultZone;

		public bool runInQueueMode = false;

		public string blueBoxIpAddress;

		public int blueBoxPort = 0;

		public bool smartConnect = true;

		public SyncArrayList buddyList = new SyncArrayList();

		public Hashtable myBuddyVars = new Hashtable();

		public bool debug;

		public int myUserId;

		public string myUserName;

		public int playerId;

		public bool amIModerator;

		public int activeRoomId;

		public bool changingRoom;

		public int httpPort = 8080;

		private object disconnectionLocker = new object();

		public string GetRawProtocolSeparator()
		{
			return MSG_STR;
		}

		public void SetRawProtocolSeparator(string value)
		{
			if (value != "<" && value != "{")
			{
				MSG_STR = value;
			}
		}

		public bool IsConnected()
		{
			return connected;
		}

		public void SetIsConnected(bool b)
		{
			connected = b;
		}

		public int GetHttpPollSpeed()
		{
			return _httpPollSpeed;
		}

		public void SetHttpPollSpeed(int sp)
		{
			if (sp >= 0 && sp <= 10000)
			{
				_httpPollSpeed = sp;
			}
		}

		public SmartFoxClient()
			: this(debug: false)
		{
		}

		public SmartFoxClient(bool debug)
		{
			majVersion = 1;
			minVersion = 5;
			subVersion = 4;
			activeRoomId = -1;
			this.debug = debug;
			messageHandlers.Clear();
			SetupMessageHandlers();
			httpConnection = new HttpConnection(this);
			httpConnection.AddEventListener("OnHttpConnect", HandleHttpConnect);
			httpConnection.AddEventListener("OnHttpClose", HandleHttpClose);
			httpConnection.AddEventListener("OnHttpError", HandleHttpError);
			httpConnection.AddEventListener("OnHttpData", HandleHttpData);
		}

		~SmartFoxClient()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
				}
				if (connected)
				{
					Disconnect();
				}
				isDisposed = true;
			}
		}

		public string GetConnectionMode()
		{
			string result = "disconnected";
			if (IsConnected())
			{
				result = ((!isHttpMode) ? "socket" : "http");
			}
			return result;
		}

		public void Connect(string ipAdr)
		{
			Connect(ipAdr, 9339);
		}

		public void Connect(string ipAdr, int port)
		{
			DebugMessage("Trying to connect");
			if (!connected && !connecting)
			{
				try
				{
					connecting = true;
					Initialize();
					ipAddress = ipAdr;
					this.port = port;
					thrConnect = new Thread(ConnectThread);
					thrConnect.Start();
				}
				catch (Exception ex)
				{
					connecting = false;
					HandleIOError(ex.Message);
				}
			}
			else
			{
				DebugMessage("*** ALREADY CONNECTED ***");
			}
		}

		private void ConnectThread()
		{
			try
			{
				IPAddress address = IPAddress.Parse(ipAddress);
				socketConnection = new TcpClient();
				socketConnection.Connect(address, port);
				connected = true;
				networkStream = socketConnection.GetStream();
				thrSocketReader = new Thread(HandleSocketData);
				thrSocketReader.Start();
				HandleSocketConnection(this, new EventArgs());
			}
			catch (FormatException)
			{
				DebugMessage("API only accepts IP addresses for connections");
				connecting = false;
			}
			catch (SocketException ex2)
			{
				DebugMessage("SocketExc " + ex2.ToString());
				connecting = false;
				HandleIOError(ex2.Message);
			}
			connecting = false;
		}

		public void Disconnect()
		{
			lock (disconnectionLocker)
			{
				if (!isHttpMode)
				{
					if (connected)
					{
						try
						{
							socketConnection.Client.Shutdown(SocketShutdown.Both);
						}
						catch (Exception ex)
						{
							DebugMessage("Disconnect Exception: " + ex.ToString());
						}
						connected = false;
					}
				}
				else
				{
					try
					{
						if (thrHttpPoll != null && thrHttpPoll.IsAlive)
						{
							thrHttpPoll.Abort();
						}
						httpConnection.Close();
					}
					catch (Exception ex)
					{
						DebugMessage("Disconnect Exception: " + ex.ToString());
					}
					connected = false;
				}
				HandleSocketDisconnection();
			}
		}

		public void AutoJoin()
		{
			if (CheckRoomList())
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				Send(hashtable, "autoJoin", activeRoomId, "");
			}
		}

		public void Login(string zone, string name, string pass)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string message = "<login z='" + zone + "'><nick><![CDATA[" + name + "]]></nick><pword><![CDATA[" + pass + "]]></pword></login>";
			Send(hashtable, "login", 0, message);
		}

		public void Logout()
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "logout", -1, "");
		}

		public void LoadConfig()
		{
			LoadConfig("config.xml", autoConnect: true);
		}

		public void LoadConfig(string configFile)
		{
			LoadConfig(configFile, autoConnect: true);
		}

		public void LoadConfig(string configFile, bool autoConnect)
		{
			autoConnectOnConfigSuccess = autoConnect;
			WebClient webClient = new WebClient();
			string xml;
			SFSEvent evt;
			try
			{
				Stream stream = webClient.OpenRead(configFile);
				StreamReader streamReader = new StreamReader(stream);
				xml = streamReader.ReadToEnd();
				stream.Close();
			}
			catch (Exception ex)
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("message", ex.Message);
				evt = new SFSEvent("OnConfigLoadFailure", hashtable);
				DispatchEvent(evt);
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			ipAddress = (blueBoxIpAddress = XmlUtil.GetString(xmlDocument, "./ip"));
			port = XmlUtil.GetInt(xmlDocument, "./port");
			defaultZone = XmlUtil.GetString(xmlDocument, "./zone");
			if (XmlUtil.GetSingleNode(xmlDocument, "./blueBoxIpAddress") != null)
			{
				blueBoxIpAddress = XmlUtil.GetString(xmlDocument, "./blueBoxIpAddress");
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./blueBoxPort") != null)
			{
				blueBoxPort = XmlUtil.GetInt(xmlDocument, "./blueBoxPort");
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./debug") != null)
			{
				debug = XmlUtil.GetBool(xmlDocument, "./debug");
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./smartConnect") != null)
			{
				smartConnect = XmlUtil.GetBool(xmlDocument, "./smartConnect");
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./httpPort") != null)
			{
				httpPort = XmlUtil.GetInt(xmlDocument, "./httpPort");
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./httpPollSpeed") != null)
			{
				SetHttpPollSpeed(XmlUtil.GetInt(xmlDocument, "./httpPollSpeed"));
			}
			if (XmlUtil.GetSingleNode(xmlDocument, "./rawProtocolSeparator") != null)
			{
				SetRawProtocolSeparator(XmlUtil.GetString(xmlDocument, "./rawProtocolSeparator"));
			}
			if (autoConnectOnConfigSuccess)
			{
				Connect(ipAddress, port);
				return;
			}
			evt = new SFSEvent("OnConfigLoadSuccess", null);
			DispatchEvent(evt);
		}

		public void ProcessEventQueue()
		{
			if (runInQueueMode)
			{
				Queue queue = null;
				lock (sfsQueuedEventsLocker)
				{
					queue = sfsQueuedEvents.Clone() as Queue;
					sfsQueuedEvents.Clear();
				}
				while (queue.Count > 0)
				{
					SFSEvent evt = queue.Dequeue() as SFSEvent;
					_DispatchEvent(evt);
				}
			}
		}

		public void ProcessSingleEventInEventQueue()
		{
			if (!runInQueueMode)
			{
				return;
			}
			SFSEvent sFSEvent = null;
			lock (sfsQueuedEventsLocker)
			{
				if (sfsQueuedEvents.Count > 0)
				{
					sFSEvent = sfsQueuedEvents.Dequeue() as SFSEvent;
				}
			}
			if (sFSEvent != null)
			{
				_DispatchEvent(sFSEvent);
			}
		}

		public int NumEventsInEventQueue()
		{
			lock (sfsQueuedEventsLocker)
			{
				return sfsQueuedEvents.Count;
			}
		}

		public void AddBuddy(string buddyName)
		{
			if (buddyName != myUserName && !CheckBuddyDuplicates(buddyName))
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message = "<n>" + buddyName + "</n>";
				Send(hashtable, "addB", -1, message);
			}
		}

		private bool CheckBuddyDuplicates(string buddyName)
		{
			bool result = false;
			foreach (Buddy buddy in buddyList)
			{
				if (buddy.GetName() == buddyName)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public void ClearBuddyList()
		{
			buddyList = new SyncArrayList();
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "clearB", -1, "");
			Hashtable hashtable2 = new Hashtable();
			hashtable2.Add("list", buddyList);
			SFSEvent evt = new SFSEvent("OnBuddyList", hashtable2);
			DispatchEvent(evt);
		}

		public Buddy GetBuddyByName(string buddyName)
		{
			foreach (Buddy buddy in buddyList)
			{
				if (buddy.GetName() == buddyName)
				{
					return buddy;
				}
			}
			return null;
		}

		public Buddy GetBuddyById(int id)
		{
			foreach (Buddy buddy in buddyList)
			{
				if (buddy.GetId() == id)
				{
					return buddy;
				}
			}
			return null;
		}

		public void GetBuddyRoom(Buddy buddy)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			hashtable.Add("bid", buddy.GetId());
			if (buddy.GetId() != -1)
			{
				Send(hashtable, "roomB", -1, "<b id='" + buddy.GetId() + "' />");
			}
		}

		public void LoadBuddyList()
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "loadB", -1, "");
		}

		public void RemoveBuddy(string buddyName)
		{
			bool flag = false;
			foreach (Buddy buddy in buddyList)
			{
				if (buddy.GetName() == buddyName)
				{
					buddyList.Remove(buddy);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message = "<n>" + buddyName + "</n>";
				Send(hashtable, "remB", -1, message);
				Hashtable hashtable2 = new Hashtable();
				hashtable2.Add("list", buddyList);
				SFSEvent evt = new SFSEvent("OnBuddyList", hashtable2);
				DispatchEvent(evt);
			}
		}

		public void SendBuddyPermissionResponse(bool allowBuddy, string targetBuddy)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string message = "<n res='" + (allowBuddy ? "g" : "r") + "'>" + targetBuddy + "</n>";
			Send(hashtable, "bPrm", -1, message);
		}

		public void SetBuddyBlockStatus(string buddyName, bool status)
		{
			Buddy buddyByName = GetBuddyByName(buddyName);
			if (buddyByName != null && buddyByName.IsBlocked() != status)
			{
				buddyByName.SetBlocked(status);
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message = "<n x='" + (status ? "1" : "0") + "'>" + buddyName + "</n>";
				Send(hashtable, "setB", -1, message);
				Hashtable hashtable2 = new Hashtable();
				hashtable2.Add("buddy", buddyByName);
				SFSEvent evt = new SFSEvent("OnBuddyListUpdate", hashtable2);
				DispatchEvent(evt);
			}
		}

		public void SetBuddyVariables(Hashtable varList)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string text = "<vars>";
			foreach (string key in varList.Keys)
			{
				string text3 = (string)varList[key];
				if ((string)myBuddyVars[key] != text3)
				{
					myBuddyVars[key] = text3;
					string text4 = text;
					text = text4 + "<var n='" + key + "'><![CDATA[" + text3 + "]]></var>";
				}
			}
			text += "</vars>";
			Send(hashtable, "setBvars", -1, text);
		}

		public void CreateRoom(NewRoomDescriptor roomObj)
		{
			CreateRoom(roomObj, -1);
		}

		public void CreateRoom(NewRoomDescriptor roomObj, int roomId)
		{
			if (!CheckRoomList() || !CheckJoin())
			{
				return;
			}
			if (roomId == -1)
			{
				roomId = activeRoomId;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string text = (roomObj.IsGame ? "1" : "0");
			string text2 = (roomObj.ExitCurrentRoom ? "1" : "0");
			string text3 = "<room tmp='1' gam='" + text + "' spec='" + Convert.ToString(roomObj.MaxSpectators) + "' exit='" + text2 + "'>";
			text3 = text3 + "<name><![CDATA[" + roomObj.Name + "]]></name>";
			text3 = text3 + "<pwd><![CDATA[" + roomObj.Password + "]]></pwd>";
			text3 = text3 + "<max>" + Convert.ToString(roomObj.MaxUsers) + "</max>";
			text3 = text3 + "<uCnt>" + (roomObj.ReceiveUCount ? "1" : "0") + "</uCnt>";
			if (roomObj.Extension != null)
			{
				text3 = text3 + "<xt n='" + roomObj.Extension.Name;
				text3 = text3 + "' s='" + roomObj.Extension.Script + "' />";
			}
			if (roomObj.Variables.Count == 0)
			{
				text3 += "<vars></vars>";
			}
			else
			{
				text3 += "<vars>";
				foreach (RoomVariable variable in roomObj.Variables)
				{
					text3 += GetXmlRoomVariable(variable);
				}
				text3 += "</vars>";
			}
			text3 += "</room>";
			Send(hashtable, "createRoom", roomId, text3);
		}

		public void CreateRoom(Hashtable roomObj)
		{
			CreateRoom(roomObj, -1);
		}

		public void CreateRoom(Hashtable roomObj, int roomId)
		{
			ErrorTrace("*DEPRECATED* - Use object based CreateRoom instead of hashtable based");
			if (!CheckRoomList() || !CheckJoin())
			{
				return;
			}
			if (roomId == -1)
			{
				roomId = activeRoomId;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string text = "0";
			if (roomObj.ContainsKey("isGame"))
			{
				text = (((bool)roomObj["isGame"]) ? "1" : "0");
			}
			string text2 = "1";
			string text3 = (roomObj.ContainsKey("maxUsers") ? Convert.ToString(roomObj["maxUsers"]) : "0");
			string text4 = (roomObj.ContainsKey("maxSpectators") ? Convert.ToString(roomObj["maxSpectators"]) : "0");
			if (text == "1" && roomObj.ContainsKey("exitCurrent"))
			{
				text2 = (((bool)roomObj["exitCurrent"]) ? "1" : "0");
			}
			string text5 = "<room tmp='1' gam='" + text + "' spec='" + text4 + "' exit='" + text2 + "'>";
			object obj = text5;
			text5 = string.Concat(obj, "<name><![CDATA[", (roomObj["name"] == null) ? "" : roomObj["name"], "]]></name>");
			obj = text5;
			text5 = string.Concat(obj, "<pwd><![CDATA[", (roomObj["password"] == null) ? "" : roomObj["password"], "]]></pwd>");
			text5 = text5 + "<max>" + text3 + "</max>";
			if (roomObj.ContainsKey("uCount"))
			{
				text5 = text5 + "<uCnt>" + (((bool)roomObj["uCount"]) ? "1" : "0") + "</uCnt>";
			}
			if (roomObj.ContainsKey("extension"))
			{
				text5 = text5 + "<xt n='" + ((Hashtable)roomObj["extension"])["name"];
				obj = text5;
				text5 = string.Concat(obj, "' s='", ((Hashtable)roomObj["extension"])["script"], "' />");
			}
			if (!roomObj.ContainsKey("vars"))
			{
				text5 += "<vars></vars>";
			}
			else
			{
				text5 += "<vars>";
				foreach (RoomVariable item in (ArrayList)roomObj["vars"])
				{
					text5 += GetXmlRoomVariable(item);
				}
				text5 += "</vars>";
			}
			text5 += "</room>";
			Send(hashtable, "createRoom", roomId, text5);
		}

		public Hashtable GetAllRooms()
		{
			return roomList;
		}

		public Room GetRoom(int roomId)
		{
			if (!CheckRoomList())
			{
				return null;
			}
			return (Room)roomList[roomId];
		}

		public Room GetRoomByName(string roomName)
		{
			if (!CheckRoomList())
			{
				return null;
			}
			Room result = null;
			foreach (Room value in roomList.Values)
			{
				if (value.GetName() == roomName)
				{
					result = value;
					break;
				}
			}
			return result;
		}

		public void GetRoomList()
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "getRmList", activeRoomId, "");
		}

		public Room GetActiveRoom()
		{
			if (!CheckRoomList() || !CheckJoin())
			{
				return null;
			}
			return (Room)roomList[activeRoomId];
		}

		public void JoinRoom(object newRoom)
		{
			JoinRoom(newRoom, "", isSpectator: false, dontLeave: false, -1);
		}

		public void JoinRoom(object newRoom, string pword)
		{
			JoinRoom(newRoom, pword, isSpectator: false, dontLeave: false, -1);
		}

		public void JoinRoom(object newRoom, string pword, bool isSpectator)
		{
			JoinRoom(newRoom, pword, isSpectator, dontLeave: false, -1);
		}

		public void JoinRoom(object newRoom, string pword, bool isSpectator, bool dontLeave)
		{
			JoinRoom(newRoom, pword, isSpectator, dontLeave, -1);
		}

		public void JoinRoom(object newRoom, string pword, bool isSpectator, bool dontLeave, int oldRoom)
		{
			if (!CheckRoomList())
			{
				return;
			}
			int num = -1;
			int num2 = (isSpectator ? 1 : 0);
			if (changingRoom)
			{
				return;
			}
			if (newRoom.GetType() == typeof(int))
			{
				num = (int)newRoom;
			}
			else if (newRoom.GetType() == typeof(string))
			{
				foreach (Room value in roomList.Values)
				{
					if (value.GetName() == (string)newRoom)
					{
						num = value.GetId();
						break;
					}
				}
			}
			if (num != -1)
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string text = (dontLeave ? "0" : "1");
				int num3 = ((oldRoom > -1) ? oldRoom : activeRoomId);
				if (activeRoomId == -1)
				{
					text = "0";
					num3 = -1;
				}
				string message = "<room id='" + num + "' pwd='" + pword + "' spec='" + num2 + "' leave='" + text + "' old='" + num3 + "' />";
				Send(hashtable, "joinRoom", activeRoomId, message);
				changingRoom = true;
			}
			else
			{
				DebugMessage("SmartFoxError: requested room to join does not exist!");
			}
		}

		public void LeaveRoom(int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message = "<rm id='" + roomId + "' />";
				Send(hashtable, "leaveRoom", roomId, message);
			}
		}

		internal void ClearRoomList()
		{
			roomList.Clear();
		}

		public void SetRoomVariables(ArrayList varList)
		{
			SetRoomVariables(varList, -1, setOwnership: true);
		}

		public void SetRoomVariables(ArrayList varList, int roomId)
		{
			SetRoomVariables(varList, roomId, setOwnership: true);
		}

		public void SetRoomVariables(ArrayList varList, int roomId, bool setOwnership)
		{
			if (!CheckRoomList() || !CheckJoin())
			{
				return;
			}
			if (roomId == -1)
			{
				roomId = activeRoomId;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string text = ((!setOwnership) ? "<vars so='0'>" : "<vars>");
			foreach (RoomVariable var in varList)
			{
				text += GetXmlRoomVariable(var);
			}
			text += "</vars>";
			Send(hashtable, "setRvars", roomId, text);
		}

		private string GetXmlRoomVariable(RoomVariable rVar)
		{
			string name = rVar.GetName();
			object obj = rVar.GetValue();
			string text = (rVar.IsPrivate() ? "1" : "0");
			string text2 = (rVar.IsPersistent() ? "1" : "0");
			string text3 = null;
			if (obj.GetType() == typeof(bool))
			{
				text3 = "b";
				obj = (((bool)obj) ? "1" : "0");
			}
			else if (obj.GetType() == typeof(int))
			{
				text3 = "n";
			}
			else if (obj.GetType() == typeof(string))
			{
				text3 = "s";
			}
			else if ((obj == null && obj.GetType() == typeof(object)) || obj.GetType() == null)
			{
				text3 = "x";
				obj = "";
			}
			if (text3 != null)
			{
				return string.Concat("<var n='", name, "' t='", text3, "' pr='", text, "' pe='", text2, "'><![CDATA[", obj, "]]></var>");
			}
			return "";
		}

		public void SendPublicMessage(string message)
		{
			SendPublicMessage(message, -1);
		}

		public void SendPublicMessage(string message, int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message2 = "<txt><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";
				Send(hashtable, "pubMsg", roomId, message2);
			}
		}

		public void SendPrivateMessage(string message, int recipientId)
		{
			SendPrivateMessage(message, recipientId, -1);
		}

		public void SendPrivateMessage(string message, int recipientId, int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message2 = "<txt rcp='" + recipientId + "'><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";
				Send(hashtable, "prvMsg", roomId, message2);
			}
		}

		public void SendModeratorMessage(string message, string type)
		{
			SendModeratorMessage(message, type, -1);
		}

		public void SendModeratorMessage(string message, string type, int id)
		{
			if (CheckRoomList() && CheckJoin())
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message2 = "<txt t='" + type + "' id='" + id + "'><![CDATA[" + Entities.EncodeEntities(message) + "]]></txt>";
				Send(hashtable, "modMsg", activeRoomId, message2);
			}
		}

		public void SendObject(SFSObject obj)
		{
			SendObject(obj, -1);
		}

		public void SendObject(SFSObject obj, int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				string message = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(obj) + "]]>";
				Send(hashtable, "asObj", roomId, message);
			}
		}

		public void SendObjectToGroup(SFSObject obj, ArrayList userList)
		{
			SendObjectToGroup(obj, userList, -1);
		}

		public void SendObjectToGroup(SFSObject obj, ArrayList userList, int roomId)
		{
			if (!CheckRoomList() || !CheckJoin())
			{
				return;
			}
			if (roomId == -1)
			{
				roomId = activeRoomId;
			}
			string text = "";
			foreach (int user in userList)
			{
				text = text + user + ",";
			}
			text = text.Substring(0, text.Length - 1);
			obj.Put("_$$_", text);
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string message = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(obj) + "]]>";
			Send(hashtable, "asObjG", roomId, message);
		}

		public void SendXtMessage(string xtName, string cmd, ICollection paramObj)
		{
			SendXtMessage(xtName, cmd, paramObj, "xml", -1);
		}

		public void SendXtMessage(string xtName, string cmd, ICollection paramObj, string type)
		{
			SendXtMessage(xtName, cmd, paramObj, type, -1);
		}

		public void SendXtMessage(string xtName, string cmd, ICollection paramObj, string type, int roomId)
		{
			if (!CheckRoomList())
			{
				return;
			}
			if (roomId == -1)
			{
				roomId = activeRoomId;
			}
			if (paramObj != null && (type == "json" || type == "xml") && paramObj.GetType() != typeof(Hashtable))
			{
				DebugMessage("ERROR sending JSON or XML Xt message. Parameter object is not a Hashtable. Message has NOT been sent.");
				return;
			}
			if (paramObj != null && type == "str" && paramObj.GetType() != typeof(ArrayList))
			{
				DebugMessage("ERROR sending STR Xt message. Parameter object is not an ArrayList. Message has NOT been sent.");
				return;
			}
			if (paramObj == null && (type == "json" || type == "xml"))
			{
				paramObj = new Hashtable();
			}
			if (paramObj == null && type == "str")
			{
				paramObj = new ArrayList();
			}
			switch (type)
			{
			case "xml":
			{
				Hashtable hashtable3 = new Hashtable();
				hashtable3.Add("t", "xt");
				SFSObject sFSObject = new SFSObject();
				sFSObject.Put("name", xtName);
				sFSObject.Put("cmd", cmd);
				sFSObject.PutDictionary("param", (Hashtable)paramObj);
				string message = "<![CDATA[" + SFSObjectSerializer.GetInstance().Serialize(sFSObject) + "]]>";
				Send(hashtable3, "xtReq", roomId, message);
				break;
			}
			case "str":
			{
				string text = MSG_STR + "xt" + MSG_STR + xtName + MSG_STR + cmd + MSG_STR + roomId + MSG_STR;
				foreach (object item in paramObj)
				{
					text = text + item.ToString() + MSG_STR;
				}
				SendString(text);
				break;
			}
			case "json":
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("x", xtName);
				hashtable.Add("c", cmd);
				hashtable.Add("r", roomId);
				hashtable.Add("p", paramObj);
				Hashtable hashtable2 = new Hashtable();
				hashtable2.Add("t", "xt");
				hashtable2.Add("b", hashtable);
				string jsMessage = JsonMapper.ToJson(hashtable2);
				SendJson(jsMessage);
				break;
			}
			}
		}

		public void SetUserVariables(Hashtable varObj)
		{
			SetUserVariables(varObj, -1);
		}

		public void SetUserVariables(Hashtable varObj, int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				Room activeRoom = GetActiveRoom();
				User user = activeRoom.GetUser(myUserId);
				user.SetVariables(varObj);
				string xmlUserVariable = GetXmlUserVariable(varObj);
				Send(hashtable, "setUvars", roomId, xmlUserVariable);
			}
		}

		private string GetXmlUserVariable(Hashtable uVars)
		{
			string text = "<vars>";
			foreach (string key in uVars.Keys)
			{
				object obj = uVars[key];
				string text3 = null;
				if (obj.GetType() == typeof(bool))
				{
					text3 = "b";
					obj = (((bool)obj) ? "1" : "0");
				}
				else if (obj.GetType() == typeof(int))
				{
					text3 = "n";
				}
				else if (obj.GetType() == typeof(string))
				{
					text3 = "s";
				}
				else if ((obj == null && obj.GetType() == typeof(object)) || obj.GetType() == null)
				{
					text3 = "x";
					obj = "";
				}
				if (text3 != null)
				{
					object obj2 = text;
					text = string.Concat(obj2, "<var n='", key, "' t='", text3, "'><![CDATA[", obj, "]]></var>");
				}
			}
			return text + "</vars>";
		}

		public void GetRandomKey()
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "rndK", -1, "");
		}

		public string GetVersion()
		{
			return majVersion + "." + minVersion + "." + subVersion;
		}

		public void RoundTripBench()
		{
			benchStartTime = DateTime.Now;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			Send(hashtable, "roundTrip", activeRoomId, "");
		}

		public void SwitchSpectator()
		{
			SwitchSpectator(-1);
		}

		public void SwitchSpectator(int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				Send(hashtable, "swSpec", roomId, "");
			}
		}

		public void SwitchPlayer()
		{
			SwitchPlayer(-1);
		}

		public void SwitchPlayer(int roomId)
		{
			if (CheckRoomList() && CheckJoin())
			{
				if (roomId == -1)
				{
					roomId = activeRoomId;
				}
				Hashtable hashtable = new Hashtable();
				hashtable.Add("t", "sys");
				Send(hashtable, "swPl", roomId, "");
			}
		}

		public string GetUploadPath()
		{
			return "http://" + ipAddress + ":" + httpPort + "/default/uploads/";
		}

		public void UploadFile(string filePath)
		{
			UploadFile(filePath, -1, "", -1);
		}

		public void UploadFile(string filePath, int id)
		{
			UploadFile(filePath, id, "", -1);
		}

		public void UploadFile(string filePath, int id, string nick)
		{
			UploadFile(filePath, id, nick, -1);
		}

		public void UploadFile(string filePath, int id, string nick, int port)
		{
			if (id == -1)
			{
				id = myUserId;
			}
			if (nick == "")
			{
				nick = myUserName;
			}
			if (port == -1)
			{
				port = httpPort;
			}
			WebClient webClient = new WebClient();
			webClient.UploadFile("http://" + ipAddress + ":" + port + "/default/Upload.py?id=" + id + "&nick=" + nick, "POST", filePath);
			DebugMessage("[UPLOAD]: http://" + ipAddress + ":" + port + "/default/Upload.py?id=" + id + "&nick=" + nick);
		}

		internal DateTime GetBenchStartTime()
		{
			return benchStartTime;
		}

		internal void __Logout()
		{
			Initialize(isLogOut: true);
		}

		private void Initialize()
		{
			Initialize(isLogOut: false);
		}

		private void Initialize(bool isLogOut)
		{
			changingRoom = false;
			amIModerator = false;
			playerId = -1;
			activeRoomId = -1;
			myUserId = -1;
			myUserName = "";
			roomList.Clear();
			buddyList.Clear();
			myBuddyVars.Clear();
			messageBuffer = "";
			if (!isLogOut)
			{
				connected = false;
				isHttpMode = false;
			}
		}

		private void SetupMessageHandlers()
		{
			sysHandler = new SysHandler(this);
			extHandler = new ExtHandler(this);
			AddMessageHandler("sys", sysHandler);
			AddMessageHandler("xt", extHandler);
		}

		private void AddMessageHandler(string key, IMessageHandler handler)
		{
			if (messageHandlers[key] == null)
			{
				messageHandlers[key] = handler;
			}
			else
			{
				DebugMessage("Warning, message handler called: " + key + " already exist!");
			}
		}

		private void HandleMessage(string msg)
		{
			if (msg != "ok")
			{
				DebugMessage("[ RECEIVED ]: " + msg + ", (len: " + msg.Length + ")");
			}
			string text = msg.Substring(0, 1);
			if (text == MSG_XML)
			{
				XmlReceived(msg);
			}
			else if (text == MSG_STR)
			{
				StrReceived(msg);
			}
			else if (text == MSG_JSON)
			{
				JsonReceived(msg);
			}
		}

		private void XmlReceived(string msg)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(msg);
				string @string = XmlUtil.GetString(xmlDocument, "/msg/@t");
				((IMessageHandler)messageHandlers[@string])?.HandleMessage(xmlDocument, "xml");
			}
			catch (NullReferenceException ex)
			{
				DebugMessage("XML Handler null reference exception " + ex.ToString());
			}
		}

		private void JsonReceived(string msg)
		{
			JsonData jsonData = JsonMapper.ToObject(msg);
			string key = (string)jsonData["t"];
			((IMessageHandler)messageHandlers[key])?.HandleMessage(jsonData["b"], "json");
		}

		private void StrReceived(string msg)
		{
			string[] array = msg.Substring(1, msg.Length - 2).Split(MSG_STR.ToCharArray());
			string key = array[0];
			((IMessageHandler)messageHandlers[key])?.HandleMessage(string.Join(MSG_STR, array, 1, array.Length - 1), "str");
		}

		private void Send(Hashtable header, string action, int fromRoom, string message)
		{
			string text = MakeXmlHeader(header);
			object obj = text;
			text = string.Concat(obj, "<body action='", action, "' r='", fromRoom, "'>", message, "</body>", CloseXmlHeader());
			DebugMessage("[Sending]: " + text + "\n");
			if (isHttpMode)
			{
				httpConnection.Send(text);
			}
			else
			{
				WriteToSocket(text);
			}
		}

		private string MakeXmlHeader(Hashtable headerObj)
		{
			string text = "<msg";
			IDictionaryEnumerator enumerator = headerObj.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object obj = text;
				text = string.Concat(obj, " ", enumerator.Key, "='", enumerator.Value, "'");
			}
			return text + ">";
		}

		private string CloseXmlHeader()
		{
			return "</msg>";
		}

		internal void SendString(string strMessage)
		{
			DebugMessage("[Sending - STR]: " + strMessage + "\n");
			if (isHttpMode)
			{
				httpConnection.Send(strMessage);
			}
			else
			{
				WriteToSocket(strMessage);
			}
		}

		internal void SendJson(string jsMessage)
		{
			DebugMessage("[Sending - JSON]: " + jsMessage + "\n");
			if (isHttpMode)
			{
				httpConnection.Send(jsMessage);
			}
			else
			{
				WriteToSocket(jsMessage);
			}
		}

		private void HandleSocketConnection(object sender, EventArgs e)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("t", "sys");
			string message = "<ver v='" + majVersion + minVersion + subVersion + "' />";
			Send(hashtable, "verChk", 0, message);
		}

		private void HandleSocketDisconnection()
		{
			Initialize();
			SFSEvent evt = new SFSEvent("OnConnectionLost", new Hashtable());
			DispatchEvent(evt);
		}

		private void HandleIOError(string originalError)
		{
			TryBlueBoxConnection(originalError);
		}

		private void WriteToSocket(string msg)
		{
			if (!connected)
			{
				DebugMessage("WriteToSocket: Not Connected.");
				return;
			}
			try
			{
				StreamWriter streamWriter = new StreamWriter(networkStream);
				streamWriter.Write(msg + '\0');
				streamWriter.Flush();
			}
			catch (NullReferenceException ex)
			{
				HandleIOError(ex.Message);
			}
			catch (SocketException ex2)
			{
				HandleIOError(ex2.Message);
			}
		}

		private void HandleSocketData()
		{
			if (!connected)
			{
			}
			int num = 0;
			try
			{
				while (true)
				{
					bool flag = true;
					num = networkStream.Read(byteBuffer, 0, 4096);
					if (num < 1)
					{
						break;
					}
					messageBuffer += Encoding.ASCII.GetString(byteBuffer, 0, num);
					Regex regex = new Regex("\0");
					if (regex.Matches(messageBuffer).Count == 0)
					{
						continue;
					}
					char[] array = new char[1];
					char[] separator = array;
					string[] array2 = messageBuffer.Split(separator);
					bool flag2 = false;
					for (int i = 0; i < array2.Length; i++)
					{
						if (i == array2.Length - 1)
						{
							if (array2[i].Length == 0)
							{
								messageBuffer = "";
								break;
							}
							messageBuffer = array2[i];
							flag2 = true;
						}
						if (!flag2)
						{
							HandleMessage(array2[i]);
						}
					}
				}
				DebugMessage("Disconnect due to lost socket connection");
				Disconnect();
			}
			catch (Exception ex)
			{
				DebugMessage("Disconnect due to: " + ex.Message);
				Disconnect();
			}
		}

		private void TryBlueBoxConnection(string originalError)
		{
			if (!connected)
			{
				if (smartConnect)
				{
					DebugMessage("Socket connection failed. Trying BlueBox");
					isHttpMode = true;
					string ipAddr = ((blueBoxIpAddress != null) ? blueBoxIpAddress : ipAddress);
					int num = ((blueBoxPort != 0) ? blueBoxPort : httpPort);
					httpConnection.Connect(ipAddr, num);
				}
				else
				{
					DispatchConnectionError(originalError);
				}
			}
			else
			{
				SFSEvent evt = new SFSEvent("OnConnectionLost", new Hashtable());
				DispatchEvent(evt);
				DebugMessage("[WARN] Connection error: " + originalError);
			}
		}

		private void StartHttpPollThread()
		{
			if (thrHttpPoll == null)
			{
				ThreadStart @object = delegate
				{
					HttpPoll();
				};
				thrHttpPoll = new Thread(@object.Invoke);
				thrHttpPoll.IsBackground = true;
				thrHttpPoll.Start();
			}
		}

		private void HttpPoll()
		{
			while (true)
			{
				httpConnection.Send("poll");
				Thread.Sleep(_httpPollSpeed);
			}
		}

		private void HandleHttpData(HttpEvent evt)
		{
			string text = (string)evt.GetParameter("data");
			string[] array = text.ToString().Split('\n');
			if (!(array[0] != ""))
			{
				return;
			}
			for (int i = 0; i < array.Length - 1; i++)
			{
				string text2 = array[i];
				if (text2.Length > 0)
				{
					HandleMessage(text2);
				}
			}
		}

		private void HandleHttpConnect(HttpEvent evt)
		{
			StartHttpPollThread();
			HandleSocketConnection(null, null);
			connected = true;
			httpConnection.Send("poll");
		}

		private void HandleHttpClose(HttpEvent evt)
		{
			if (thrHttpPoll != null && thrHttpPoll.IsAlive)
			{
				thrHttpPoll.Abort();
			}
			Initialize();
			SFSEvent evt2 = new SFSEvent("OnConnectionLost", new Hashtable());
			DispatchEvent(evt2);
		}

		private void HandleHttpError(HttpEvent evt)
		{
			if (!connected)
			{
				DebugMessage("HttpError: " + ((Exception)evt.GetParameter("exception")).Message);
				DispatchConnectionError(((Exception)evt.GetParameter("exception")).Message);
			}
		}

		private void DispatchConnectionError(string errorMessage)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("success", false);
			hashtable.Add("error", "I/O Error: " + errorMessage);
			SFSEvent evt = new SFSEvent("OnConnection", hashtable);
			DispatchEvent(evt);
		}

		internal void DispatchEvent(SFSEvent evt)
		{
			if (runInQueueMode)
			{
				_EnqueueEvent(evt);
			}
			else
			{
				_DispatchEvent(evt);
			}
		}

		internal void _EnqueueEvent(SFSEvent evt)
		{
			lock (sfsQueuedEventsLocker)
			{
				sfsQueuedEvents.Enqueue(evt);
			}
		}

		internal void _DispatchEvent(SFSEvent evt)
		{
			switch (evt.GetType())
			{
			case "OnAdminMessage":
				if (onAdminMessage != null)
				{
					onAdminMessage((string)evt.GetParameter("message"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onAdminMessage, but no callback is registered");
				}
				break;
			case "OnBuddyList":
				if (onBuddyList != null)
				{
					onBuddyList(((SyncArrayList)evt.GetParameter("list")).ToArrayList());
				}
				else
				{
					Console.Error.WriteLine("Trying to call onBuddyList, but no callback is registered");
				}
				break;
			case "OnBuddyListError":
				if (onBuddyListError != null)
				{
					onBuddyListError((string)evt.GetParameter("error"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onBuddyListError, but no callback is registered");
				}
				break;
			case "OnBuddyListUpdate":
				if (onBuddyListUpdate != null)
				{
					onBuddyListUpdate((Buddy)evt.GetParameter("buddy"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onBuddyListUpdate, but no callback is registered");
				}
				break;
			case "OnBuddyPermissionRequest":
				if (onBuddyPermissionRequest != null)
				{
					onBuddyPermissionRequest((string)evt.GetParameter("sender"), (string)evt.GetParameter("message"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onBuddyPermissionRequest, but no callback is registered");
				}
				break;
			case "OnBuddyRoom":
				if (onBuddyRoom != null)
				{
					onBuddyRoom((ArrayList)evt.GetParameter("idList"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onBuddyRoom, but no callback is registered");
				}
				break;
			case "OnConfigLoadFailure":
				if (onConfigLoadFailure != null)
				{
					onConfigLoadFailure((string)evt.GetParameter("message"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onConfigLoadFailure, but no callback is registered");
				}
				break;
			case "OnConfigLoadSuccess":
				if (onConfigLoadSuccess != null)
				{
					onConfigLoadSuccess();
				}
				else
				{
					Console.Error.WriteLine("Trying to call onConfigLoadSuccess, but no callback is registered");
				}
				break;
			case "OnConnection":
				if (onConnection != null)
				{
					onConnection((bool)evt.GetParameter("success"), (string)evt.GetParameter("error"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onConnection, but no callback is registered");
				}
				break;
			case "OnConnectionLost":
				if (onConnectionLost != null)
				{
					onConnectionLost();
				}
				else
				{
					Console.Error.WriteLine("Trying to call onConnectionLost, but no callback is registered");
				}
				break;
			case "OnCreateRoomError":
				if (onCreateRoomError != null)
				{
					onCreateRoomError((string)evt.GetParameter("error"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onCreateRoomError, but no callback is registered");
				}
				break;
			case "OnDebugMessage":
				if (onDebugMessage != null)
				{
					onDebugMessage((string)evt.GetParameter("message"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onDebugMessage, but no callback is registered");
				}
				break;
			case "OnExtensionResponse":
				if (onExtensionResponse != null)
				{
					onExtensionResponse(evt.GetParameter("dataObj"), (string)evt.GetParameter("type"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onExtensionResponse, but no callback is registered");
				}
				break;
			case "OnJoinRoom":
				if (onJoinRoom != null)
				{
					onJoinRoom((Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onJoinRoom, but no callback is registered");
				}
				break;
			case "OnJoinRoomError":
				if (onJoinRoomError != null)
				{
					onJoinRoomError((string)evt.GetParameter("error"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onJoinRoomError, but no callback is registered");
				}
				break;
			case "OnLogin":
				if (onLogin != null)
				{
					onLogin((bool)evt.GetParameter("success"), (string)evt.GetParameter("name"), (string)evt.GetParameter("error"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onLogin, but no callback is registered");
				}
				break;
			case "OnLogout":
				if (onLogout != null)
				{
					onLogout();
				}
				else
				{
					Console.Error.WriteLine("Trying to call onLogout, but no callback is registered");
				}
				break;
			case "OnModMessage":
				if (onModeratorMessage != null)
				{
					onModeratorMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("sender"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onModeratorMessage, but no callback is registered");
				}
				break;
			case "OnObjectReceived":
				if (onObjectReceived != null)
				{
					onObjectReceived((SFSObject)evt.GetParameter("obj"), (User)evt.GetParameter("sender"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onObjectReceived, but no callback is registered");
				}
				break;
			case "OnPrivateMessage":
				if (onPrivateMessage != null)
				{
					onPrivateMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("user"), (int)evt.GetParameter("roomId"), (int)evt.GetParameter("userId"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onPrivateMessage, but no callback is registered");
				}
				break;
			case "OnPublicMessage":
				if (onPublicMessage != null)
				{
					onPublicMessage((string)evt.GetParameter("message"), (User)evt.GetParameter("sender"), (int)evt.GetParameter("roomId"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onPublicMessage, but no callback is registered");
				}
				break;
			case "OnRandomKey":
				if (onRandomKey != null)
				{
					onRandomKey((string)evt.GetParameter("key"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRandomKey, but no callback is registered");
				}
				break;
			case "OnRoomAdded":
				if (onRoomAdded != null)
				{
					onRoomAdded((Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoomAdded, but no callback is registered");
				}
				break;
			case "OnRoomDeleted":
				if (onRoomDeleted != null)
				{
					onRoomDeleted((Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoomDeleted, but no callback is registered");
				}
				break;
			case "OnRoomLeft":
				if (onRoomLeft != null)
				{
					onRoomLeft((int)evt.GetParameter("roomId"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoomLeft, but no callback is registered");
				}
				break;
			case "OnRoomListUpdate":
				if (onRoomListUpdate != null)
				{
					onRoomListUpdate((Hashtable)evt.GetParameter("roomList"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoomListUpdate, but no callback is registered");
				}
				break;
			case "OnRoomVariablesUpdate":
				if (onRoomVariablesUpdate != null)
				{
					onRoomVariablesUpdate((Room)evt.GetParameter("room"), (Hashtable)evt.GetParameter("changedVars"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoomVariablesUpdate, but no callback is registered");
				}
				break;
			case "OnRoundTripResponse":
				if (onRoundTripResponse != null)
				{
					onRoundTripResponse((int)evt.GetParameter("elapsed"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onRoundTripResponse, but no callback is registered");
				}
				break;
			case "OnPlayerSwitched":
				if (onPlayerSwitched != null)
				{
					onPlayerSwitched((bool)evt.GetParameter("success"), (int)evt.GetParameter("newId"), (Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onPlayerSwitched, but no callback is registered");
				}
				break;
			case "OnSpectatorSwitched":
				if (onSpectatorSwitched != null)
				{
					onSpectatorSwitched((bool)evt.GetParameter("success"), (int)evt.GetParameter("newId"), (Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onSpectatorSwitched, but no callback is registered");
				}
				break;
			case "OnUserCountChange":
				if (onUserCountChange != null)
				{
					onUserCountChange((Room)evt.GetParameter("room"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onUserCountChange, but no callback is registered");
				}
				break;
			case "OnUserEnterRoom":
				if (onUserEnterRoom != null)
				{
					onUserEnterRoom((int)evt.GetParameter("roomId"), (User)evt.GetParameter("user"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onUserEnterRoom, but no callback is registered");
				}
				break;
			case "OnUserLeaveRoom":
				if (onUserLeaveRoom != null)
				{
					onUserLeaveRoom((int)evt.GetParameter("roomId"), (int)evt.GetParameter("userId"), (string)evt.GetParameter("userName"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onUserLeaveRoom, but no callback is registered");
				}
				break;
			case "OnUserVariablesUpdate":
				if (onUserVariablesUpdate != null)
				{
					onUserVariablesUpdate((User)evt.GetParameter("user"), (Hashtable)evt.GetParameter("changedVars"));
				}
				else
				{
					Console.Error.WriteLine("Trying to call onUserVariablesUpdate, but no callback is registered");
				}
				break;
			default:
				DebugMessage("Unknown event dispatched " + evt.GetType());
				break;
			}
		}

		internal void DebugMessage(string message)
		{
			if (debug)
			{
				Hashtable hashtable = new Hashtable();
				hashtable.Add("message", message);
				SFSEvent evt = new SFSEvent("OnDebugMessage", hashtable);
				DispatchEvent(evt);
			}
		}

		internal bool CheckRoomList()
		{
			bool result = true;
			if (roomList == null || roomList.Count == 0)
			{
				result = false;
				ErrorTrace("The room list is empty!\nThe client API cannot function properly until the room list is populated.\nPlease consult the documentation for more infos.");
			}
			return result;
		}

		internal bool CheckJoin()
		{
			bool result = true;
			if (activeRoomId < 0)
			{
				result = false;
				ErrorTrace("You haven't joined any rooms!\nIn order to interact with the server you should join at least one room.\nPlease consult the documentation for more infos.");
			}
			return result;
		}

		internal void ErrorTrace(string msg)
		{
			Console.WriteLine("\n****************************************************************");
			Console.WriteLine("Warning:");
			Console.WriteLine(msg);
			Console.WriteLine("****************************************************************");
			DebugMessage(msg);
		}
	}
}
