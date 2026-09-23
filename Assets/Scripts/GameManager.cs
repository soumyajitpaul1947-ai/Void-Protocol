using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float worldSpeed;

    public int critterCounter;
    public float survivalTime;
    public int score = 0;
    public int combo = 0;
    private float comboTimer = 0f;

    [Header("Endless Boss Wave Progression")]
    public float boss1SpawnTime = 45f;
    public float boss2SpawnTime = 100f;
    public float boss3SpawnTime = 150f;

    [System.NonSerialized] public float distance = 0f;
    [System.NonSerialized] public int bossCycleCount = 0;

    [SerializeField] private GameObject boss1;
    private bool boss1Spawned = false;
    private bool boss2Spawned = false;
    private bool boss3Spawned = false;

    private bool isGameOver = false;

    void Awake(){
        if (Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start(){
        critterCounter = 0;
        survivalTime = 0f;
        distance = 0f;
        score = 0;
        combo = 0;
        comboTimer = 0f;
        bossCycleCount = 0;
        boss1Spawned = false;
        boss2Spawned = false;
        boss3Spawned = false;
        isGameOver = false;

        int boundaryLayer = LayerMask.NameToLayer("LevelBoundaries");
        int critterLayer = LayerMask.NameToLayer("Critter");
        int bulletLayer = LayerMask.NameToLayer("Bullet");
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        int bossLayer = LayerMask.NameToLayer("Boss");

        int playerLayer = LayerMask.NameToLayer("Player");

        if (boundaryLayer != -1){
            if (critterLayer != -1) Physics2D.IgnoreLayerCollision(boundaryLayer, critterLayer, true);
            if (bulletLayer != -1) Physics2D.IgnoreLayerCollision(boundaryLayer, bulletLayer, true);
            if (obstacleLayer != -1) Physics2D.IgnoreLayerCollision(boundaryLayer, obstacleLayer, true);
            if (bossLayer != -1) Physics2D.IgnoreLayerCollision(boundaryLayer, bossLayer, true);
        }
        if (playerLayer != -1 && bulletLayer != -1){
            Physics2D.IgnoreLayerCollision(playerLayer, bulletLayer, true);
        }
        if (critterLayer != -1){
            Physics2D.IgnoreLayerCollision(critterLayer, critterLayer, true);
        }

        AdjustLevelBoundaries();
    }

    private void AdjustLevelBoundaries(){
        GameObject boundariesGo = GameObject.Find("LevelBoundaries");
        if (boundariesGo == null) return;

        BoxCollider2D[] colliders = boundariesGo.GetComponents<BoxCollider2D>();
        float halfWidth = Camera.main != null ? (Camera.main.orthographicSize * Camera.main.aspect) : 9.17f;
        float halfHeight = Camera.main != null ? Camera.main.orthographicSize : 5.0f;

        foreach (var col in colliders){
            if (col.offset.y > 2f){
                col.offset = new Vector2(0f, halfHeight + 0.5f);
                col.size = new Vector2(halfWidth * 2f + 4f, 1f);
            } else if (col.offset.y < -2f){
                col.offset = new Vector2(0f, -halfHeight - 0.5f);
                col.size = new Vector2(halfWidth * 2f + 4f, 1f);
            } else if (col.offset.x > 2f){
                col.offset = new Vector2(halfWidth + 0.5f, 0f);
                col.size = new Vector2(1f, halfHeight * 2f + 4f);
            } else if (col.offset.x < -2f){
                col.offset = new Vector2(-halfWidth - 0.5f, 0f);
                col.size = new Vector2(1f, halfHeight * 2f + 4f);
            }
        }
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("Fire3")){
            Pause();
        }

        if (!isGameOver){
            survivalTime += Time.deltaTime;
            distance += Mathf.Max(0f, worldSpeed) * 15f * Time.deltaTime;

            if (comboTimer > 0){
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0){
                    combo = 0;
                }
            }

            if (UIController.Instance != null){
                UIController.Instance.UpdateSurvivalTime(survivalTime, distance);
                UIController.Instance.UpdateScoreAndCombo(score, combo);
            }

            // Recurring Boss Spawning Timeline
            if (survivalTime >= boss1SpawnTime && !boss1Spawned){
                SpawnBoss1();
            }
            if (survivalTime >= boss2SpawnTime && !boss2Spawned){
                SpawnBoss2();
            }
            if (survivalTime >= boss3SpawnTime && !boss3Spawned){
                SpawnBoss3();
            }
        }
    }

    public float GetDifficultyMultiplier(){
        return 1f + Mathf.Min(survivalTime / 60f, 3f) * 0.75f;
    }

    public float GetBossHealthMultiplier(){
        // Smoothly scales boss health based on survival time, distance traveled, and completed boss cycles
        float timeFactor = (survivalTime / 90f) * 0.25f;
        float distFactor = (distance / 3000f) * 0.20f;
        float cycleFactor = bossCycleCount * 0.35f;
        return 1f + timeFactor + distFactor + cycleFactor;
    }

    public float GetBuffPowerMultiplier(){
        // Buff potency scales up as survival time and distance increase
        float timeFactor = (survivalTime / 100f) * 0.40f;
        float distFactor = (distance / 2500f) * 0.35f;
        return Mathf.Clamp(1f + timeFactor + distFactor, 1f, 2.5f);
    }

    public void AddScore(int amount){
        score += amount * Mathf.Max(1, combo);
    }

    public void AddCritterKill(){
        critterCounter++;
        combo++;
        comboTimer = 3.5f;
        AddScore(100);
    }

    private void SpawnBoss1(){
        boss1Spawned = true;
        if (boss1 != null){
            Instantiate(boss1, new Vector2(13.0f, 0), Quaternion.Euler(0, 0, -90));
        }
    }

    private void SpawnBoss2(){
        boss2Spawned = true;
        if (boss1 != null){
            GameObject b2 = Instantiate(boss1, new Vector2(13.0f, 0), Quaternion.Euler(0, 0, -90));
            Boss1 orig = b2.GetComponent<Boss1>();
            if (orig != null) Destroy(orig);
            b2.AddComponent<Boss2>();
        }
    }

    private void SpawnBoss3(){
        boss3Spawned = true;
        if (boss1 != null){
            GameObject b3 = Instantiate(boss1, new Vector2(13.0f, 0), Quaternion.Euler(0, 0, -90));
            Boss1 orig = b3.GetComponent<Boss1>();
            if (orig != null) Destroy(orig);
            b3.AddComponent<Boss3>();
        }
    }

    public void BossDefeated(){
        BossDefeated(3);
    }

    public void BossDefeated(int tier){
        if (UIController.Instance != null){
            UIController.Instance.HideBossBar();
        }

        if (tier == 1){
            AddScore(5000);
            if (PlayerController.Instance != null){
                PlayerController.Instance.Heal(2);
                PlayerController.Instance.AddEnergy(50);
            }
        } else if (tier == 2){
            AddScore(10000);
            if (PlayerController.Instance != null){
                PlayerController.Instance.Heal(3);
                PlayerController.Instance.AddEnergy(60);
                PlayerController.Instance.SpeedSurge(8f);
            }
        } else if (tier == 3){
            AddScore(25000);
            bossCycleCount++;

            if (PlayerController.Instance != null){
                PlayerController.Instance.Heal(3);
                PlayerController.Instance.AddEnergy(100);
                PlayerController.Instance.ActivateTemporaryShield(8f);
                PlayerController.Instance.PlayBuffAnimation("TITAN DEFEATED! ODYSSEY CONTINUES!");
            }

            // Schedule next boss escalation cycle in the infinite run
            boss1Spawned = false;
            boss2Spawned = false;
            boss3Spawned = false;
            boss1SpawnTime = survivalTime + 50f;
            boss2SpawnTime = survivalTime + 105f;
            boss3SpawnTime = survivalTime + 160f;
        }
    }

    public void Pause(){
        if (UIController.Instance.pausePanel.activeSelf == false){
            UIController.Instance.pausePanel.SetActive(true);
            UIController.Instance.OnPauseStateChanged(true);
            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(AudioManager.Instance.pause);
        } else {
            UIController.Instance.pausePanel.SetActive(false);
            UIController.Instance.OnPauseStateChanged(false);
            Time.timeScale = 1f;
            PlayerController.Instance.ExitBoost();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(AudioManager.Instance.unpause);
        }
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void GoToMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }

    public void GameOver(){
        if (isGameOver) return;
        isGameOver = true;

        PlayerPrefs.SetFloat("FinalSurvivalTime", survivalTime);
        PlayerPrefs.SetFloat("FinalDistance", distance);
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();

        StartCoroutine(ShowGameOverScreen());
    }

    IEnumerator ShowGameOverScreen(){
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("GameOver");
    }

    public void SetWorldSpeed(float speed){
        worldSpeed = speed;
    }
}
