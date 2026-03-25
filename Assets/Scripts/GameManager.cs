using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager: MonoBehaviour
{
    public static GameManager Instance = null;
    
    bool isGameOver;
    bool playerReady;
    bool initReadyScreen;

    int playerScore;
    float gameRestartTime;
    float gamePlayerReadyTime;

    public float gameRestartDelay = 5f;
    public float playerReadyDelay = 3f;

    TextMeshProUGUI playerScoreText;
    TextMeshProUGUI screenMessageText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if(playerReady)
        {
            if (initReadyScreen)
            {
                FreezePlayer(true);
                FreezeEnemies(true);
                screenMessageText.alignment = TextAlignmentOptions.Center;
                screenMessageText.alignment = TextAlignmentOptions.Top;
                screenMessageText.fontStyle = FontStyles.UpperCase;
                screenMessageText.fontSize = 24;
                screenMessageText.text = "\n\n\n\nREADY";
                initReadyScreen = false;
            }

            gamePlayerReadyTime -= Time.deltaTime;
            if (gamePlayerReadyTime <= 0f)
            {
                screenMessageText.text = "";
                FreezePlayer(false);
                FreezeEnemies(false);
                playerReady = false;
            }
            return;
        }

        if(playerScoreText != null)
        {
            playerScoreText.text = string.Format("<mspace=\"{0}\">{1:D7}</mspace>", playerScoreText.fontSize, playerScore);
        }

        if (!isGameOver)
        {
            RepositionEnemies();
        } else
        {
            gameRestartTime -= Time.deltaTime;
            if (gameRestartTime <= 0f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }

    private void StartGame()
    {
        isGameOver = false;
        playerReady = true;
        initReadyScreen = true;
        gamePlayerReadyTime = playerReadyDelay;
        playerScoreText = GameObject.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        screenMessageText = GameObject.Find("ScreenMessage").GetComponent<TextMeshProUGUI>();
        SoundManager.Instance.MusicSource.Play();
    }

    public void AddScorePoints(int points)
    {
        playerScore += points;
    }

    private void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            player.GetComponent<PlayerController>().FreezeInput(freeze);
            player.GetComponent<PlayerController>().FreezePlayer(freeze);
        }
    }

    private void FreezeEnemies(bool freeze)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyController>().FreezeEnemy(freeze);
        }
    }

    private void FreezeBullets(bool freeze)
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            bullet.GetComponent<Bullet>().Freeze(freeze);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        gameRestartTime = gameRestartDelay;
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.Stop();
        FreezePlayer(true); 
        FreezeEnemies(true);

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (GameObject explosion in explosions)
        {
            Destroy(explosion);
        }

        screenMessageText.alignment = TextAlignmentOptions.Center;
        screenMessageText.alignment = TextAlignmentOptions.Top;
        screenMessageText.fontStyle = FontStyles.UpperCase;
        screenMessageText.fontSize = 24;
        // screenMessageText.text = "\n\n\n\nGAME OVER"; 
    }

    private void RepositionEnemies()
    {
        Vector3 worldLeft = Camera.main.ViewportToWorldPoint(new Vector3(-0.1f, 0, 0));
        Vector3 worldRight = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0, 0));

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)        {
            Vector3 enemyPos = enemy.transform.position;
            if (enemyPos.x < worldLeft.x)
            {
                switch (enemy.name)
                {
                    case "KillerBomb":
                        enemy.transform.position = new Vector3(worldRight.x, UnityEngine.Random.Range(-1.5f, 1.5f), enemyPos.z);
                        enemy.GetComponent<KillerBombController>().ResetFollowingPath();
                        break;
                    case "Pepe":
                        enemy.transform.position = new Vector3(worldRight.x, UnityEngine.Random.Range(-1f, 1f), enemyPos.z);
                        enemy.GetComponent<PepeController>().ResetFollowingPath();
                        break;
                }
            }
        }
    }
}
