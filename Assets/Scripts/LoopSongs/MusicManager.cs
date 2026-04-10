using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Crossfade")]
    [SerializeField]
    private float crossfadeDuration = 2f;

    private EventInstance _eventInstance;
    private bool _eventStarted = false;

    private ZonaPista _currentPista;

    private readonly Dictionary<ZonaPista, float> _paramValues = new();

    public float CrossfadeDuration => crossfadeDuration;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    //al entrar ZonaCancion
    public void IniciarEvento(EventRef eventRef)
    {
        if (_eventInstance.isValid()) return;
        _eventInstance = RuntimeManager.CreateInstance(eventRef);
        _eventStarted = false;
    }

    //Al salir ZonaCancion
    public void DetenerEvento(float fadeDuration)
    {
        if (!_eventInstance.isValid()) return;

        if(_currentPista != null)
        {
            StartCoroutine(FadeOutYDetener(fadeDuration));
        } 
        else
        {
            StopYLiberar();
        }

        _currentPista = null;
        _eventStarted = false;
        _paramValues.Clear();
    }

    //Al entrar ZonaPista
    public void EnterPista(ZonaPista pista)
    {
        if (!_eventInstance.isValid()) return;

        if (pista.isStartingByStart)
        {
            _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _eventInstance.start();
            _eventStarted = true;
        }
        else
        {
            //arrancar evento primera pista
            if (!_eventStarted)
            {
                _eventInstance.start();
                _eventStarted = true;
            }
        }

        if (!_paramValues.ContainsKey(pista))
        {
            _paramValues[pista] = 0f;
        }

        _currentPista = pista;
    }

    //Al salir ZonaPista
    public void ExitPista(ZonaPista pista)
    {
        if(_currentPista == pista)
        {
            _currentPista = null;
        }
    }

    private void Update()
    {
        if (!_eventInstance.isValid() || !_eventStarted) return;

        float step = Time.deltaTime / crossfadeDuration;

        foreach (var pista in new List<ZonaPista>(_paramValues.Keys))
        {
            float target = (pista == _currentPista) ? 1f : 0f;
            _paramValues[pista] = Mathf.MoveTowards(_paramValues[pista], target, step);
            _eventInstance.setParameterByName(pista.ParamName, _paramValues[pista]);
        }
    }

    //UI
    public float GetParamValue(ZonaPista pista)
    {
        return _paramValues.TryGetValue(pista, out float v) ? v : 0f;
    }

    private IEnumerator FadeOutYDetener(float duration)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
            if(_currentPista != null)
            {
                _eventInstance.setParameterByName(_currentPista.ParamName, t);
            }
            yield return null;
        }
        StopYLiberar();
    }

    private void StopYLiberar()
    {
        _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _eventInstance.release();
        _eventInstance = default;
    }

    private void OnDestroy()
    {
        if (_eventInstance.isValid())
        {
            _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _eventInstance.release();
        }
    }
}
