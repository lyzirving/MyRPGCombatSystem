using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public int poolSize = 5;

    private List<AudioSource> m_AudioPool = new List<AudioSource>();
    private int m_AvailableIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_AvailableIndex = 0;
        for (int i = 0; i < poolSize; ++i)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;

            source.spatialBlend = 1f; // 3d audio effect
            source.volume = 1f;
            source.priority = 128; // medium priority
            source.outputAudioMixerGroup = null;

            m_AudioPool.Add(source);
        }
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;

        if (m_AvailableIndex < m_AudioPool.Count)
        {
            AudioSource source = m_AudioPool[m_AvailableIndex];
            source.PlayOneShot(clip);
            ++m_AvailableIndex;
            StartCoroutine(ReturnToPool(source, clip.length));
        }
        else
            Debug.LogWarning("No available audio source");
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        float start = Time.time;
        while (Time.time - start < delay)
            yield return null;

        --m_AvailableIndex;
    }
}
