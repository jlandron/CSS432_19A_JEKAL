using System.Collections;
using UnityEngine;

public class RandomSFXplayer : MonoBehaviour {
    AudioSource audioSource;
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] float minTimeToWait = 10;
    [SerializeField] float maxTimeToWait = 20;
    void Start( ) {
        audioSource = GetComponent<AudioSource>( );
        StartCoroutine( RandomPlayAfterWait( ) );
    }

    IEnumerator RandomPlayAfterWait( ) {
        while( true ) {
            int randomSFX = Random.Range( 0, audioClips.Length );
            audioSource.PlayOneShot( audioClips[randomSFX] );
            yield return new WaitForSeconds( UnityEngine.Random.Range(minTimeToWait, maxTimeToWait) );
        }
    }
}
