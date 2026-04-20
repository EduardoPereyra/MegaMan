using UnityEngine;
using UnityEngine.Events;

public class CameraTransition : MonoBehaviour
{
    public enum TransitionEntry { Enter, Exit };
    public enum TransitionState { PreDelay, Transition, PostDelay };
    public enum TransitionDirection { Horizontal, Vertical };
    public enum TransitionEventCall { 
        BothBeforeDelay, 
        BothAfterDelay, 
        OnEnterBeforeDelay, 
        OnEnterAfterDelay, 
        OnExitBeforeDelay, 
        OnExitAfterDelay 
    };
    [SerializeField] bool onlyMoveCamera; 
    [SerializeField] TransitionEntry entry = TransitionEntry.Enter;
    [SerializeField] TransitionState state = TransitionState.PreDelay;
    [SerializeField] TransitionDirection direction = TransitionDirection.Horizontal;
    [SerializeField] TransitionEventCall eventCallPreDelay = TransitionEventCall.BothBeforeDelay;
    [SerializeField] TransitionEventCall eventCallPostDelay = TransitionEventCall.BothBeforeDelay;

    [Header("Timers")]
    [SerializeField] float transitionDelay = 1f;
    [SerializeField] float preTransitionDelay = 1f;
    [SerializeField] float postTransitionDelay = 1f;

    [Header("Positions")]
    [SerializeField] Vector2 cameraMinPosition;
    [SerializeField] Vector2 cameraMaxPosition;
    [SerializeField] Vector2 playerChange;

    [Header("Events")]
    public UnityEvent preTransitionEvent;
    public UnityEvent postTransitionEvent;
    public UnityEvent onlyMoveCameraEvent;

    CameraFollow cam;
    GameObject player;

    Vector2 cameraMinPrevious;
    Vector2 cameraMaxPrevious;
    Vector2 cameraMoveStart;
    Vector2 cameraMoveFinish;
    Vector2 cameraMoveProgress;
    Vector2 playerMoveStart;
    Vector2 playerMoveFinish;

    float progress;
    float transitionTimer;

    bool transition;
    bool getCamPrevious = true;
    bool callPreTransitionEvent = true;
    bool callPostTransitionEvent = true;


