using System.Collections;
using UnityEngine;

public class Boss3 : MonoBehaviour
{
    private Animator animator;
    private FlashWhite flashWhite;
    private SpriteRenderer spriteRenderer;

    private float speedX;
    private float speedY;
    private bool charging;
    private bool isEntering = true;

    private float switchTimer;

    [SerializeField] private int lives = 280;
    private int maxLives = 280;
    [SerializeField] private int damage = 3;
    [SerializeField] private GameObject destroyEffect;

    private bool isEnraged = false;
    private bool isPhase2 = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashWhite = GetComponent<FlashWhite>();
        if (flashWhite == null){
            flashWhite = gameObject.AddComponent<FlashWhite>();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }

        // Ensure boss front nose collider is present and tightly fitted
        CircleCollider2D[] cols = GetComponents<CircleCollider2D>();
        bool hasFrontCollider = false;
        foreach (var c in cols){
            if (c.offset.y < -0.3f) hasFrontCollider = true;
        }
        if (!hasFrontCollider){
            CircleCollider2D frontCol = gameObject.AddComponent<CircleCollider2D>();
            frontCol.offset = new Vector2(0f, -0.6f);
            frontCol.radius = 0.55f;
        }

        // Colossal Apex Boss aesthetic: glowing purple/gold celestial titan aura
        if (spriteRenderer != null){
            spriteRenderer.color = new Color(1f, 0.3f, 0.7f, 1f);
        }
        transform.localScale = new Vector3(1.6f, 1.6f, 1f);

        float bossMult = GameManager.Instance != null ? GameManager.Instance.GetBossHealthMultiplier() : 1f;
        maxLives = Mathf.RoundToInt(Mathf.Max(lives, 280) * bossMult);
        lives = maxLives;

        EnterEntranceState();

        int cycle = GameManager.Instance != null ? GameManager.Instance.bossCycleCount : 0;
        string bossTitle = cycle > 0 ? string.Format("THE VOID APEX [MK {0} TITAN]", cycle + 1) : "THE VOID APEX [STRONGEST TITAN]";

