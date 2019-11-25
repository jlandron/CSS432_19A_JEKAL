using System;
using System.Net.Sockets;
using UnityEngine;

namespace GameClient
{
    public class ClientTCP : MonoBehaviour
    {
        private const int BUFFER_SIZE = 4096;
        private TcpClient _clientSocket;
        private NetworkStream _myStream;
        private byte[] _recieveBuffer;

        private string _serverIP;
        private int _serverPort;

        public bool IsConnected { get; set; } = false;

        public DataSender dataSender;
        public DataReciever dataReciever;
        public ClientHandleData clientHandleData;
        public ClientTypes Type { get; private set; }
        private void Awake()
        {
            dataSender = new DataSender(this);
            dataReciever = new DataReciever(this);
            clientHandleData = new ClientHandleData(this);
        }
        public void SetType(ClientTypes type)
        {
            Type = type;
            clientHandleData.InitPackets();
        }

        public void InitNetworking(string serverIP, int serverPort)
        {
            _clientSocket = new TcpClient
            {
                ReceiveBufferSize = BUFFER_SIZE,
                SendBufferSize = BUFFER_SIZE
            };
            _recieveBuffer = new byte[BUFFER_SIZE * 2];
            _serverIP = serverIP;
            _serverPort = serverPort;
            _clientSocket.BeginConnect(serverIP, serverPort, new System.AsyncCallback(ClientConnectCallback), _clientSocket);
            Debug.Log("Client " + Type + " started");
        }



        private void ClientConnectCallback(IAsyncResult ar)
        {
            _clientSocket.EndConnect(ar);

            if (_clientSocket.Connected == false)
            {
                Debug.Log("Client Connection failed");
                IsConnected = false;
                return;
            }

            _clientSocket.NoDelay = true;
            _myStream = _clientSocket.GetStream();
            Debug.Log("Connected to server");
            _myStream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null);
            IsConnected = true;
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            try
            {
                int length = _myStream.EndRead(ar);
                if (length <= 0)
                {
                    return;
                }
                byte[] newBytes = new byte[length];
                Array.Copy(_recieveBuffer, newBytes, length);
                clientHandleData.HandleData(newBytes);
                _myStream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendData(byte[] data)
        {
            _myStream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            IsConnected = false;
            if (_clientSocket.Connected)
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