    void Start()
    {
        cam = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        if (transition)
        {
            switch (state)
            {
                case TransitionState.PreDelay:
                    CallEventBeforePreDelay();
                    transitionTimer -= Time.deltaTime;
                    if (transitionTimer <= 0)
                    {
                        CallEventAfterPreDelay();
                        player.GetComponent<Animator>().speed = 1;
                        state = TransitionState.Transition;
                        transitionTimer = 0;
                    }
                    break;
                case TransitionState.Transition:
                    progress = Mathf.Clamp(transitionTimer, 0 , transitionDelay) / transitionDelay;
                    transitionTimer += Time.deltaTime;
                    cameraMoveProgress = Vector2.Lerp(cameraMoveStart, cameraMoveFinish, progress);
                    cam.transform.position = new Vector3(cameraMoveProgress.x, cameraMoveProgress.y, cam.transform.position.z);
                    player.transform.position = Vector2.Lerp(playerMoveStart, playerMoveFinish, progress);
                    if (progress >= 1)
                    {
                        player.GetComponent<Animator>().speed = 0;
                        cam.boundsMin = (entry == TransitionEntry.Enter) ? cameraMinPosition : cameraMinPrevious;
                        cam.boundsMax = (entry == TransitionEntry.Enter) ? cameraMaxPosition : cameraMaxPrevious;
                        player.transform.position = playerMoveFinish;
                        cam.player = player.transform;
                        state = TransitionState.PostDelay;
                        transitionTimer = postTransitionDelay;
                    }
                    break;
                case TransitionState.PostDelay:
                    CallEventBeforePostDelay();
                    transitionTimer -= Time.deltaTime;
                    if (transitionTimer <= 0)
                    {
                        CallEventAfterPostDelay();
                        entry = (entry == TransitionEntry.Enter) ? TransitionEntry.Exit : TransitionEntry.Enter;
                        transition = false;
                        player.GetComponent<PlayerController>().FreezeInput(false);
                        player.GetComponent<PlayerController>().FreezePlayer(false);
                    }
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (onlyMoveCamera)
            {
                cam.boundsMin = cameraMinPosition;
                cam.boundsMax = cameraMaxPosition;
                onlyMoveCameraEvent.Invoke();
                return;
            }

            if (!transition)
            {
                transition = true;
                player = collision.gameObject;
                cam.player = null;

                transitionTimer = preTransitionDelay;
                state = TransitionState.PreDelay;

                callPreTransitionEvent = true;
                callPostTransitionEvent = true;
                cameraMoveStart = cam.transform.position;
                playerMoveStart = player.transform.position;
                if (entry == TransitionEntry.Enter)
                {
                    if (getCamPrevious)
                    {
                        getCamPrevious = false;
                        cameraMinPrevious = cam.boundsMin;
                        cameraMaxPrevious = cam.boundsMax;
                    }
                    playerMoveFinish = playerMoveStart + playerChange;
                }
                else
                {
                    playerMoveFinish = playerMoveStart - playerChange;
                }

                if (direction == TransitionDirection.Horizontal)
                {
                    float cameraMinPosX = cameraMinPosition.x;
                    if (entry == TransitionEntry.Exit)
                    {
                        cameraMinPosX = (playerChange.x > 0) ? cameraMaxPrevious.x : cameraMinPrevious.x;
                    }
                    cameraMoveFinish = new Vector2(cameraMinPosX, cam.transform.position.y);
                }
                else
                {
                    float cameraMinPosY = cameraMinPosition.y;
                    if (entry == TransitionEntry.Exit)
                    {
                        cameraMinPosY = (playerChange.y > 0) ? cameraMaxPrevious.y : cameraMinPrevious.y;
                    }
                    cameraMoveFinish = new Vector2(cam.transform.position.x, cameraMinPosY);
                }

                player.GetComponent<Animator>().speed = 0;
                player.GetComponent<PlayerController>().FreezeInput(true);
                player.GetComponent<PlayerController>().FreezePlayer(true);
            }
        }
    }

    private void CallPreTransitionEvent()
    {
        if (callPreTransitionEvent)
        {
            callPreTransitionEvent = false;
            preTransitionEvent.Invoke();
        }
    }

    private void CallEventBeforePreDelay()
    {
        switch (eventCallPreDelay)
        {
            case TransitionEventCall.BothBeforeDelay:
                CallPreTransitionEvent();
                break;
            case TransitionEventCall.OnEnterBeforeDelay:
                if (entry == TransitionEntry.Enter)
                {
                    CallPreTransitionEvent();
                }
                break;
            case TransitionEventCall.OnExitBeforeDelay:
                if (entry == TransitionEntry.Exit)
                {
                    CallPreTransitionEvent();
                }
                break;
        }
    }

        private void CallEventAfterPreDelay()
    {
        switch (eventCallPreDelay)
        {
            case TransitionEventCall.BothAfterDelay:
                CallPreTransitionEvent();
                break;
            case TransitionEventCall.OnEnterAfterDelay:
                if (entry == TransitionEntry.Enter)
                {
                    CallPreTransitionEvent();
                }
                break;
            case TransitionEventCall.OnExitAfterDelay:
                if (entry == TransitionEntry.Exit)
                {
                    CallPreTransitionEvent();
                }
                break;
        }
    }

    private void CallPostTransitionEvent()
    {
        if (callPostTransitionEvent)
        {
            callPostTransitionEvent = false;
            postTransitionEvent.Invoke();
        }
    }

        private void CallEventBeforePostDelay()
    {
        switch (eventCallPostDelay)
        {
            case TransitionEventCall.BothBeforeDelay:
                CallPostTransitionEvent();
                break;
            case TransitionEventCall.OnEnterBeforeDelay:
                if (entry == TransitionEntry.Enter)
                {
                    CallPostTransitionEvent();
                }
                break;
            case TransitionEventCall.OnExitBeforeDelay:
                if (entry == TransitionEntry.Exit)
                {
                    CallPostTransitionEvent();
                }
                break;
        }
    }

        private void CallEventAfterPostDelay()
    {
        switch (eventCallPostDelay)
        {
            case TransitionEventCall.BothAfterDelay:
                CallPostTransitionEvent();
                break;
            case TransitionEventCall.OnEnterAfterDelay:
                if (entry == TransitionEntry.Enter)
                {
                    CallPostTransitionEvent();
                }
                break;
            case TransitionEventCall.OnExitAfterDelay:
                if (entry == TransitionEntry.Exit)
                {
                    CallPostTransitionEvent();
                }
                break;
        }
    }
}
