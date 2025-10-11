using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CameraControl : MonoBehaviour
{
    public PlayableDirector Director { get; private set; }
    public TimelineAsset Timeline { get; private set; }
    [Tooltip("The maximum camera transition time ")]
    public float _camTransitionTimeMax = 1.8f;
    [Tooltip("The camera move time equals to the distance between cameras times this value")]
    public float _camTransitionTimeDelta = 0.1f;
    CinemachineVirtualCamera _curVirCam;
    CinemachineVirtualCamera _mainVirCam;

    GameController Controller { get { return GameController.Instance; } }
    GameMethods gameMethods { get { return GameMethods.Instance; } }
    public CinemachineVirtualCamera MainVirCam {
        get
        {
            if (_mainVirCam == null && gameObject.scene.buildIndex == 1)
            {
                MainVirCam = GameObject.FindGameObjectWithTag("VirtualCameras").transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
            }
            if (_mainVirCam == null)
            {
                Debug.Log("Cannot find Main CinemachineBrain");
            }
            return _mainVirCam;
        }
        set
        {
            _mainVirCam = value;
        }
    }
    CinemachineBrain _mainCamBrain;
    public CinemachineBrain MainCamBrain
    {
        get
        {
            if(_mainCamBrain == null)
            {
                _mainCamBrain = Camera.main.GetComponent<CinemachineBrain>();
            }
            if (_mainCamBrain == null)
            {
                Debug.Log("Cannot find Main CinemachineBrain");
            }
            return _mainCamBrain;
        }
        set
        {
            _mainCamBrain = value;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        _curVirCam = MainVirCam;
    }

    public void GameStartInitialize()
    {
    }

    /// <summary>
    /// Sets the camera transition from the current virtual camera to a new virtual camera with an optional completion callback.
    /// </summary>
    /// <param name="camEaseOut">The virtual camera to transition to.</param>
    /// <param name="onComplete">An optional action to perform when the transition is complete.</param>
    public void SetCameraTransition(CinemachineVirtualCamera camEaseOut, System.Action onComplete = null)
    {
        _curVirCam.Priority = 5;
        camEaseOut.Priority = 10;

        float distance = Vector3.Distance(_curVirCam.transform.position, camEaseOut.transform.position);
        float camTransitionTime = Mathf.Min(distance * _camTransitionTimeDelta, _camTransitionTimeMax);
        // set blend time
        MainCamBrain.m_DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Style.EaseInOut,
            camTransitionTime);

        // start detecting if blend is finished
        StartCoroutine(CheckBlending(camEaseOut, onComplete));
    }

    /// <summary>
    /// Coroutine to check if the camera blending is complete and performs a callback if provided.
    /// </summary>
    /// <param name="camEaseOut">The virtual camera that is transitioning to.</param>
    /// <param name="callback">An optional action to perform once the blending is complete.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    IEnumerator CheckBlending(CinemachineVirtualCamera camEaseOut, System.Action callback)
    {
        
        //the main cam is not blending yet at the beginning, wait for a bit sencond so it actually blending
        yield return new WaitForSeconds(0.1f);
        while (MainCamBrain.IsBlending)// wait until blend finish
        {
            //Debug.Log($"{MainCamBrain.gameObject.name} is blending: {MainCamBrain.IsBlending}");
            yield return new WaitForFixedUpdate();//delay for a fixed update frame;
        }
        camEaseOut.MoveToTopOfPrioritySubqueue();
        yield return new WaitForEndOfFrame();
        _curVirCam = camEaseOut; // set new camera as current camera
        callback?.Invoke();

        //void OnCameraExitedBlending(CinemachineVirtualCameraBase virtualCamera)
        //{
        //    // This method will be called when the camera exits blending
        //    // You can use it to trigger additional actions if needed
        //}
    }
}
