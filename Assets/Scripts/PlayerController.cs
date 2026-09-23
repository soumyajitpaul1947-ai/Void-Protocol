using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private Rigidbody2D rb;
    private Animator animator;
    private FlashWhite flashWhite;
    private SpriteRenderer spriteRenderer;

    private Vector2 playerDirection;
    [SerializeField] private float moveSpeed;
    public bool boosting = false;

    [SerializeField] private float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegen;

    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private ParticleSystem engineEffect;

    private bool isInvulnerable = false;
    private Coroutine buffAnimCoroutine;
    private Coroutine shieldCoroutine;
    private GameObject activeShieldVisual;

    void Awake(){
        if (Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        CircleCollider2D[] cols = GetComponents<CircleCollider2D>();
        bool hasFront = false;
        foreach (var c in cols){
            if (c.offset.x > 0.1f) hasFront = true;
        }
        if (!hasFront){
            CircleCollider2D frontCol = gameObject.AddComponent<CircleCollider2D>();
            frontCol.offset = new Vector2(0.2f, 0f);
            frontCol.radius = 0.26f;
        }

        animator = GetComponent<Animator>();
        flashWhite = GetComponent<FlashWhite>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (Camera.main != null && CameraShake.Instance == null){
            Camera.main.gameObject.AddComponent<CameraShake>();
        }
        energy = maxEnergy;
        UIController.Instance.UpdateEnergySlider(energy, maxEnergy);
        health = maxHealth;
        UIController.Instance.UpdateHealthSlider(health, maxHealth);
    }

    void Update()
    {
        if (Time.timeScale > 0){
            float directionX = Input.GetAxisRaw("Horizontal");
            float directionY = Input.GetAxisRaw("Vertical");

            animator.SetFloat("moveX", directionX);
            animator.SetFloat("moveY", directionY);
            
            playerDirection = new Vector2(directionX, directionY).normalized;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire2")){
                EnterBoost();
            } else if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Fire2")){
                ExitBoost();
            }

            if (Input.GetKey(KeyCode.RightShift) || Input.GetButton("Fire1") || Input.GetMouseButton(0)){
                PhaserWeapon.Instance.Shoot();
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)){
                TriggerEMPOverdrive();
            }
        }
    }

    public void TriggerEMPOverdrive(){
        if (energy < 25f) return;
        energy -= 25f;
        UIController.Instance.UpdateEnergySlider(energy, maxEnergy);

        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.45f, 0.25f);
        }
        if (AudioManager.Instance != null && AudioManager.Instance.boom2 != null){
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.boom2);
        }
        PlayBuffAnimation("+EMP OVERDRIVE!");

        BossProjectile[] enemyBullets = FindObjectsByType<BossProjectile>(FindObjectsSortMode.None);
        foreach (var b in enemyBullets){
            if (b != null) Destroy(b.gameObject);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 6.5f);
        foreach (var h in hits){
            if (h.CompareTag("Critter")){
                Critter1 critter = h.GetComponent<Critter1>();
                if (critter != null && !critter.IsOnScreen()) continue;
                Destroy(h.gameObject);
                if (GameManager.Instance != null) GameManager.Instance.AddCritterKill();
            } else if (h.CompareTag("Boss")){
                Boss1 b1 = h.GetComponent<Boss1>();
                if (b1 != null) b1.TakeDamage(15);
                Boss2 b2 = h.GetComponent<Boss2>();
                if (b2 != null) b2.TakeDamage(15);
                Boss3 b3 = h.GetComponent<Boss3>();
                if (b3 != null) b3.TakeDamage(15);
            }
        }
    }

    void FixedUpdate(){
        rb.linearVelocity = new Vector2(playerDirection.x * moveSpeed, playerDirection.y * moveSpeed);

        if (boosting){
            if (energy >= 0.5f) energy -= 0.5f;
            else {
                ExitBoost();
            }
        } else {
            if (energy < maxEnergy){
                energy += energyRegen;
            }
        }
        UIController.Instance.UpdateEnergySlider(energy, maxEnergy);
    }

    private void EnterBoost(){
        if (energy > 10){
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(AudioManager.Instance.fire);
            animator.SetBool("boosting", true);
            GameManager.Instance.SetWorldSpeed(7f);
            boosting = true;
            engineEffect.Play();
        }
    }

    public void ExitBoost(){
        animator.SetBool("boosting", false);
        GameManager.Instance.SetWorldSpeed(1f);
        boosting = false;
    }

    public void TakeDamage(int damage){
        if (health <= 0) return;
        if (isInvulnerable){
            if (AudioManager.Instance != null && AudioManager.Instance.hitArmor != null){
                AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.hitArmor);
            }
            return;
        }

        health -= damage;
        UIController.Instance.UpdateHealthSlider(health, maxHealth);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(AudioManager.Instance.hit);
        flashWhite.Flash();

        if (CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.35f, 0.2f);
        }

        if (health <= 0){
            ExitBoost();
            GameManager.Instance.SetWorldSpeed(0f);
            gameObject.SetActive(false);
            Instantiate(destroyEffect, transform.position, transform.rotation);
            GameManager.Instance.GameOver();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(AudioManager.Instance.ice);
        } else {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    IEnumerator InvulnerabilityRoutine(){
        isInvulnerable = true;
        Color normalColor = Color.white;
        Color transparentColor = new Color(1f, 1f, 1f, 0.3f);

        for (int i = 0; i < 6; i++){
            if (spriteRenderer != null) spriteRenderer.color = transparentColor;
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = normalColor;
            yield return new WaitForSeconds(0.12f);
        }

        if (spriteRenderer != null) spriteRenderer.color = normalColor;
        isInvulnerable = false;
    }

    public void Heal(int amount){
        if (health < maxHealth){
            health = Mathf.Min(health + amount, maxHealth);
            UIController.Instance.UpdateHealthSlider(health, maxHealth);
        }
        PlayBuffAnimation(string.Format("+{0} HEALTH!", amount));
    }

    public void AddEnergy(float amount){
        energy = Mathf.Min(energy + amount, maxEnergy);
        UIController.Instance.UpdateEnergySlider(energy, maxEnergy);
        PlayBuffAnimation(string.Format("+{0} ENERGY!", Mathf.RoundToInt(amount)));
    }

    public void SpeedSurge(float duration = 5f){
        StartCoroutine(SpeedSurgeRoutine(duration));
    }

    private IEnumerator SpeedSurgeRoutine(float duration){
        PlayBuffAnimation(duration > 5.5f ? string.Format("MEGA SURGE ({0:F0}s)!", duration) : "+THRUST SURGE!");
        float originalSpeed = moveSpeed;
        moveSpeed *= 1.35f;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }

    public void ActivateTemporaryShield(float duration = 6f){
        if (shieldCoroutine != null){
            StopCoroutine(shieldCoroutine);
        }
        shieldCoroutine = StartCoroutine(TemporaryShieldRoutine(duration));
    }

    private IEnumerator TemporaryShieldRoutine(float duration){
        PlayBuffAnimation(duration > 6.5f ? string.Format("SUPER SHIELD ({0:F0}s)!", duration) : "INVULNERABILITY SHIELD!");
        isInvulnerable = true;

        if (activeShieldVisual != null) Destroy(activeShieldVisual);

        // Create a glowing shield aura around player ship
        activeShieldVisual = new GameObject("PlayerForcefieldShield");
        activeShieldVisual.transform.SetParent(transform, false);
        activeShieldVisual.transform.localPosition = Vector3.zero;
        activeShieldVisual.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        SpriteRenderer sr = activeShieldVisual.AddComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null){
            sr.sprite = spriteRenderer.sprite;
        }
        sr.color = new Color(0.2f, 0.9f, 1f, 0.65f);
        sr.sortingOrder = 25;

        float elapsed = 0f;
        while (elapsed < duration){
            elapsed += Time.deltaTime;
            // Pulsate shield scale and alpha
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 9f);
            if (activeShieldVisual != null){
                activeShieldVisual.transform.localScale = new Vector3(2.4f * pulse, 2.4f * pulse, 1f);
                // In the last 1.8 seconds, flash rapidly in amber/gold to warn player
                if (duration - elapsed <= 1.8f){
                    float flash = Mathf.PingPong(Time.time * 14f, 1f);
                    sr.color = new Color(1f, 0.8f, 0.2f, 0.3f + flash * 0.5f);
                } else {
                    sr.color = new Color(0.2f, 0.9f, 1f, 0.5f + 0.25f * Mathf.Sin(Time.time * 6f));
                }
            }
            yield return null;
        }

        if (activeShieldVisual != null){
            Destroy(activeShieldVisual);
            activeShieldVisual = null;
        }
        isInvulnerable = false;
        shieldCoroutine = null;
    }

    public void PlayBuffAnimation(string buffText){
        if (buffAnimCoroutine != null) StopCoroutine(buffAnimCoroutine);
        buffAnimCoroutine = StartCoroutine(BuffAnimationRoutine(buffText));
    }

    IEnumerator BuffAnimationRoutine(string buffText){
        if (AudioManager.Instance != null && AudioManager.Instance.unpause != null){
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.unpause);
        }

        GameObject textObj = new GameObject("BuffText");
        textObj.transform.position = transform.position + new Vector3(0, 1.2f, 0);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = buffText;
        tmp.fontSize = 5;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.3f, 1f);
        tmp.sortingOrder = 50;

        Vector3 baseScale = Vector3.one;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration){
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scaleMult = 1f + 0.35f * Mathf.Sin(progress * Mathf.PI);
            transform.localScale = baseScale * scaleMult;

            if (textObj != null){
                textObj.transform.position += new Vector3(0, 1f * Time.deltaTime, 0);
                tmp.color = new Color(1f, 0.95f, 0.3f, 1f - progress * 0.8f);
            }
            yield return null;
        }

        transform.localScale = baseScale;
        if (textObj != null) Destroy(textObj);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBossCollision(collision.gameObject);
    }

    private void HandleBossCollision(GameObject obj)
    {
        if (obj.CompareTag("Boss")){
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            Boss3 b3 = obj.GetComponentInParent<Boss3>();
            Boss2 b2 = obj.GetComponentInParent<Boss2>();
            float maxAllowedDist = b3 != null ? 3.6f : (b2 != null ? 2.9f : 2.3f);
            if (dist > maxAllowedDist) return; // Ignore any far-away or ghost collision

            int dmg = b3 != null ? 3 : (b2 != null ? 2 : 2);
            TakeDamage(dmg);
        }
    }
}
