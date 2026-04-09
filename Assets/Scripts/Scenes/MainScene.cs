using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    float startTime;
    float runTime;

    GameObject dialogueBox;
    Text runTimeText;
    TextMeshProUGUI tmpDialogueText;
    TextMeshProUGUI screenMessageText;

    [SerializeField] bool showRunTime;

    public AudioClip musicClip;

    [SerializeField] GameObject player;
    [SerializeField] GameObject hologram;

    [SerializeField] float startSeqBeginPoint = 5.6f;
    [SerializeField] float startSeqEndPoint = 7.0f;

    [SerializeField] GameObject wallLeft;
    [SerializeField] GameObject checkpointTrigger;
    [SerializeField] float wallLeftXPos = 11.5f;

    public enum LevelStates
    {
        Exploration,
        Hologram,
        KeepLooking,
        Checkpoint
    };
    public LevelStates levelState = LevelStates.Exploration;

    string[] dialogueStrings =
    {
        "DR. LIGHT:\n\n\tMEGA MAN. I CAN'T STAY HERE LONG.",
        "DR. LIGHT:\n\n\tMY HOLOGRAM IS VERY UNSTABLE.",
        "MEGA MAN:\n\n\tDR. LIGHT, I HAVEN'T SEEN ANY DISTURBANCE.",
        "DR. LIGHT:\n\n\tTHERE HAS TO BE SOMETHING. KEEP LOOKING.",
        "MEGA MAN:\n\n\tOKAY. I'LL CONTACT YOU SOON."
    };

    void Awake()
    {
        dialogueBox = GameObject.Find("DialogueBox");
        runTimeText = GameObject.Find("RunTime").GetComponent<Text>();
        tmpDialogueText = GameObject.Find("DialogueText").GetComponent<TextMeshProUGUI>();
        screenMessageText = GameObject.Find("ScreenMessage").GetComponent<TextMeshProUGUI>();

        tmpDialogueText.text = "";
        dialogueBox.SetActive(false);
    }

    void Update()
    {

        runTimeText.text = showRunTime ? string.Format("TIME: {0:0.00}", runTime) : "";

        switch (levelState)
        {
            case LevelStates.Exploration:
                if (player)
                {
                    if (player.transform.position.x >= startSeqBeginPoint)
                    {
                        startTime = Time.time;

                        player.GetComponent<PlayerController>().FreezeInput(true);
                        Vector2 playerVelocity = player.GetComponent<Rigidbody2D>().linearVelocity;
                        player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerVelocity.y);
                        
                        levelState = LevelStates.Hologram;
                    }
                }
                break;
            case LevelStates.Hologram:
                runTime = Time.time - startTime;
                if(UtilityFunctions.InTime(runTime, 2.0f, 5.0f))
                {
                    if (player.transform.position.x <= startSeqEndPoint)
                    {
                        player.GetComponent<PlayerController>().SimulateMoveRight();
                    }
                    else
                    {
                        player.GetComponent<PlayerController>().SimulateMoveStop();
                    }
                }
                if (UtilityFunctions.InTime(runTime, 5.0f))
                {
                    dialogueBox.SetActive(true);
                    tmpDialogueText.text = dialogueStrings[0];
                }
                if (UtilityFunctions.InTime(runTime, 9.0f))
                {
                    tmpDialogueText.text = dialogueStrings[1];
                }
                if (UtilityFunctions.InTime(runTime, 13.0f))
                {
                    tmpDialogueText.text = dialogueStrings[2];
                }
                if (UtilityFunctions.InTime(runTime, 17.0f))
                {
                    tmpDialogueText.text = dialogueStrings[3];
                }
                if (UtilityFunctions.InTime(runTime, 21.0f))
                {
                    tmpDialogueText.text = dialogueStrings[4];
                }
                if (UtilityFunctions.InTime(runTime, 24.0f))
                {
                    tmpDialogueText.text = "";
                    dialogueBox.SetActive(false);
                }
                if (UtilityFunctions.InTime(runTime, 25.0f))
                {
                    StartCoroutine(FlickerOutHologram());
                }
                if (UtilityFunctions.InTime(runTime, 28.0f))
                {
                    player.GetComponent<PlayerController>().FreezeInput(false);
                    levelState = LevelStates.KeepLooking;
                }

                break;
            case LevelStates.KeepLooking:
                break;
            case LevelStates.Checkpoint:
                break;
        }
    }

    public void CheckpointReached()
    {
        StartCoroutine(CoCheckpointReached());
        levelState = LevelStates.Checkpoint;
        Vector3 wallLeftPos = wallLeft.transform.position;
        wallLeftPos.x = wallLeftXPos;
        wallLeft.transform.position = wallLeftPos;
        checkpointTrigger.SetActive(false);
    }

    private IEnumerator CoCheckpointReached()
    {
        screenMessageText.alignment = TextAlignmentOptions.Center;
        screenMessageText.alignment = TextAlignmentOptions.Top;
        screenMessageText.fontStyle = FontStyles.UpperCase;
        screenMessageText.fontSize = 24;
        screenMessageText.text = "CHECKPOINT REACHED";
        yield return new WaitForSeconds(5f);
        screenMessageText.text = "";
    }

    private IEnumerator FlickerOutHologram()
    {
        Color originalColor = hologram.GetComponent<SpriteRenderer>().color;
        float delay1 = 0.15f;
        float delay2 = 0.025f;

        for (int i = 0; i < 5; i++)
        {
            hologram.GetComponent<SpriteRenderer>().color = Color.clear;
            yield return new WaitForSeconds(delay1);
            hologram.GetComponent<SpriteRenderer>().color = originalColor;
            yield return new WaitForSeconds(delay1);
        }

        for (int i = 0; i < 5; i++)
        {
            hologram.GetComponent<SpriteRenderer>().color = Color.clear;
            yield return new WaitForSeconds(delay2);
            hologram.GetComponent<SpriteRenderer>().color = originalColor;
            yield return new WaitForSeconds(delay2);
        }

        Destroy(hologram);
    }
}
