using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public float minPauseTime = 5f; // Minimum time to pause the music
    public float maxPauseTime = 15f; // Maximum time to pause the music

    public AudioClip[] musicTracks; // Array of music tracks to play
    AudioSource musicSource; // Reference to the AudioSource component

    int lastTrackIndex = -1; // Track the last played song

    // Start is called before the first frame update
    void Start()
    {
        musicSource = GetComponent<AudioSource>();
        StartCoroutine(PlayMusicWithPause());
    }

    IEnumerator PlayMusicWithPause()
    {
        while (true)
        {
            // Select a new track index that isn't the last one
            int nextTrackIndex;
            do
            {
                nextTrackIndex = Random.Range(0, musicTracks.Length);
            } while (musicTracks.Length > 1 && nextTrackIndex == lastTrackIndex);

            lastTrackIndex = nextTrackIndex;
            musicSource.clip = musicTracks[nextTrackIndex];
            musicSource.Play();

            // Wait for the song to finish
            yield return new WaitForSeconds(musicSource.clip.length);

            // Pause for a random time between min and max
            float pauseTime = Random.Range(minPauseTime, maxPauseTime);
            musicSource.Stop();
            yield return new WaitForSeconds(pauseTime);
        }
    }
}
