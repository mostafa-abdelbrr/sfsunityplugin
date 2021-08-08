using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace SmartFoxClientAPI.Http
{
    /**
     * <summary>Internal class for handling all HTTP based communication</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.0</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class HttpConnection
    {
        /**
         * <summary>Token used in handshaking</summary>
         */
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

        /**
         * <summary>Delegate for all HTTP callbacks</summary>
         */
        public delegate void HttpCallbackHandler(HttpEvent evt);

        // Handlers
        private HttpCallbackHandler OnHttpConnectCallback;
		private HttpCallbackHandler OnHttpCloseCallback;
        private HttpCallbackHandler OnHttpErrorCallback;
        private HttpCallbackHandler OnHttpDataCallback;

		/**
		 * <summary>
		 * HttpConnection constructor.
		 * </summary>
		 * 
		 * <param name="sfs">The smartfox client</param>
		 */
        public HttpConnection(SmartFoxClient sfs)
        {
            this.codec = new RawProtocolCodec();
            this.sfs = sfs;
        }

        /**
         * <summary>
         * Get the session id of the current connection.
         * </summary>
         * 
         * <returns>The session id</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public string GetSessionId()
        {
            return this.sessionId;
        }

        /**
         * <summary>
         * A boolean flag indicating if we are connected
         * </summary>
         * 
         * <returns><c>true</c> if the conection is open</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public bool IsConnected()
        {
            return this.connected;
        }

        /**
         * <summary><see cref="Connect(string, int)"/></summary>
         */
        public void Connect(string ipAddr)
        {
            Connect(ipAddr, 8080);
        }

        /**
         * <summary>
         * Connect to the given server address and port
         * </summary>
         * 
         * <param name="ipAddr">Address of server</param>
         * <param name="port">Port of server</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Connect(string ipAddr, int port)
        {
            this.ipAddr = ipAddr;
            this.port = port;
            this.webUrl = "http://" + this.ipAddr + ":" + this.port + "/" + servletUrl;

            Send(HANDSHAKE);

        }
		
        /**
         */ 
		public string GetWebUrl() {
			return webUrl;
		}

        /**
         * <summary>
         * Close current connection
         * </summary>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Close()
        {
            Send(DISCONNECT);
        }

        /**
         * <summary>
         * Send message to server
         * </summary>
         * 
         * <param name="message">Message to send</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void Send(string message)
        {
            if (connected || (!connected && message == HANDSHAKE) || (!connected && message == "poll"))
            {
                sfs.DebugMessage("[ Send ]: " + message + "\n");

                ThreadStart starter = delegate { HttpSendViaSockets(message); };
                Thread t = new Thread(new ThreadStart(starter));
                t.IsBackground = true;
                t.Start();
            }
        }

        private void HttpSendViaSockets(string message)
        {
            TcpClient client = null;

            try
            {
                IPAddress parsedIpAddress = IPAddress.Parse(this.ipAddr);
                client = new TcpClient();
                client.Connect(parsedIpAddress, this.port); 
            }
            catch (Exception e)
            {
                sfs.DebugMessage("Http error creting http connection: " + e.ToString());
				Hashtable parameters = new Hashtable();
				parameters.Add("exception", e);
				if ( OnHttpErrorCallback != null ) {
					OnHttpErrorCallback(new HttpEvent(HttpEvent.onHttpError, parameters));
				}
                return;
            }
            try
            {

                string data = paramName + "=" + codec.Encode(this.sessionId, message);
                Byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

                StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("POST /" + servletUrl + " HTTP/1.0\r\n");
                sb.Append("Content-Type: application/x-www-form-urlencoded; charset=utf-8\r\n");
                sb.AppendFormat("Content-Length: {0}\r\n", dataBytes.Length);
                sb.Append("\r\n");
                sb.Append(data);

                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.Write(sb.ToString() + (char)0);
                writer.Flush();

                System.Text.StringBuilder responseFromServer = new System.Text.StringBuilder();
                int ct = 0;
                byte[] receive = new byte[4096];
                while ((ct = client.GetStream().Read(receive, 0, 4096)) > 0)
                {
                    // Add the received byte message to the messageBuffer, so we can cut up that one                 
                    responseFromServer.Append(Encoding.UTF8.GetString(receive));
                    receive = new byte[4096];
                }

                // Remove http header and trim whitespaces at the end
                string[] parts = System.Text.RegularExpressions.Regex.Split(responseFromServer.ToString(), "\r\n\r\n");
                string dataPayload = System.Text.RegularExpressions.Regex.Replace(parts[1], @"\s+$", ""); 

                sfs.DebugMessage("[ Receive ]: '" + dataPayload + "'\n");

                // Data is read now - lets process it unless the length is 0 (nothing recieved)
                // Handle handshake
                if (dataPayload.Length != 0 && dataPayload.Substring(0, 1) == HANDSHAKE_TOKEN)
                {
                    if (sessionId == null)
                    {
                        sessionId = codec.Decode(dataPayload);
                        connected = true;

                        Hashtable parameters = new Hashtable();
                        parameters.Add("sessionId", this.sessionId);
                        parameters.Add("success", true);

                        if (OnHttpConnectCallback != null)
                            OnHttpConnectCallback(new HttpEvent(HttpEvent.onHttpConnect, parameters));

                    }
                    else
                    {
                        sfs.DebugMessage("**ERROR** SessionId is being rewritten");
                    }
                }
                // Handle data
                else
                {
                    // The server can send multiple messages back to us - separated by \n characters
                    // Split and handle each message
                    string[] messages = System.Text.RegularExpressions.Regex.Split(dataPayload, "\r\n");

                    for (int messageCount = 0; messageCount < messages.Length; messageCount++)
                    {
                        if (messages[messageCount].Length > 0)
                        {
                            // fire disconnection
                            if (messages[messageCount].IndexOf(CONN_LOST) == 0)
                            {
                                if (OnHttpCloseCallback != null)
                                    OnHttpCloseCallback(new HttpEvent(HttpEvent.onHttpClose, null));
                            }

                            // fire onHttpData
                            else
                            {
                                Hashtable parameters = new Hashtable();
                                parameters.Add("data", messages[messageCount]);
                                if (OnHttpDataCallback != null)
                                    OnHttpDataCallback(new HttpEvent(HttpEvent.onHttpData, parameters));
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
				sfs.DebugMessage("WARNING: Exception thrown trying to send/read data via HTTP. Exception: " + e.ToString());
				if ( OnHttpCloseCallback != null )
					OnHttpCloseCallback(new HttpEvent(HttpEvent.onHttpClose, null));
			}
            finally
            {
                try
                {
                    client.Close();
                }
                catch (Exception e)
                {
                    sfs.DebugMessage("Http error closing http connection: " + e.ToString());
                    if (OnHttpCloseCallback != null)
                        OnHttpCloseCallback(new HttpEvent(HttpEvent.onHttpClose, null));
                }
            }
        }

        /**
         * <summary>
         * Add callback methods to the given event
         * </summary>
         * 
         * <param name="evt">The event to listen to</param>
         * <param name="method">The callback handler</param>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public void AddEventListener(string evt, HttpCallbackHandler method)
        {
            switch (evt)
            {
                case HttpEvent.onHttpConnect:
                    OnHttpConnectCallback = method;
                    break;
                case HttpEvent.onHttpClose:
                    OnHttpCloseCallback = method;
                    break;
                case HttpEvent.onHttpError:
                    OnHttpErrorCallback = method;
                    break;
                case HttpEvent.onHttpData:
                    OnHttpDataCallback = method;
                    break;
            }
        }
    }
}
