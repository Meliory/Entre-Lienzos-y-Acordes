using FMODUnity;
using UnityEngine;

public class CinematicaManager : MonoBehaviour
{
    [Header("FMOD")]
    public EventRef musicaEvento;

    private FMOD.Studio.EventInstance _musicaInstance;

    private void Start()
    {
        _musicaInstance = RuntimeManager.CreateInstance(musicaEvento);
        _musicaInstance.start();
    }

    private void OnDestroy()
    {
        if (_musicaInstance.isValid())
        {
            _musicaInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _musicaInstance.release();
        }
    }
}
