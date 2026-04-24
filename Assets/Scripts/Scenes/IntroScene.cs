using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    float startTime;
    float runTime;

    bool calledNextScene;

    bool inputDetected = false;

    [SerializeField] bool showRunTime;

    float progress;
    float fadeTimer;
    [SerializeField] float fadeDelay = 5f;

    [SerializeField] GameObject outsideLab;
    [SerializeField] GameObject insideLab;
    [SerializeField] GameObject player;

    public AudioClip musicClip;

    Text runTimeText;
    TextMeshProUGUI tmpDialogueText;

    float[] playerRunPoints =
    {
        0.38f,
        2.45f,
    };

    float musicVolume;

    private enum IntroSceneStates
    {
        OutsideLab,
        ScreenFade1,
        InsideLab,
        ScreenFade2,
        NextScene
    };
    IntroSceneStates currentState = IntroSceneStates.OutsideLab;

    string[] dialogStrings =
    {
        "SOMETIME IN THE FUTURE...",
        "DR. LIGHT'S LAB",
        "DR. LIGHT:\n\n\tMEGA MAN, COME OVER HERE PLEASE.",
        "DR. LIGHT:\n\n\tTHERE IS A DISTURBANCE AT THE HIGHWAY.",
        "DR. LIGHT:\n\n\tI NEED YOU TO INVESTIGATE.",
        "MEGA MAN:\n\n\tOF COURSE. I'LL LEAVE RIGHT AWAY.",
        "DR. LIGHT:\n\n\tTHANK YOU, MEGA MAN."
    };

    void Awake()
    {
        runTimeText = GameObject.Find("RunTime").GetComponent<Text>();
        tmpDialogueText = GameObject.Find("DialogueText").GetComponent<TextMeshProUGUI>();
        tmpDialogueText.text = "";
        player.GetComponent<PlayerController>().FreezeInput(true);
        foreach(Transform child in insideLab.transform)
        {
            child.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
        }
    }

    void Start()
    {
        startTime = Time.time;

        SoundManager.Instance.MusicSource.volume = 0.75f;
        SoundManager.Instance.PlayMusic(musicClip, false);
    }

    void Update()
    {
        runTime = Time.time - startTime;

        runTimeText.text = showRunTime ? string.Format("TIME: {0:0.00}", runTime) : "";

        if (Input.anyKey && !inputDetected && currentState != IntroSceneStates.ScreenFade2)
        {
            inputDetected = true;
            InitSceneExit();
        }

        switch (currentState)
        {
            case IntroSceneStates.OutsideLab:
                if (UtilityFunctions.InTime(runTime, 2.0f))
                {
                    tmpDialogueText.text = dialogStrings[0];
                }
                if (UtilityFunctions.InTime(runTime, 5.0f))
                {
                    tmpDialogueText.text = dialogStrings[1];
                }
                if (UtilityFunctions.InTime(runTime, 8.0f))
                {
                    currentState = IntroSceneStates.ScreenFade1;
                }
                break;
            case IntroSceneStates.ScreenFade1:
                progress = Mathf.Clamp(fadeTimer, 0, fadeDelay) / fadeDelay;
                fadeTimer += Time.deltaTime;
                foreach (Transform child in insideLab.transform)
                {
                    child.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, progress);
                }
                tmpDialogueText.color = new Color(1, 1, 1, 1f - (progress * 1.5f));
                if (progress >= 1f)
                {
                    tmpDialogueText.text = "";
                    tmpDialogueText.color = Color.white;
                    tmpDialogueText.alignment = TextAlignmentOptions.TopLeft;
                    currentState = IntroSceneStates.InsideLab;
                }
                break;
            case IntroSceneStates.InsideLab:
                if (UtilityFunctions.InTime(runTime, 14.0f))
                {
                    tmpDialogueText.text = dialogStrings[2];
                }
                if (UtilityFunctions.InTime(runTime, 17.0f))
                {
                    tmpDialogueText.text = "";
                }
                if(UtilityFunctions.InTime(runTime, 17.0f, 20.0f))
                {
                    if (player.transform.position.x >= playerRunPoints[0])
                    {
                        player.GetComponent<PlayerController>().SimulateMoveLeft();
                    }
                    else
                    {
                        player.GetComponent<PlayerController>().SimulateMoveStop();
                    }
                }
                if (UtilityFunctions.InTime(runTime, 20.0f))
                {
                    tmpDialogueText.text = dialogStrings[3];
                }
                if (UtilityFunctions.InTime(runTime, 24.0f))
                {
                    tmpDialogueText.text = dialogStrings[4];
                }
                if (UtilityFunctions.InTime(runTime, 28.0f))
                {
                    tmpDialogueText.text = dialogStrings[5];
                }
                if (UtilityFunctions.InTime(runTime, 32.0f))
                {
                    tmpDialogueText.text = dialogStrings[6];
                }
                if (UtilityFunctions.InTime(runTime, 35.0f))
                {
                    tmpDialogueText.text = "";
                }
                if(UtilityFunctions.InTime(runTime, 32.0f, 35.0f))
                {
                    if (player.transform.position.x <= playerRunPoints[1])
                    {
                        player.GetComponent<PlayerController>().SimulateMoveRight();
                    }
                    else
                    {
                        player.GetComponent<PlayerController>().SimulateMoveStop();
                    }
                }
                if (UtilityFunctions.InTime(runTime, 36.0f))
                {
                    InitSceneExit();
                }
                break;
            case IntroSceneStates.ScreenFade2:
                progress = Mathf.Clamp(fadeTimer, 0, fadeDelay) / fadeDelay;
                fadeTimer += Time.deltaTime;
                foreach (Transform child in insideLab.transform)
                {
                    child.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f - progress);
                }
                SoundManager.Instance.MusicSource.volume = musicVolume * (1f - progress);
                if (progress >= 1f)
                {
                    SoundManager.Instance.MusicSource.volume = 0;
                    currentState = IntroSceneStates.NextScene;
                }
                break;
            case IntroSceneStates.NextScene:
                if (!calledNextScene)
                {
                    GameManager.Instance.StartNextScene(GameManager.GameScenes.MainScene);
                    calledNextScene = true;
                }
                break;
        }
    }

    private void InitSceneExit()
    {
        fadeTimer = 0f;
        tmpDialogueText.text = "";
        outsideLab.SetActive(false);
        player.SetActive(false);
        musicVolume = SoundManager.Instance.MusicSource.volume;
        currentState = IntroSceneStates.ScreenFade2;
    }
}
