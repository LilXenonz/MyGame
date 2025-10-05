using System.Collections.Generic;
using UnityEngine;

public class BasicAudioList : MonoBehaviour
{
    [Tooltip("List of WAV AudioClips to play")]
    public List<AudioClip> wavClips;

    [Tooltip("Number of AudioSources to pool")]
    public int poolSize = 10;

    private List<AudioSource> audioSourcePool;
    private int currentSourceIndex = 0;

    private void Awake()
    {
        audioSourcePool = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            audioSourcePool.Add(src);
        }
    }

    public void PlaySoundByIndex(int index)
    {
        if (index >= 0 && index < wavClips.Count && wavClips[index] != null)
        {
            AudioSource src = GetNextAvailableSource();
            src.clip = wavClips[index];
            src.Play();
        }
        else
        {
            Debug.LogWarning("Invalid index or null AudioClip.");
        }
    }

    private AudioSource GetNextAvailableSource()
    {
        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            int idx = (currentSourceIndex + i) % audioSourcePool.Count;
            if (!audioSourcePool[idx].isPlaying)
            {
                currentSourceIndex = (idx + 1) % audioSourcePool.Count;
                return audioSourcePool[idx];
            }
        }

        AudioSource src = audioSourcePool[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % audioSourcePool.Count;
        return src;
    }
}
