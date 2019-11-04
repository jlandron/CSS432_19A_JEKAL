using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient {

    public class PlayerUpdater : MonoBehaviour {
        // Update is called once per frame
        void FixedUpdate( ) {
            //this client will only send its own position data
            if( this.gameObject.tag == "Player" ) {
                SendPlayerInformation( );
            }
        }
        private void SendPlayerInformation( ) {
            ByteBuffer buffer = new ByteBuffer( );
            //player location
            buffer.Write( transform.position.x );
            buffer.Write( transform.position.y );
            buffer.Write( transform.position.z );

            buffer.Write( transform.rotation.x );
            buffer.Write( transform.rotation.y );
            buffer.Write( transform.rotation.z );
            buffer.Write( transform.rotation.w );

            DataSender.SendTransformMessage( buffer.ToArray( ) );
        }
        public void UpdatePlayerTransform( Vector3 position, Quaternion rotation ) {
            this.gameObject.transform.position = position;
            Debug.Log( position.ToString( ) );
            this.gameObject.transform.rotation = rotation;
        }
    }
}
