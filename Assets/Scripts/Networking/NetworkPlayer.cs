using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient {

    public class NetworkPlayer : MonoBehaviour {

        [Header( "Player Properties" )]
        public int playerID;
        public bool isLocalPlayer;

        [ Header( "Player Movement Properties" )]
        public bool canSendNetworkMovement;
        public float speed;
        public float networkSendRate = 5;
        public float timeBetweenMovementStart;
        public float timeBetweenMovementEnd;

        [Header( "Lerping Properties" )]
        public bool isLerpingPosition;
        public bool isLerpingRotation;
        public Vector3 realPosition;
        public Quaternion realRotation;
        public Vector3 lastRealPosition;
        public Quaternion lastRealRotation;
        public float timeStartedLerping;
        public float timeToLerp;

        
        private void Start( ) {
            
            if( isLocalPlayer ) {
                NetworkManager.Instance.SetLocalPlayerID( playerID );
                canSendNetworkMovement = false;
            } else {
                
                isLerpingPosition = false;
                isLerpingRotation = false;

                realPosition = transform.position;
                realRotation = transform.rotation;
            }
        }

        private void Update( ) {
            if( isLocalPlayer ) {
                UpdatePlayerMovement( );
            } else {
                return;
            }
        }

        private void UpdatePlayerMovement( ) {
            if( !canSendNetworkMovement ) {
                canSendNetworkMovement = true;
                StartCoroutine( StartNetworkSendCooldown( ) );
            }
        }

        private IEnumerator StartNetworkSendCooldown( ) {
            timeBetweenMovementStart = Time.time;
            yield return new WaitForSeconds( ( 1 / networkSendRate ) );
            SendNetworkMovement( );
        }

        private void SendNetworkMovement( ) {
            timeBetweenMovementEnd = Time.time;
            SendMovementMessage( playerID, transform.position, transform.rotation, ( timeBetweenMovementEnd - timeBetweenMovementStart ) );
            canSendNetworkMovement = false;
        }

        public void SendMovementMessage( int _playerID, Vector3 _position, Quaternion _rotation, float _timeTolerp ) {
            
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( _playerID );
            //player location
            buffer.Write( _position.x );
            buffer.Write( _position.y );
            buffer.Write( _position.z );

            buffer.Write( _rotation.x );
            buffer.Write( _rotation.y );
            buffer.Write( _rotation.z );
            buffer.Write( _rotation.w );
            //time information
            buffer.Write( _timeTolerp );

            DataSender.SendTransformMessage( buffer.ToArray( ) );

            buffer.Dispose( );
        }

        public void ReceiveMovementMessage( byte[] data) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            buffer.ReadInt( );  //message type code
            buffer.ReadInt( ); //this character id
            //read position and rotation
            Vector3 _position = new Vector3( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ) );
            Quaternion _rotation = new Quaternion( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ) );
            //read lerp time
            float _timeToLerp = buffer.ReadFloat( );
            buffer.Dispose( );

            lastRealPosition = realPosition;
            lastRealRotation = realRotation;
            realPosition = _position;
            realRotation = _rotation;
            timeToLerp = _timeToLerp;

            if( realPosition != transform.position ) {
                isLerpingPosition = true;
            }

            if( realRotation.eulerAngles != transform.rotation.eulerAngles ) {
                isLerpingRotation = true;
            }
            timeStartedLerping = Time.time;
        }


        private void FixedUpdate( ) {
            if( !isLocalPlayer ) {
                NetworkLerp( );
            }
        }

        private void NetworkLerp( ) {
            if( isLerpingPosition ) {
                float lerpPercentage = ( Time.time - timeStartedLerping ) / timeToLerp;
                Vector3 newPos = Vector3.Lerp( lastRealPosition, realPosition, lerpPercentage );

                transform.position = newPos;
            }

            if( isLerpingRotation ) {
                float lerpPercentage = ( Time.time - timeStartedLerping ) / timeToLerp;

                transform.rotation = Quaternion.Lerp( lastRealRotation, realRotation, lerpPercentage );
            }
        }
    }
}
