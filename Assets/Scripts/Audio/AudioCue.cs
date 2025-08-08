using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName="Audio/AudioCue")]
public class AudioCue : ScriptableObject
{
    public AudioClip[] clips;
    [Range(0f,1f)] public float volume = 1f;
    [Range(-12f,12f)] public float pitchSemitones = 0f;
    [Range(0f,0.2f)] public float volumeJitter = 0.03f;
    [Range(0f,0.5f)] public float pitchJitterSemitones = 0.1f;
    public bool spatial3D = false;
    [Range(0f,1f)] public float spatialBlend = 0f; // 0=2D for UI
    public AudioMixerGroup mixerGroup;

    public AudioClip Pick() => (clips==null||clips.Length==0) ? null : clips[Random.Range(0,clips.Length)];
    public float Vol() => Mathf.Clamp01(volume + Random.Range(-volumeJitter, volumeJitter));
    public float Pitch() => Mathf.Pow(2f, (pitchSemitones + Random.Range(-pitchJitterSemitones, pitchJitterSemitones))/12f);
}
