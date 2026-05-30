using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicaManager : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicaEvento;

    [Header("Timeline")]
    public PlayableDirector director;

    private FMOD.Studio.EventInstance _musicaInstance;

    private void Start()
    {
        _musicaInstance = RuntimeManager.CreateInstance(musicaEvento);
        _musicaInstance.start();
    }

    public void Pausar()
    {
        _musicaInstance.setPaused(true);
    }

    public void Reanudar()
    {
        _musicaInstance.setPaused(false);
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
