﻿using Common.Protocols;
using NetworkGame.Client;
using NetworkGame.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace NetworkGame
{
    public class PlayerManager : MonoBehaviour
    {

        [SerializeField]
        public Dictionary<int, Client.NetworkPlayer> ConnectedPlayers { get; private set; }
        public static PlayerManager Instance { get; private set; }
        public ConcurrentQueue<byte[]> playersToSpawn { get; private set; }
        public ConcurrentQueue<byte[]> playersSwitchingTeams { get; private set; }
        public ConcurrentQueue<byte[]> playersToRemove { get; private set; }
        public ConcurrentQueue<byte[]> playersToUpdate { get; private set; }
        public ConcurrentQueue<byte[]> playersTagged { get; private set; }

        [Header("Player")]
        [SerializeField]
        private Vector3[] startingPositions;
        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        Client.NetworkPlayer localPlayer = null;
        public int NumberConnectedPlayers { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            ConnectedPlayers = new Dictionary<int, Client.NetworkPlayer>();
            playersToSpawn = new ConcurrentQueue<byte[]>();
            playersSwitchingTeams = new ConcurrentQueue<byte[]>();
            playersToRemove = new ConcurrentQueue<byte[]>();
            playersToUpdate = new ConcurrentQueue<byte[]>();
            NumberConnectedPlayers = 0;
            Debug.Log("Player Manager initilized");
        }

        // Update is called once per frame
        void Update()
        {
            byte[] playerToSpawnData;
            while (playersToSpawn.TryDequeue(out playerToSpawnData))
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.Write(playerToSpawnData);
                int playerID = buffer.ReadInt();
                string playerName = buffer.ReadString();
                int teamNum = buffer.ReadInt();
                InstantiatePlayer(playerID, playerName , teamNum);
                buffer.Dispose();
            }
            byte[] playerToRemoveData;
            if (playersToRemove.TryDequeue(out playerToRemoveData))
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.Write(playerToRemoveData);
                HandleRemovePlayer(buffer.ToArray());
                buffer.Dispose();
            }
            byte[] updateData;
            if (playersToUpdate.TryDequeue(out updateData))
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.Write(updateData);
                UpdateGame(buffer.ToArray());
                buffer.Dispose();
            }
            byte[] playerSwitchingTeamData;
            if (playersSwitchingTeams.TryDequeue(out playerSwitchingTeamData))
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.Write(playerSwitchingTeamData);
                HandleSwitchTeam(buffer.ToArray());
                buffer.Dispose();
            }
            if (localPlayer != null)
            {
                if (localPlayer.GetComponent<FirstPersonController>().enabled != GameManager.Instance.AllowPlayerInput)
                {
                    Debug.Log("Setting player controls to: " + GameManager.Instance.AllowPlayerInput);
                    localPlayer.GetComponent<FirstPersonController>().enabled = GameManager.Instance.AllowPlayerInput;
                    localPlayer.GetComponent<CharacterController>().enabled = GameManager.Instance.AllowPlayerInput;
                }
            }
        }

        private void InstantiatePlayer(int playerID, string playerName, int teamNum)
        {
            if (playerID == NetworkManager.Instance.PlayerID)
            {
                //Debug.Log("setting local player team number");
                if (localPlayer == null)
                {
                    //Debug.Log("Finding local player");
                    GameObject go = GameObject.FindGameObjectWithTag("Player");
                    localPlayer = go.GetComponent<Client.NetworkPlayer>();
                }
                localPlayer.Team = teamNum;
            }
            else if (!ConnectedPlayers.ContainsKey(playerID))
            {
                Debug.Log("Instantiating player with ID: " + playerID);
                //add spawning locations for teams, more game information needed
                if (playerPrefab == null)
                {
                    Debug.LogError("No player prefab!");
                    return;
                }
                //make player object
                //instantiate new player in game world
                GameObject playerObject;
                if (startingPositions.Length != 0)
                {
                    playerObject = Instantiate(playerPrefab, startingPositions[playerID % startingPositions.Length], Quaternion.identity);
                }
                else
                {
                    playerObject = Instantiate(playerPrefab);
                }
                //set player atributes
                playerObject.name = playerName;
                playerObject.tag = "ExtPlayer";
                Client.NetworkPlayer networkPlayer = playerObject.GetComponent<Client.NetworkPlayer>();
                networkPlayer.playerID = playerID;
                networkPlayer.Team = teamNum;
                AddPlayerToConnectedPlayers(networkPlayer);
            }
        }

        private void HandleRemovePlayer(byte[] data)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.Write(data);
            _ = byteBuffer.ReadInt();
            int playerID = byteBuffer.ReadInt();
            RemovePlayerFromConnectedPlayers(playerID);
            byteBuffer.Dispose();
        }
        private void HandleSwitchTeam(byte[] data)
        {
            Debug.Log("player joining team");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            int playerID = buffer.ReadInt();
            string taggerName = buffer.ReadString();
            //TODO: flash taggerName on screen
            int taggerID = buffer.ReadInt();
            int oldTeamID = buffer.ReadInt();
            int teamNum = buffer.ReadInt();
            Debug.Log("player: " + playerID + " joined team " + teamNum);
            buffer.Dispose();
            if (playerID == NetworkManager.Instance.PlayerID)
            {
                localPlayer.Team = teamNum;
                //TODO: do something on localPlayers UI if needed.
            }
            else
            {
                ConnectedPlayers[playerID].Team = teamNum;
            }
        }
        internal void UpdateGame(byte[] data)
        {
            //Debug.Log("Entering update game");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt(); //message type
            UpdateGameTime(buffer.ReadInt());
            int numMessages = buffer.ReadInt();
            try
            {
                for (int i = 0; i < numMessages; i++)
                {
                    ByteBuffer byteBuffer = new ByteBuffer();
                    int playerID = buffer.ReadInt();
                    string playerName = buffer.ReadString();
                    byteBuffer.Write(buffer.ReadFloat()); //x
                    byteBuffer.Write(buffer.ReadFloat()); //y
                    byteBuffer.Write(buffer.ReadFloat()); //z
                    byteBuffer.Write(buffer.ReadFloat()); //x
                    byteBuffer.Write(buffer.ReadFloat()); //y
                    byteBuffer.Write(buffer.ReadFloat()); //z
                    byteBuffer.Write(buffer.ReadFloat()); //w
                    byteBuffer.Write(buffer.ReadFloat()); //time
                    int teamNum = buffer.ReadInt();
                    byteBuffer.Write(teamNum);
                    byteBuffer.Write(buffer.ReadInt()); //tagged
                    byteBuffer.Write(buffer.ReadInt()); //tags
                    
                    if (playerID != NetworkManager.Instance.PlayerID)
                    {
                        if (!ConnectedPlayers.ContainsKey(playerID))
                        {
                            Debug.Log("Player " + playerID + " not in game, Instantiating.");
                            InstantiatePlayer(playerID, playerName, teamNum);
                        }
                        Debug.Log("processing player " + playerID + "'s movement");
                        if (ConnectedPlayers.ContainsKey(playerID))
                        {
                            ConnectedPlayers[playerID].ReceiveStatusMessage(byteBuffer.ToArray());
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            buffer.Dispose();
        }

        private void UpdateGameTime(float v)
        {
            if(GameManager.Instance.MyGameState == GameManager.GameState.WAIT) { return; }
            ChatManager.Instance.timer.text = ("Time left: " + v);
        }



        internal void AddPlayerToConnectedPlayers(Client.NetworkPlayer networkPlayer)
        {
            if (!ConnectedPlayers.ContainsKey(networkPlayer.playerID))
            {
                ConnectedPlayers.Add(networkPlayer.playerID, networkPlayer);
                NumberConnectedPlayers++;
            }
        }

        public void RemovePlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                Destroy(ConnectedPlayers[_playerID].gameObject);
                ConnectedPlayers.Remove(_playerID);
                NumberConnectedPlayers--;
            }
        }
    }
}
