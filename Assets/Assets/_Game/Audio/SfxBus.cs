using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bus global de SFX con pool de AudioSource (2D) y anti-solapado básico.
/// Añádelo una sola vez a la escena (o a un bootstrap) y asigna 1 clip por tipo.
/// Usa SfxBus.PlayHit(), SfxBus.PlayParry(), SfxBus.PlayDefend().
/// </summary>
[DisallowMultipleComponent]
public class SfxBus : MonoBehaviour
{
    public static SfxBus Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip parryClip;
    [SerializeField] private AudioClip defendClip;

    [Header("Ajustes")]
    [Range(0f, 1f)] [SerializeField] private float volume = 0.9f;
    [Tooltip("Variación aleatoria de pitch ±jitter")] 
    [Range(0f, 0.25f)] [SerializeField] private float pitchJitter = 0.05f;
    [Tooltip("Tamaño del pool de AudioSources")]
    [SerializeField] private int poolSize = 6;
    [Tooltip("Antiespam por tipo (ms). Evita cascadas en frames contiguos.")]
    [SerializeField] private float minIntervalMs = 60f;

    private readonly List<AudioSource> _pool = new List<AudioSource>(8);
    private int _rrIndex = 0;
    private float _lastHitTime;
    private float _lastParryTime;
    private float _lastDefendTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitPool();
    }

    private void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D
            src.volume = volume;
            _pool.Add(src);
        }
    }

    private AudioSource NextSource()
    {
        // Primero intenta uno libre (sin solapar).
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].isPlaying) return _pool[i];
        }
        // Si todos están ocupados, reutiliza por Round-Robin (corta el más antiguo).
        var s = _pool[_rrIndex++ % _pool.Count];
        s.Stop();
        return s;
    }

    private bool CanPlay(ref float lastTimeStamp)
    {
        float minInterval = minIntervalMs / 1000f;
        float now = Time.unscaledTime;
        if (now - lastTimeStamp < minInterval) return false;
        lastTimeStamp = now;
        return true;
    }

    private void PlayClip(ref float guardTs, AudioClip clip)
    {
        if (clip == null) return;
        if (!CanPlay(ref guardTs)) return;

        var src = NextSource();
        src.clip = clip;
        src.volume = volume;
        src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        src.Play();
    }

    // API pública (estática) — cómoda de invocar desde cualquier lugar.
    public static void PlayHit()
    {
        if (Instance == null) return;
        Instance.PlayClip(ref Instance._lastHitTime, Instance.hitClip);
    }

    public static void PlayParry()
    {
        if (Instance == null) return;
        Instance.PlayClip(ref Instance._lastParryTime, Instance.parryClip);
    }

    public static void PlayDefend()
    {
        if (Instance == null) return;
        Instance.PlayClip(ref Instance._lastDefendTime, Instance.defendClip);
    }

    // Utilidades opcionales
    public void SetClips(AudioClip hit, AudioClip parry, AudioClip defend)
    {
        hitClip = hit;
        parryClip = parry;
        defendClip = defend;
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        for (int i = 0; i < _pool.Count; i++)
            _pool[i].volume = volume;
    }

    // Facilita verificación en pruebas
    public bool IsAnyPlaying()
    {
        for (int i = 0; i < _pool.Count; i++)
            if (_pool[i].isPlaying) return true;
        return false;
    }
}