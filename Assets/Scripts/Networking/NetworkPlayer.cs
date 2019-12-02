﻿using Common.Protocols;
using System.Collections;
using UnityEngine;

namespace NetworkGame.Client
{

    public class NetworkPlayer : MonoBehaviour
    {

        [Header("Player Properties")]
        public int playerID;
        public bool isLocalPlayer;

        [Header("Player Movement Properties")]
        public bool canSendNetworkMovement;
        public float speed;
        public float networkSendRate = 10;
        public float timeBetweenMovementStart;
        public float timeBetweenMovementEnd;

        [Header("Lerping Properties")]
        public bool isLerpingPosition;
        public bool isLerpingRotation;
        public Vector3 realPosition;
        public Quaternion realRotation;
        public Vector3 lastRealPosition;
        public Quaternion lastRealRotation;
        public float timeStartedLerping;
        public float timeToLerp;


        [Header("Team information")]
        [SerializeField]
        private int team;

        [SerializeField]
        MeshRenderer[] meshRenderers;
        [SerializeField]
        Material[] materials;

        public int Team { get => team; set => team = value; }

        private void Start()
        {

            if (isLocalPlayer)
            {
                canSendNetworkMovement = false;
            }
            else
            {

                isLerpingPosition = false;
                isLerpingRotation = false;

                realPosition = transform.position;
                realRotation = transform.rotation;
            }
        }

        private void Update()
        {
            
            //process networtk stuff
            if (isLocalPlayer)
            {
                if (NetworkManager.Instance.gameClientTCP != null)
                {
                    UpdatePlayerMovement();
                }
            }

            foreach (MeshRenderer item in meshRenderers)
            {
                item.material = materials[Team % materials.Length];
            }

            if(GameManager.Instance.AllowPlayerInput && Input.GetKeyDown(KeyCode.E))
            {
                //TODO: add actual tag message with distance/collision checking
                NetworkManager.Instance.gameClientTCP.dataSender.SendTagMessage(0);
            }
        }


        private void UpdatePlayerMovement()
        {
            if (!canSendNetworkMovement)
            {
                canSendNetworkMovement = true;
                StartCoroutine(StartNetworkSendCooldown());
            }
        }

        private IEnumerator StartNetworkSendCooldown()
        {
            timeBetweenMovementStart = Time.time;
            yield return new WaitForSeconds((1 / networkSendRate));
            SendNetworkMovement();
        }

        private void SendNetworkMovement()
        {
            timeBetweenMovementEnd = Time.time;
            Debug.Log("Sending my movement");
            SendMovementMessage(playerID, transform.position, transform.rotation, (timeBetweenMovementEnd - timeBetweenMovementStart));
            canSendNetworkMovement = false;
        }

        public void SendMovementMessage(int _playerID, Vector3 _position, Quaternion _rotation, float _timeTolerp)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(_playerID);
            //player location
            buffer.Write(_position.x);
            buffer.Write(_position.y);
            buffer.Write(_position.z);

            buffer.Write(_rotation.x);
            buffer.Write(_rotation.y);
            buffer.Write(_rotation.z);
            buffer.Write(_rotation.w);
            //time information
            buffer.Write(_timeTolerp);
            NetworkManager.Instance.gameClientTCP.dataSender.SendTransformMessage(buffer.ToArray());
            buffer.Dispose();
        }

        public void ReceiveMovementMessage(Vector3 pos, Quaternion rot, float timeToLerp)
        {

            lastRealPosition = realPosition;
            lastRealRotation = realRotation;
            realPosition = pos;
            realRotation = rot;
            this.timeToLerp = timeToLerp;

            if (realPosition != transform.position)
            {
                isLerpingPosition = true;
            }

            if (realRotation.eulerAngles != transform.rotation.eulerAngles)
            {
                isLerpingRotation = true;
            }
            timeStartedLerping = Time.time;
        }


        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                NetworkLerp();
            }
        }

        private void NetworkLerp()
        {
            if (isLerpingPosition)
            {
                float lerpPercentage = (Time.time - timeStartedLerping) / timeToLerp;
                Vector3 newPos = Vector3.Lerp(lastRealPosition, realPosition, lerpPercentage);

                transform.position = newPos;
            }

            if (isLerpingRotation)
            {
                float lerpPercentage = (Time.time - timeStartedLerping) / timeToLerp;

                transform.rotation = Quaternion.Lerp(lastRealRotation, realRotation, lerpPercentage);
            }
        }
    }
}
