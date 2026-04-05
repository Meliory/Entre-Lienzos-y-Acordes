using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Personajes")]
    public CharacterData aura;
    public CharacterData alberto;
    public CharacterData ruby;
    public CharacterData ramon;

    [Header("Minigame Variables")]
    [FMODUnity.BankRef]
    public string eventPath = "event:/Test";

    private FMOD.Studio.EventInstance _instance;
    private FMOD.Studio.EVENT_CALLBACK _callback;

    private static ConcurrentQueue<string> _markerQueue = new ConcurrentQueue<string>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //instania del evento
        _instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);

        //Suscribirse al callback de marcadores
        _callback = new FMOD.Studio.EVENT_CALLBACK(TimelineCallback);
        _instance.setCallback(_callback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        _instance.start();

        //start minigame characters

        aura.ActiveMinigame();
        //ramon.ActiveMinigame();
        //ruby.ActiveMinigame();
        //alberto.ActiveMinigame();

        Debug.Log("[FMOD] Event started");
    }

    void Update()
    {
        //procesar marcador en el hilo principal
        while (_markerQueue.TryDequeue(out string markerName))
        {
            TriggerEvent(markerName);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT TimelineCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var props = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)
                Marshal.PtrToStructure(
                    paramPtr,
                    typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES)
                );

            _markerQueue.Enqueue(props.name);
        }

        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _instance.release();
    }

    public void TriggerEvent(string markerName)
    {
        switch (markerName)
        {
            case "event_test":
                aura.ActivateEvent();
                Debug.Log("[EventManager] Aura está nerviosa.");
                break;

        }
    }

    public void OnCharacterFailed(string characterName)
    {
        Debug.Log($"[EventManager] GAME OVER — {characterName} ha fallado.");
    }


}
