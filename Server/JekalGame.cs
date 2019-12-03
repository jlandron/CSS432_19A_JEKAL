using Jekal.Objects;
using Jekal.Protocols;
using Jekal.Servers;
using Jekal.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal
{
    public class JekalGame
    {
        private const int BUFFER_SIZE = 4096;
        private readonly object _jekalLock = new object();

        public GameManager Games { get; set; }
        public PlayerManager Players { get; set; }

        // Network Settings
        private TcpServer loginServer;
        private int _loginPort;
        private TcpServer chatServer;
        private int _chatPort;
        private GameServer gameServer;
        private int _gamePort;

        // Game Settings
        private int _maxPlayers;
        private int _maxTeams;
        private int _maxPlayersPerTeam;
        private int _maxGames;
        private int _gameTime;  // in minutes

        private List<Player> _closedChatConnections = null;
        private List<Player> _closedGameConnections = null;

        private CancellationTokenSource source;

        public NameValueCollection Settings { get; }

        public JekalGame(NameValueCollection settings)
        {
            Settings = settings;
        }

        public void Initialize()
        {
            Console.WriteLine("JEKALGAME: Initializing Servers...");
            
            source = new CancellationTokenSource();

            // Get Game settings
            _loginPort = Convert.ToInt32(Settings["loginServerPort"]);
            _chatPort = Convert.ToInt32(Settings["chatServerPort"]);
            _gamePort = Convert.ToInt32(Settings["gameServerPort"]);
            _maxTeams = Convert.ToInt32(Settings["maxTeamsPerGame"]);
            _maxPlayersPerTeam = Convert.ToInt32(Settings["maxPlayersPerGame"]);
            _maxGames = Convert.ToInt32(Settings["maxGameCount"]);
            _gameTime = Convert.ToInt32(Settings["gameTime"]);
            _maxPlayers = _maxPlayersPerTeam * _maxTeams * _maxGames;

            // Init IP Address
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            IPAddress ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            // Init Login Server
            loginServer = new TcpServer();
            loginServer.Initialize(ipAddress, _loginPort, "LOGINSERVER");
            loginServer.ConnectionHandler = LoginConnection;
            loginServer.ShutDownEvent += LoginShutDown;

            //chatServer = new ChatServer(this);
            chatServer = new TcpServer();
            chatServer.Initialize(ipAddress, _chatPort, "CHATSERVER");
            chatServer.ConnectionHandler = ChatConnection;
            chatServer.ShutDownEvent += ChatShutDown;

            // Lists to handle players with connection errors or disconnects
            _closedChatConnections = new List<Player>();
            _closedGameConnections = new List<Player>();

            gameServer = new GameServer(this);
            Players = new PlayerManager();
            Games = new GameManager(this);
        }

        #region Jekal Game Methods
        public void StartGame()
        {
            Console.WriteLine("JEKALGAME: Starting Jekal servers...");
            var token = source.Token;
            var loginTask = loginServer.StartServer(token);
            var chatTask = chatServer.StartServer(token);
            var gameTask = gameServer.StartServer(token);

            Task.WaitAll(loginTask, chatTask, gameTask);

            Console.WriteLine("JEKALGAME: Stopping games in progress...");

            foreach (var g in Games.GetAllGames())
            {
                g.StopGame();
            }

            Console.WriteLine("JEKALGAME: Cleaning up player list...");
            foreach (var p in Players.GetAllPlayers())
            {
                p.Value.CloseChat(null);
                p.Value.CloseGame();
            }

            Console.WriteLine("JEKALGAME: Game Stopped.");
        }

        public void StopGame()
        {
            source.Cancel();
        }
        #endregion

        #region LoginServer Methods
        private Task LoginConnection(TcpClient loginConnection)
        {
            Console.WriteLine("LOGINSERVER: Incoming Connection");

            var login = new LoginMessage();
            byte[] inBuffer;
            inBuffer = new byte[BUFFER_SIZE];
            NetworkStream netStream = null;

            try
            {
                // Read incoming data
                netStream = loginConnection.GetStream();
                do
                {
                    int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                    byte[] temp = new byte[bytesRead];
                    Array.Copy(inBuffer, temp, bytesRead);
                    login.Buffer.Write(temp);
                }
                while (netStream.DataAvailable);

                // Parse Login Message
                if (login.Parse() && (login.MessageType == LoginMessage.Messages.LOGIN))
                {
                    lock (_jekalLock)
                    {
                        login.Buffer.Clear();
                        if (Players.PlayerExists(login.Player))
                        {
                            // Player name exists already
                            Console.WriteLine($"LOGINSERVER: REJECT {login.Player} - Dupe.");
                            login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                            login.Buffer.Write("User name in use.");
                        }
                        else if (login.Player.Length < 3 || login.Player.Contains(" "))
                        {
                            // Invalid Player Name
                            Console.WriteLine($"LOGINSERVER: REJECT {login.Player} - Name too short or spaces in name.");
                            login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                            login.Buffer.Write("Invalid name format, min 3 characters with no spaces");
                        }
                        else if (Players.GetAllPlayers().Count == _maxPlayers)
                        {
                            // Out of player slots
                            Console.WriteLine($"LOGINSERVER: REJECT {login.Player} - Server full.");
                            login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                            login.Buffer.Write("Server full, please try again later.");
                        }
                        else
                        {
                            // Log in
                            int sessionID = Players.CreateSession(login.Player).SessionID;
                            Console.WriteLine($"LOGINSERVER: AUTH {login.Player}; SESSION: {sessionID}");

                            // Player Validated, create an auth message and a session
                            login.Buffer.Write((int)LoginMessage.Messages.AUTH);
                            login.Buffer.Write(_chatPort);
                            login.Buffer.Write(_gamePort);
                            login.Buffer.Write(sessionID);

                        }
                    } // End lock

                    netStream.Write(login.Buffer.ToArray(), 0, login.Buffer.Count());
                }
                else
                {
                    Console.WriteLine("LOGINSERVER: Invalid login message received. Closing connection.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LOGINSERVER: Error processing login - {ex.Message}");
            }
            finally
            {
                // Close connection
                netStream?.Close();
                loginConnection.Close();
            }
            return Task.FromResult(0);
        }

        public void LoginShutDown(bool error, string msg = "")
        {
            if (error)
            {
                Console.WriteLine($"LOGINSERVER: Error - {msg}");
            }
        }
        #endregion

        #region ChatServer Methods
        private Task ChatConnection(TcpClient chatConnection)
        {
            Console.WriteLine("CHATSERVER: Incoming Connection");

            NetworkStream netStream = null;
            var chatMsg = new ChatMessage();
            byte[] inBuffer;
            inBuffer = new byte[BUFFER_SIZE];

            try
            {
                // Read incoming data
                 netStream = chatConnection.GetStream();
                do
                {
                    int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                    byte[] temp = new byte[bytesRead];
                    Array.Copy(inBuffer, temp, bytesRead);
                    chatMsg.Buffer.Write(temp);
                }
                while (netStream.DataAvailable);

                lock (_jekalLock)
                {
                    // Parse Chat Message
                    if (chatMsg.Parse() && (chatMsg.MessageType == ChatMessage.Messages.JOIN))
                    {
                        var playerName = chatMsg.Source;
                        var sessionId = chatMsg.SourceId;

                        // Clear for reuse
                        chatMsg.Buffer.Clear();

                        if (!Players.ValidateSession(playerName, sessionId))
                        {
                            Console.WriteLine($"CHATSERVER: Reject {playerName} - No Session");
                            chatMsg.Buffer.Write((int)ChatMessage.Messages.REJECT);
                            chatMsg.Buffer.Write("No session ID.");
                            netStream.Write(chatMsg.Buffer.ToArray(), 0, chatMsg.Buffer.Count());
                            netStream.Close();
                            chatConnection.Close();
                        }
                        else
                        {
                            Console.WriteLine($"CHATSERVER: JOIN {playerName}; SESSION: {sessionId}");
                            var player = Players.GetPlayer(playerName);
                            player.AssignChatConnection(chatConnection, new AsyncCallback(ChatMessageHandler));
                            SendSystemMessage($"[{playerName}] has joined the chat.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("CHATSERVER: Expecting chat JOIN message. Closing connection.");
                        netStream.Close();
                        chatConnection.Close();
                    }
                } // End lock
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CHATSERVER: Error processing chat JOIN - {ex.Message}");
                netStream?.Close();
                chatConnection?.Close();
            }

            return Task.FromResult(0);
        }

        private void ChatShutDown(bool error, string msg = "")
        {
            Console.WriteLine($"CHATSERVER: Cleanup...");
            if (error)
            {
                Console.WriteLine($"CHATSERVER: Error - {msg}");
            }

            // Send chat close messages
            Console.WriteLine($"CHATSERVER: Sending CLOSE messages...");
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.CLOSE);
            foreach (var p in Players.GetAllPlayers())
            {
                if (p.Value.ChatEnabled)
                {
                    p.Value.CloseChat(byteBuffer);
                }
            }
            byteBuffer.Dispose();
            Console.WriteLine($"CHATSERVER: Cleanup Done.");
        }

        private void ChatMessageHandler(IAsyncResult ar)
        {
            Player player = (Player)ar.AsyncState;

            // Handle one message at a time
            lock (_jekalLock)
            {
                try
                {
                    int length = 0;
                    length = player.EndReadChat(ar);

                    var chatMsg = new ChatMessage();
                    byte[] temp = new byte[length];

                    Array.Copy(player.GetChatBuffer(), temp, length);
                    chatMsg.Buffer.Write(temp);

                    // Continue reading if more data
                    while (player.ChatHasData())
                    {
                        player.BeginReadChat(new AsyncCallback(ChatMessageHandler));
                    }

                    if (!chatMsg.Parse())
                    {
                        // Invalid Message, drop it
                        return;
                    }

                    // Handle messages from client
                    switch (chatMsg.MessageType)
                    {
                        case ChatMessage.Messages.LEAVE:
                            PlayerLeaving(chatMsg);
                            break;
                        case ChatMessage.Messages.MSG:
                            StandardMessage(chatMsg);
                            break;
                        case ChatMessage.Messages.PMSG:
                            PrivateMessage(chatMsg);
                            break;
                        case ChatMessage.Messages.TMSG:
                            TeamMessage(chatMsg);
                            break;
                        default:
                            // Drop, invalid message
                            return;
                    }

                    player.BeginReadChat(new AsyncCallback(ChatMessageHandler));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CHATSERVER ERROR: {ex.Message}");
                    CloseChatConnection(player);
                }

                foreach (var p in _closedChatConnections)
                {
                    Players.RemovePlayer(p);
                }

                _closedChatConnections.Clear();
            } // End lock
        }

        private void SendSystemMessage(string message)
        {
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.SYSTEM);
            byteBuffer.Write(message);

            foreach (var p in Players.GetAllPlayers())
            {
                if (!p.Value.ChatEnabled)
                {
                    // Chat socket closed.
                    continue;
                }

                // Send message
                if (!p.Value.SendChatMessage(byteBuffer))
                {
                    CloseChatConnection(p.Value);
                }
            }

            byteBuffer.Dispose();
        }

        private void TeamMessage(ChatMessage chatMessage)
        {
            var player = Players.GetPlayer(chatMessage.Source);
            var team = Games.GetGame(player.GameID).GetTeam(player.TeamID);
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.TMSG);
            byteBuffer.Write(player.Name);
            byteBuffer.Write(player.SessionID);
            byteBuffer.Write(chatMessage.Message);

            foreach (var p in team.GetPlayers())
            {
                if (!p.ChatEnabled)
                {
                    // Chat socket closed
                    continue;
                }

                // Send Team Message
                if (!p.SendChatMessage(byteBuffer))
                {
                    CloseChatConnection(player);
                }
            }

            byteBuffer.Dispose();
        }

        private void PlayerLeaving(ChatMessage chatMessage)
        {
            var player = Players.GetPlayer(chatMessage.Source);
            CloseChatConnection(player);
        }

        private void StandardMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.MSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(chatMessage.Message);

            foreach (var p in Players.GetAllPlayers())
            {
                if (!p.Value.ChatEnabled)
                {
                    continue;
                }

                if (!p.Value.SendChatMessage(byteBuffer))
                {
                    CloseChatConnection(p.Value);
                }
            }
            byteBuffer.Dispose();
        }

        private void PrivateMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.PMSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(chatMessage.Destination);
            byteBuffer.Write(chatMessage.Message);

            // TODO: Check for player online
            // TODO: Send source player PMSG as well

            var player = Players.GetPlayer(chatMessage.Destination);

            if (!player.SendChatMessage(byteBuffer))
            {
                CloseChatConnection(player);
            }

            byteBuffer.Dispose();
        }

        public void CloseChatConnection(Player player)
        {
            _closedChatConnections.Add(player);
        }
        #endregion
    }
}
