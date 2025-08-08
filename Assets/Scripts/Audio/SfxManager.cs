using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : Singleton<SfxManager>
{
    [SerializeField] int poolSize = 16;
    [SerializeField] float defaultMinDistance = 2f;
    [SerializeField] float defaultMaxDistance = 20f;
    [SerializeField] float sameCueCooldown = 0.05f; // anti-spam

    readonly Queue<AudioSource> pool = new();
    readonly Dictionary<AudioCue, float> lastPlayed = new();

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = defaultMinDistance;
            src.maxDistance = defaultMaxDistance;
            pool.Enqueue(src);
        }
    }

    bool OnCooldown(AudioCue cue)
    {
        if (!lastPlayed.TryGetValue(cue, out float t)) return false;
        return Time.unscaledTime - t < sameCueCooldown;
    }

    AudioSource GetSource() => pool.Count > 0 ? pool.Dequeue() : new GameObject("SFX_EXTRA").AddComponent<AudioSource>();
    void Release(AudioSource s){ s.Stop(); s.clip=null; s.transform.SetParent(transform); pool.Enqueue(s); }

    void Apply(AudioSource s, AudioCue cue)
    {
        s.clip = cue.Pick();
        s.volume = cue.Vol();
        s.pitch = cue.Pitch();
        s.outputAudioMixerGroup = cue.mixerGroup;
        s.spatialBlend = cue.spatial3D ? cue.spatialBlend : 0f;
    }

    IEnumerator ReleaseWhenDone(AudioSource s){ yield return new WaitWhile(()=> s && s.isPlaying); if(s) Release(s); }

    public void Play2D(AudioCue cue)
    {
        if (!cue || OnCooldown(cue)) return;
        var clip = cue.Pick(); if (!clip) return;

        var s = GetSource();
        Apply(s, cue);
        s.spatialBlend = 0f;
        s.transform.position = Vector3.zero;
        s.Play();
        lastPlayed[cue] = Time.unscaledTime;
        StartCoroutine(ReleaseWhenDone(s));
    }

    public void PlayAt(AudioCue cue, Vector3 pos)
    {
        if (!cue || OnCooldown(cue)) return;
        var clip = cue.Pick(); if (!clip) return;

        var s = GetSource();
        Apply(s, cue);
        s.transform.position = pos;
        s.Play();
        lastPlayed[cue] = Time.unscaledTime;
        StartCoroutine(ReleaseWhenDone(s));
    }
}
