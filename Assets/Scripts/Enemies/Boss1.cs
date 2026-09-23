using UnityEngine;

public class Boss1 : MonoBehaviour
{
    private Animator animator;
    private FlashWhite flashWhite;
    private float speedX;
    private float speedY;
    private bool charging;

    private float switchInterval;
    private float switchTimer;

    [SerializeField] private int lives = 100;
    private int maxLives = 100;
    [SerializeField] private int damage = 2;
    [SerializeField] private GameObject destroyEffect;

    void Start()
    {
        animator = GetComponent<Animator>();
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

        float bossMult = GameManager.Instance != null ? GameManager.Instance.GetBossHealthMultiplier() : 1f;
        maxLives = Mathf.RoundToInt(Mathf.Max(lives, 100) * bossMult);
        lives = maxLives;

        EnterChargeState();

        int cycle = GameManager.Instance != null ? GameManager.Instance.bossCycleCount : 0;
        string bossTitle = cycle > 0 ? string.Format("PROTOCOL GUARDIAN [MK {0}]", cycle + 1) : "PROTOCOL GUARDIAN [TIER 1]";

        if (UIController.Instance != null){
            UIController.Instance.ShowBossBar(bossTitle, maxLives);
        }

        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.35f, 0.18f);
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

        if (switchTimer > 0){
            switchTimer -= Time.deltaTime;
        } else {
            if (charging && transform.position.x > playerPosition){
                EnterPatrolState();
            } else {
                EnterChargeState();
            }
        }

        if (!charging){
            float targetDirY = Mathf.Sign(playerPosY - transform.position.y);
            speedY = Mathf.MoveTowards(speedY, targetDirY * 1.71f, 5f * Time.deltaTime);
        } else {
            float targetDirY = Mathf.Clamp(playerPosY - transform.position.y, -1.8f, 1.8f);
            speedY = targetDirY * 0.6175f;
        }

        if (transform.position.y > 3.2f || transform.position.y < -3.2f){
            speedY *= -0.8f;
        } else if (transform.position.x < playerPosition && !charging){
            EnterChargeState();
        }

        bool boost = PlayerController.Instance.boosting;
        float moveX;
        if (boost && !charging){
            moveX = GameManager.Instance.worldSpeed * Time.deltaTime * -0.5f;
        } else {
            moveX = speedX * Time.deltaTime;
        }
        float moveY = speedY * Time.deltaTime;

        transform.position += new Vector3(moveX, moveY);
        if (transform.position.x < -14){
            transform.position = new Vector3(10.5f, Random.Range(-3f, 3f), 0f);
            EnterChargeState();
        }
    }

    void EnterPatrolState(){
        speedX = 0;
        speedY = Random.Range(-1.425f, 1.425f);
        switchInterval = Random.Range(3f, 5.5f);
        switchTimer = switchInterval;
        charging = false;
        if (animator != null) animator.SetBool("charging", false);
    }

    void EnterChargeState(){
        if (!charging && AudioManager.Instance != null){
            AudioManager.Instance.PlaySound(AudioManager.Instance.bossCharge);
        }
        speedX = -5.225f;
        switchInterval = Random.Range(1.2f, 1.8f);
        switchTimer = switchInterval;
        charging = true;
        if (animator != null) animator.SetBool("charging", true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")){
            float dist = Vector2.Distance(transform.position, collision.transform.position);
            // Strictly reject collision if not in physical contact
            if (dist > 2.3f) return;

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
            CameraShake.Instance.Shake(0.5f, 0.25f);
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
            GameManager.Instance.AddScore(2500);
            GameManager.Instance.BossDefeated(1);
        }
        Destroy(gameObject);
    }
}

public class BossProjectile : MonoBehaviour
{
    public Vector3 direction = Vector3.left;
    public float speed = 8f;
    public int damage = 1;
    private float lifeTime = 5f;

    void Update(){
        transform.position += direction * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0 || transform.position.x < -14f || transform.position.x > 15f){
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null){
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        } else if (other.CompareTag("Bullet")){
            Destroy(gameObject);
        }
    }
}
