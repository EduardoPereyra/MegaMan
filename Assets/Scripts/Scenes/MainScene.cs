using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    float startTime;
    float runTime;

    bool calledNextScene;

    GameObject dialogueBox;
    Text runTimeText;
    TextMeshProUGUI tmpDialogueText;
    TextMeshProUGUI screenMessageText;

    bool sniperJoeEnabled;

    [SerializeField] bool showRunTime;

    public AudioClip musicClip;
    public AudioClip bossFightClip;
    public AudioClip victoryThemeClip;


    [SerializeField] GameObject player;
    [SerializeField] GameObject hologram;
    [SerializeField] GameObject enemy;
    [SerializeField] GameObject weaponPart;

    [SerializeField] float startSniperJoePoint = -14.1f;
    [SerializeField] float startSeqBeginPoint1 = 5.6f;
    [SerializeField] float startSeqEndPoint1 = 7.0f;

    [SerializeField] GameObject wallLeft;
    [SerializeField] GameObject checkpointTrigger;
    [SerializeField] float wallLeftXPos1 = 11.5f;

    // boss battle world adjustments
    [SerializeField] float startSeqBeginPoint2 = 20f;
    [SerializeField] float startSeqEndPoint2 = 22.25f;
    [SerializeField] float wallLeftXPos2 = 21.35f;
    [SerializeField] float timeOffset = 0.1f;
    [SerializeField] Vector3 minCamBounds = new Vector3(22f, 0);
    [SerializeField] Vector3 maxCamBounds = new Vector3(22f, 0.3f);


    public enum LevelStates { Exploration, Hologram, KeepLooking, Checkpoint, BossFightIntro, BossFight, PlayerVictory, NextScene };

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

    void Start()
    {
        // how many points is this level worth
        // this is a placeholder until the stage/level select screen is built
        GameManager.Instance.SetLevelPoints(50000);

        // attach action function (or listening event) to weapon part being collected

        // use this method if the weapon part gets dropped from the enemy being defeated
        //enemy.GetComponent<EnemyController>().BonusItemAction += WeaponPartCollected;

        // we use this method if the weapon part is just a game object in the scene
        weaponPart.GetComponent<ItemsController>().BonusItemEvent.AddListener(WeaponPartCollected);
    }

    void Update()
    {

        runTimeText.text = showRunTime ? string.Format("TIME: {0:0.00}", runTime) : "";

        switch (levelState)
        {
            case LevelStates.Exploration:
                if (player)
                {
                    if (player.transform.position.x >= startSniperJoePoint && !sniperJoeEnabled)
                    {
                        GameObject sniperJoe = GameObject.Find("SniperJoe");
                        if (sniperJoe)
                        {
                            sniperJoeEnabled = true;
                            sniperJoe.GetComponent<SniperJoeController>().EnableAI(true);
                        }
                    }

                    if (player.transform.position.x >= startSeqBeginPoint1)
                    {
                        startTime = Time.time;

                        player.GetComponent<PlayerController>().FreezeInput(true);
                        Vector2 playerVelocity = player.GetComponent<Rigidbody2D>().linearVelocity;
                        player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerVelocity.y);
                        
                        levelState = LevelStates.Hologram;
                    }
                    // warp ahead to skip the intro to speed along development
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        // move player, set new camera coords and bounds, advance level state
                        player.transform.position = new Vector2(18f, -1.14f);
                        Camera.main.transform.position = new Vector3(18f, 0, -10f);
                        Camera.main.GetComponent<CameraFollow>().boundsMin = new Vector3(12.2f, 0);
                        Camera.main.GetComponent<CameraFollow>().boundsMax = new Vector3(18f, 0.3f);
                        levelState = LevelStates.Checkpoint;
                    }
                }
                break;
            case LevelStates.Hologram:
                runTime = Time.time - startTime;
                if(UtilityFunctions.InTime(runTime, 2.0f, 5.0f))
                {
                    if (player.transform.position.x <= startSeqEndPoint1)
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
                // player can't move back so we look for the player reaching 
                // x coordinate to activate the boss fight intro state
                if (player != null)
                {
                    if (player.transform.position.x >= startSeqBeginPoint2)
                    {
                        // get start time
                        startTime = Time.time;
                        // freeze the player input and stop movement
                        player.GetComponent<PlayerController>().FreezeInput(true);
                        Vector2 playerVelocity = player.GetComponent<Rigidbody2D>().linearVelocity;
                        player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, playerVelocity.y);
                        // go to the boss fight intro state
                        levelState = LevelStates.BossFightIntro;
                    }
                }
                break;
            case LevelStates.BossFightIntro:
                // how long has this sequence been running for
                runTime = Time.time - startTime;

                // move player forward a bit and stop in front of the boss
                if (UtilityFunctions.InTime(runTime, 2.0f, 5.0f))
                {
                    if (player.transform.position.x <= startSeqEndPoint2)
                    {
                        player.GetComponent<PlayerController>().SimulateMoveRight();
                    }
                    else
                    {
                        player.GetComponent<PlayerController>().SimulateMoveStop();
                    }
                }

                // move the left wall to block in megaman and the boss plus change the camera bounds
                if (UtilityFunctions.InTime(runTime, 2.0f))
                {
                    // move the left wall
                    Vector3 wallPos = wallLeft.transform.position;
                    wallPos.x = wallLeftXPos2;
                    wallLeft.transform.position = wallPos;
                    // change the camera bounds and speed
                    // snap the camera to our boss fight area
                    Camera.main.GetComponent<CameraFollow>().timeOffset = timeOffset;
                    Camera.main.GetComponent<CameraFollow>().boundsMin = minCamBounds;
                    Camera.main.GetComponent<CameraFollow>().boundsMax = maxCamBounds;
                }

                // start fight music
                if (UtilityFunctions.InTime(runTime, 3.5f))
                {
                    SoundManager.Instance.StopMusic();
                    SoundManager.Instance.MusicSource.volume = 1f;
                    SoundManager.Instance.PlayMusic(bossFightClip);
                }

                // show the enemy health bar
                if (UtilityFunctions.InTime(runTime, 5.0f))
                {
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.EnemyHealth, 0);
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.EnemyHealth, UIEnergyBars.EnergyBarTypes.BombMan);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.EnemyHealth, true);
                }

                // do bombman's pose
                if (UtilityFunctions.InTime(runTime, 6.5f))
                {
                    enemy.GetComponent<BombManController>().Pose();
                }

                // fill enemy health bar and play sound clip
                if (UtilityFunctions.InTime(runTime, 7.0f))
                {
                    StartCoroutine(FillEnemyHealthBar());
                }

                // battle starts, enable boss ai and give player control
                if (UtilityFunctions.InTime(runTime, 8.5f))
                {
                    enemy.GetComponent<BombManController>().EnableAI(true);
                    player.GetComponent<PlayerController>().FreezeInput(false);
                    // move on to BossFight state
                    levelState = LevelStates.BossFight;
                }
                break;
            case LevelStates.BossFight:
                /*
                 * do stuff during the boss fight state (anything really)
                 *
                 * we have an event function that gets called when the boss is defeated and
                 * there is an action attached to the bonus item event listener (Weapon Part)
                 * when the player captures the weapon part then we can finish the level
                 *
                 * what we'll do during our boss fight is watch the music's time position
                 * and reset it to a position so it will constantly loop while the fight is
                 * going. if you listen to the music it's different in the beginning from
                 * where it loops. an alternative is to break the audio into two clips
                 * and play the "intro" first and then the "loop". I find it a little much.
                 * 
                 * The values I use for the clip loop start and end are guesstimates. Trying 
                 * to figure out precisely where a sound loops in an audio file is like meh.
                 */
                // look for time end and set new position when found
                if (SoundManager.Instance.MusicSource.time >= 15.974f)
                {
                    SoundManager.Instance.MusicSource.time = 3.192f;
                }
                break;
            case LevelStates.PlayerVictory:
                // how long has this sequence been running for
                runTime = Time.time - startTime;

                // have game manager do the score tally
                if (UtilityFunctions.InTime(runTime, 7.0f))
                {
                    GameManager.Instance.TallyPlayerScore();
                }

                // reset the points collected and go to next scene state
                if (UtilityFunctions.InTime(runTime, 15.0f))
                {
                    GameManager.Instance.ResetPointsCollected();
                    // switch to the next scene state
                    levelState = LevelStates.NextScene;

                }
                break;
            case LevelStates.NextScene:
                // tell GameManager to trigger the next scene
                if (!calledNextScene)
                {
                    GameManager.Instance.StartNextScene();
                    calledNextScene = true;
                }
                break;
        }
    }

    public void Highway0Reached()
    {
        // GameObject kamadoma = GameObject.Find("Kamadoma2");
        // if (kamadoma)
        // {
        //     kamadoma.GetComponent<KamadomaController>().EnableAI(true);
        // }

        GameObject bombombLauncher = GameObject.Find("BombombLauncher");
        if (bombombLauncher)
        {
            bombombLauncher.GetComponent<BombombController>().EnableAI(false);
        }
    }

    public void Highway2Reached()
    {
        GameObject bigEye = GameObject.Find("BigEye");
        Debug.Log(bigEye);
        if (bigEye)
        {
            bigEye.GetComponent<BigEyeController>().EnableAI(true);
        }
    }

    public void CheckpointReached()
    {
        StartCoroutine(CoCheckpointReached());
        levelState = LevelStates.Checkpoint;
        Vector3 wallLeftPos = wallLeft.transform.position;
        wallLeftPos.x = wallLeftXPos1;
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

        private IEnumerator FillEnemyHealthBar()
    {
        // get enemy max health for bar calculation
        int maxHealth = enemy.GetComponent<EnemyController>().maxHealth;
        // loop the sound and play the repeat clip we generated
        SoundManager.Instance.Play(enemy.GetComponent<EnemyController>().energyFullSound, true);
        // increment the enemy health bar with a slight delay between each bar
        for (int i = 1; i <= maxHealth; i++)
        {
            float bars = i / (float)maxHealth;
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.EnemyHealth, bars);
            yield return new WaitForSeconds(0.025f);
        }
        // stop playing the repeat sound
        SoundManager.Instance.Stop();
    }

    public void BossDefeated()
    {
        /* 
         * do anything required when the boss is defeated
         *
         * currently the weapon part is instantiated from the enemy controller
         * we could instantiate it here at a different location much like how in
         * the original game it spawns far above the player and falls to the floor
         *
         * EDIT
         * I have the weapon part as an object in the scene that will be activated
         * and fall to the ground upon the boss defeat - similar to the original game
         */
        // stop the music
        SoundManager.Instance.StopMusic();
        // hide the enemy health bar
        UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.EnemyHealth, false);
        // destroy all weapons
        GameManager.Instance.DestroyWeapons();
        // active weapon part
        weaponPart.SetActive(true);
    }

    private void WeaponPartCollected()
    {
        /*
         * this is our cue that the player has captured the weapon part and we
         * should signal the game manager to end the level, tally up the points, 
         * and move on to the level selection screen to pick another boss
         */
        // get start time
        startTime = Time.time;
        // play victory theme clip
        SoundManager.Instance.MusicSource.volume = 1f;
        SoundManager.Instance.PlayMusic(victoryThemeClip, false);
        // freeze the player and input
        GameManager.Instance.FreezePlayer(true);
        // switch state the player victory
        levelState = LevelStates.PlayerVictory;
    }
}
