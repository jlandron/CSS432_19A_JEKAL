using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using UnityEngine;

namespace NetworkGame.Client
{
    public class ClientTCP : MonoBehaviour
    {
        private const int BUFFER_SIZE = 4096;
        private TcpClient _clientSocket;
        private NetworkStream _myStream;
        private byte[] _recieveBuffer;
        public ConcurrentQueue<byte[]> dataToSend;

        private string _serverIP;
        private int _serverPort;

        public bool IsConnected { get; set; }

        public DataSender dataSender;
        public DataReciever dataReciever;
        public ClientHandleData clientHandleData;
        public ClientTypes Type { get; set; }
        public void CustomAwake()
        {
            IsConnected = false;
            dataSender = new DataSender(this);
            dataReciever = new DataReciever(this);
            clientHandleData = new ClientHandleData(this);
            clientHandleData.InitPackets();
            dataToSend = new ConcurrentQueue<byte[]>();
            switch (this.Type)
            {
                case ClientTypes.LOGIN:
                    InitNetworking(NetworkManager.Instance.ServerIP, NetworkManager.Instance.LoginServerPort);
                    break;
                case ClientTypes.CHAT:
                    InitNetworking(NetworkManager.Instance.ServerIP, NetworkManager.Instance.ChatServerPort);
                    break;
                case ClientTypes.GAME:
                    InitNetworking(NetworkManager.Instance.ServerIP, NetworkManager.Instance.GameServerPort);
                    break;
            }
        }

        public void InitNetworking(string serverIP, int serverPort)
        {
            _clientSocket = new TcpClient
            {
                ReceiveBufferSize = BUFFER_SIZE,
                SendBufferSize = BUFFER_SIZE
            };
            _recieveBuffer = new byte[BUFFER_SIZE];
            _serverIP = serverIP;
            _serverPort = serverPort;
            _clientSocket.BeginConnect(serverIP, serverPort, new System.AsyncCallback(ClientConnectCallback), _clientSocket);
            //Debug.Log("Client " + Type + " started");
        }

        public bool QueueIsEmpty()
        {
            return dataToSend.IsEmpty;
        }
        public void SetReadyFlag()
        {
            switch (this.Type)
            {
                case ClientTypes.LOGIN:
                    break;
                case ClientTypes.CHAT:
                    NetworkManager.Instance.ChatIsReady = true;
                    break;
                case ClientTypes.GAME:
                    NetworkManager.Instance.GameIsReady = true;
                    break;
            }
        }

        private void ClientConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            //Debug.Log("Client " + Type + " connected");
            if (_clientSocket.Connected == false)
            {
                Debug.Log("Client Connection failed");
                IsConnected = false;
                return;
            }

            _clientSocket.NoDelay = true;
            _myStream = _clientSocket.GetStream();
            //Debug.Log(Type + " Connected to server");
            SetReadyFlag();
            _myStream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE, RecieveCallback, null);
            IsConnected = true;
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            int length = -1;
            try
            {
                length = _myStream.EndRead(ar);
                if (length <= 0)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            byte[] newBytes = new byte[length];
            Array.Copy(_recieveBuffer, newBytes, length);
            try
            {
                clientHandleData.HandleData(newBytes);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            try
            {
                //Debug.Log("Client: " + Type + " recieved callback and sending to handle data");
                _myStream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE, RecieveCallback, null);
            }

            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }

        private void Update()
        {
            byte[] result;
            if (dataToSend.TryDequeue(out result))
            {
                _myStream.Write(result, 0, result.Length);
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Close();
            }
        }
        public void MoniterConnection()
        {
            IsConnected = _clientSocket.Connected;
        }
    }
}