        if (UIController.Instance != null){
            UIController.Instance.ShowBossBar(bossTitle, maxLives);
        }

        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.8f, 0.35f);
        }

        if (AudioManager.Instance != null){
            AudioManager.Instance.PlaySound(AudioManager.Instance.bossSpawn);
        }
    }

    void Update()
    {
        if (PlayerController.Instance == null) return;
        float playerPosition = PlayerController.Instance.transform.position.x;
        float playerPosY = PlayerController.Instance.transform.position.y;

        // Phase transitions
        float healthPct = (float)lives / maxLives;
        if (healthPct <= 0.60f && !isPhase2){
            isPhase2 = true;
            TriggerPhaseTransition("<color=#F472B6>PHASE 2: WARP TRAJECTORY</color>");
        }
        if (healthPct <= 0.30f && !isEnraged){
            isEnraged = true;
            TriggerPhaseTransition("<color=#EF4444>PHASE 3: ENRAGED APEX SINGULARITY</color>");
        }

        if (isEntering){
            if (transform.position.x <= 7.0f){
                isEntering = false;
                EnterPatrolState();
            }
        } else {
            if (switchTimer > 0){
                switchTimer -= Time.deltaTime;
            } else {
                if (charging && transform.position.x > playerPosition){
                    EnterPatrolState();
                } else {
                    EnterChargeState();
                }
            }
        }

        if (!charging && !isEntering){
            float targetDirY = Mathf.Sign(playerPosY - transform.position.y);
            float trackSpeed = isEnraged ? 2.47f : 1.71f;
            speedY = Mathf.MoveTowards(speedY, targetDirY * trackSpeed, 5f * Time.deltaTime);
        } else if (charging) {
            float targetDirY = Mathf.Clamp(playerPosY - transform.position.y, -2.0f, 2.0f);
            speedY = targetDirY * 0.7125f;
        }

        if (transform.position.y > 3.4f || transform.position.y < -3.4f){
            speedY *= -0.85f;
        } else if (transform.position.x < playerPosition && !charging && !isEntering){
            EnterChargeState();
        }

        bool boost = PlayerController.Instance.boosting;
        float moveX;
        if (boost && !charging && !isEntering){
            moveX = GameManager.Instance.worldSpeed * Time.deltaTime * -0.5f;
        } else {
            moveX = speedX * Time.deltaTime;
        }
        float moveY = speedY * Time.deltaTime;

        transform.position += new Vector3(moveX, moveY);
        if (transform.position.x < -15){
            transform.position = new Vector3(10.5f, Random.Range(-3f, 3f), 0f);
            EnterChargeState();
        }
    }

    void EnterEntranceState(){
        isEntering = true;
        charging = false;
        speedX = -3.8f;
        speedY = 0f;
        switchTimer = 2.0f;
        if (animator != null) animator.SetBool("charging", false);
    }

    void EnterPatrolState(){
        speedX = -0.285f;
        speedY = Random.Range(-1.425f, 1.425f);
        switchTimer = isEnraged ? Random.Range(3f, 5f) : Random.Range(4.5f, 7.5f);
        charging = false;
        if (animator != null) animator.SetBool("charging", false);
    }

    void EnterChargeState(){
        if (!charging && AudioManager.Instance != null){
            AudioManager.Instance.PlaySound(AudioManager.Instance.bossCharge);
        }
        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.4f, 0.25f);
        }
        speedX = isEnraged ? -6.65f : -5.51f;
        switchTimer = Random.Range(1.3f, 2.0f);
        charging = true;
        if (animator != null) animator.SetBool("charging", true);
    }

    private void TriggerPhaseTransition(string message){
        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.6f, 0.3f);
        }
        if (AudioManager.Instance != null){
            AudioManager.Instance.PlaySound(AudioManager.Instance.bossSpawn);
        }
        if (spriteRenderer != null && isEnraged){
            spriteRenderer.color = new Color(1f, 0.15f, 0.15f, 1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")){
            float dist = Vector2.Distance(transform.position, collision.transform.position);
            // Strictly reject collision if not in physical contact
            if (dist > 3.6f) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(damage);
        }
    }

    public bool IsOnScreen(){
        float camX = 0f;
        float halfWidth = 9.17f;
        if (Camera.main != null){
            camX = Camera.main.transform.position.x;
            halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        }
        float rightLimit = camX + halfWidth;
        float leftLimit = camX - halfWidth;

        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (var col in cols){
            if (col != null && col.enabled && col.bounds.min.x <= rightLimit && col.bounds.max.x >= leftLimit){
                return true;
            }
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.enabled && sr.bounds.min.x <= rightLimit && sr.bounds.max.x >= leftLimit){
            return true;
        }
        return transform.position.x <= (rightLimit + 1.5f) && transform.position.x >= (leftLimit - 2f);
    }

    public void TakeDamage(int damageTaken){
        if (lives <= 0) return;
        if (!IsOnScreen()) return;

        lives -= damageTaken;
        if (AudioManager.Instance != null){
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.hitArmor);
        }

        if (flashWhite != null){
            flashWhite.Flash();
        }

        if (UIController.Instance != null){
            UIController.Instance.UpdateBossHealth(lives, maxLives);
        }

        if (lives <= 0){
            Die();
        }
    }

    private void Die(){
        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(1.2f, 0.5f);
        }
        if (AudioManager.Instance != null){
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.boom2);
        }
        if (destroyEffect != null){
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }
        if (UIController.Instance != null){
            UIController.Instance.HideBossBar();
        }
        if (GameManager.Instance != null){
            GameManager.Instance.AddScore(15000);
            GameManager.Instance.BossDefeated(3);
        }
        Destroy(gameObject);
    }
}
