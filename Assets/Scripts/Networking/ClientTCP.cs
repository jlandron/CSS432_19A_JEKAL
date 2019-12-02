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
            dataToSend = new ConcurrentQueue<byte[]>();
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
            //Debug.Log("Client " + Type + " started");
        }

        public bool QueueIsEmpty()
        {
            return dataToSend.IsEmpty;
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
            Debug.Log(Type + " Connected to server");
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
                Debug.Log("Client: " + Type + " recieved callback and sending to handle data");
                _myStream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                _clientSocket.Close();
                Destroy(this.gameObject);
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
