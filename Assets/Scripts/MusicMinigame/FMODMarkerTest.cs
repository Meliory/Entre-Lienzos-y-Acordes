using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

public class FMODMarkerTest : MonoBehaviour
{
    [FMODUnity.BankRef]
    public string eventPath = "event:/Test";

    private FMOD.Studio.EventInstance _instance;
    private FMOD.Studio.EVENT_CALLBACK _callback;

    private static ConcurrentQueue<string> _markerQueue = new ConcurrentQueue<string>();

    void Start()
    {
        //instania del evento
        _instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);

        //Suscribirse al callback de marcadores
        _callback = new FMOD.Studio.EVENT_CALLBACK(TimelineCallback);
        _instance.setCallback(_callback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        _instance.start();
        Debug.Log("[FMOD] Event started");
    }

    void Update()
    {
        //procesar marcador en el hilo principal
        while(_markerQueue.TryDequeue(out string markerName))
        {
            Debug.Log($"[FMOD] Procesando marcador en Update: '{markerName}'");
        }
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT TimelineCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr,IntPtr paramPtr)
    {
        if(type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
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
}
