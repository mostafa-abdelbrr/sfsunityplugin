using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SmartFoxClientAPI.Http
{
	public class HttpConnection
	{
		public delegate void HttpCallbackHandler(HttpEvent evt);

		public const string HANDSHAKE_TOKEN = "#";

		private const string HANDSHAKE = "connect";

		private const string DISCONNECT = "disconnect";

		private const string CONN_LOST = "ERR#01";

		private const string servletUrl = "BlueBox/HttpBox.do";

		private const string paramName = "sfsHttp";

		private string sessionId;

		private bool connected = false;

		private string ipAddr;

		private int port;

		private string webUrl;

		private IHttpProtocolCodec codec;

		private SmartFoxClient sfs;

		private HttpCallbackHandler OnHttpConnectCallback;

		private HttpCallbackHandler OnHttpCloseCallback;

		private HttpCallbackHandler OnHttpErrorCallback;

		private HttpCallbackHandler OnHttpDataCallback;

		public HttpConnection(SmartFoxClient sfs)
		{
			codec = new RawProtocolCodec();
			this.sfs = sfs;
		}

		public string GetSessionId()
		{
			return sessionId;
		}

		public bool IsConnected()
		{
			return connected;
		}

		public void Connect(string ipAddr)
		{
			Connect(ipAddr, 8080);
		}

		public void Connect(string ipAddr, int port)
		{
			this.ipAddr = ipAddr;
			this.port = port;
			webUrl = "http://" + this.ipAddr + ":" + this.port + "/BlueBox/HttpBox.do";
			Send("connect");
		}

		public void Close()
		{
			Send("disconnect");
		}

		public void Send(string message)
		{
			if (connected || (!connected && message == "connect") || (!connected && message == "poll"))
			{
				sfs.DebugMessage("[ Send ]: " + message + "\n");
				ThreadStart @object = delegate
				{
					HttpSendViaSockets(message);
				};
				Thread thread = new Thread(@object.Invoke);
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private void HttpSendViaSockets(string message)
		{
			TcpClient tcpClient = null;
			try
			{
				IPAddress address = IPAddress.Parse(ipAddr);
				tcpClient = new TcpClient();
				tcpClient.Connect(address, port);
			}
			catch (Exception ex)
			{
				sfs.DebugMessage("Http error creting http connection: " + ex.ToString());
				Hashtable hashtable = new Hashtable();
				hashtable.Add("exception", ex);
				if (OnHttpErrorCallback != null)
				{
					OnHttpErrorCallback(new HttpEvent("OnHttpError", hashtable));
				}
				return;
			}
			try
			{
				string text = "sfsHttp=" + codec.Encode(sessionId, message);
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("POST /BlueBox/HttpBox.do HTTP/1.0\r\n");
				stringBuilder.Append("Content-Type: application/x-www-form-urlencoded; charset=utf-8\r\n");
				stringBuilder.AppendFormat("Content-Length: {0}\r\n", bytes.Length);
				stringBuilder.Append("\r\n");
				stringBuilder.Append(text);
				StreamWriter streamWriter = new StreamWriter(tcpClient.GetStream());
				streamWriter.Write(stringBuilder.ToString() + '\0');
				streamWriter.Flush();
				StringBuilder stringBuilder2 = new StringBuilder();
				int num = 0;
				byte[] array = new byte[4096];
				while ((num = tcpClient.GetStream().Read(array, 0, 4096)) > 0)
				{
					stringBuilder2.Append(Encoding.UTF8.GetString(array));
					array = new byte[4096];
				}
				string[] array2 = Regex.Split(stringBuilder2.ToString(), "\r\n\r\n");
				string text2 = Regex.Replace(array2[1], "\\s+$", "");
				sfs.DebugMessage("[ Receive ]: '" + text2 + "'\n");
				if (text2.Length != 0 && text2.Substring(0, 1) == "#")
				{
					if (sessionId == null)
					{
						sessionId = codec.Decode(text2);
						connected = true;
						Hashtable hashtable = new Hashtable();
						hashtable.Add("sessionId", sessionId);
						hashtable.Add("success", true);
						if (OnHttpConnectCallback != null)
						{
							OnHttpConnectCallback(new HttpEvent("OnHttpConnect", hashtable));
						}
					}
					else
					{
						sfs.DebugMessage("**ERROR** SessionId is being rewritten");
					}
					return;
				}
				string[] array3 = Regex.Split(text2, "\r\n");
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i].Length <= 0)
					{
						continue;
					}
					if (array3[i].IndexOf("ERR#01") == 0)
					{
						if (OnHttpCloseCallback != null)
						{
							OnHttpCloseCallback(new HttpEvent("OnHttpClose", null));
						}
						continue;
					}
					Hashtable hashtable = new Hashtable();
					hashtable.Add("data", array3[i]);
					if (OnHttpDataCallback != null)
					{
						OnHttpDataCallback(new HttpEvent("OnHttpData", hashtable));
					}
				}
			}
			catch (Exception ex)
			{
				sfs.DebugMessage("WARNING: Exception thrown trying to send/read data via HTTP. Exception: " + ex.ToString());
				if (OnHttpCloseCallback != null)
				{
					OnHttpCloseCallback(new HttpEvent("OnHttpClose", null));
				}
			}
			finally
			{
				try
				{
					tcpClient.Close();
				}
				catch (Exception ex)
				{
					sfs.DebugMessage("Http error closing http connection: " + ex.ToString());
					if (OnHttpCloseCallback != null)
					{
						OnHttpCloseCallback(new HttpEvent("OnHttpClose", null));
					}
				}
			}
		}

		public void AddEventListener(string evt, HttpCallbackHandler method)
		{
			switch (evt)
			{
			case "OnHttpConnect":
				OnHttpConnectCallback = method;
				break;
			case "OnHttpClose":
				OnHttpCloseCallback = method;
				break;
			case "OnHttpError":
				OnHttpErrorCallback = method;
				break;
			case "OnHttpData":
				OnHttpDataCallback = method;
				break;
			}
		}
	}
}
