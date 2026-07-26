using UnityEngine;
using System.Collections.Generic;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry {
        public string key;
        public AudioClip clip;
        [Range(0f,3f)] public float volume = 1f;
        public bool loop = false;
        public bool playOnAwake = false;
        [Range(-3f,3f)] public float pitch = 1f;
        [Range(0f,1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    }

    [Tooltip("Add AudioClips here and configure volume/loop/pitch. Keys must be unique.")]
    public List<SoundEntry> sounds = new List<SoundEntry>();

    // runtime map from key -> AudioSource
    private Dictionary<string, AudioSource> sourceMap = new Dictionary<string, AudioSource>();

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // create an AudioSource per entry and configure it
        for (int i = 0; i < sounds.Count; i++) {
            var e = sounds[i];
            if (string.IsNullOrEmpty(e.key)) {
                Debug.LogWarning($"AudioSystem: Sound entry at index {i} has empty key.");
                continue;
            }
            if (sourceMap.ContainsKey(e.key)) {
                Debug.LogWarning($"AudioSystem: Duplicate key '{e.key}' found. Skipping duplicate.");
                continue;
            }

            var go = new GameObject($"Audio_{e.key}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.clip = e.clip;
            src.volume = Mathf.Clamp01(e.volume);
            src.loop = e.loop;
            src.playOnAwake = e.playOnAwake;
            src.pitch = e.pitch;
            src.spatialBlend = Mathf.Clamp01(e.spatialBlend);

            sourceMap[e.key] = src;

            if (e.playOnAwake && e.clip != null) src.Play();
        }
    }

    private AudioSource FindSource(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        AudioSource s;
        sourceMap.TryGetValue(key, out s);
        return s;
    }

    public void Play(string key)
    {
        var s = FindSource(key);
        if (s != null) {
            if (s.clip != null) s.Play();
            else Debug.LogWarning($"AudioSystem: Play() - '{key}' has no clip assigned.");
        } else Debug.LogWarning($"AudioSystem: Play() - no sound with key '{key}'");
    }

    public void PlayOneShot(string key, float volumeScale = 1f)
    {
        var s = FindSource(key);
        if (s != null) {
            if (s.clip != null) s.PlayOneShot(s.clip, Mathf.Clamp01(volumeScale));
            else Debug.LogWarning($"AudioSystem: PlayOneShot() - '{key}' has no clip.");
        } else Debug.LogWarning($"AudioSystem: PlayOneShot() - no sound with key '{key}'");
    }

    public void Stop(string key)
    {
        var s = FindSource(key);
        if (s != null) s.Stop();
        else Debug.LogWarning($"AudioSystem: Stop() - no sound with key '{key}'");
    }

    public void SetVolume(string key, float volume)
    {
        var s = FindSource(key);
        if (s != null) s.volume = Mathf.Clamp01(volume);
        else Debug.LogWarning($"AudioSystem: SetVolume() - no sound with key '{key}'");
    }

    // Play a runtime clip at position without adding to map
    public void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
    }

    // Optional: get list of keys
    public IEnumerable<string> GetAllKeys() => sourceMap.Keys;
}
