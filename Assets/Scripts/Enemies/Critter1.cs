using UnityEngine;

public class Critter1 : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;

    private float moveSpeed;
    private float waveFrequency;
    private float waveAmplitude;
    private float waveOffset;

    [SerializeField] private GameObject zappedEffect;
    [SerializeField] private GameObject burnEffect;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprites != null && sprites.Length > 0){
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        }

        float difficulty = GameManager.Instance != null ? GameManager.Instance.GetDifficultyMultiplier() : 1f;
        moveSpeed = Random.Range(2f, 4.5f) * difficulty;
        waveFrequency = Random.Range(1.5f, 3.5f) * Mathf.Min(difficulty, 1.4f);
        waveAmplitude = Random.Range(1.5f, 3f);
        waveOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float deltaX = -(moveSpeed + GameManager.Instance.worldSpeed) * Time.deltaTime;
        float deltaY = Mathf.Cos(Time.time * waveFrequency + waveOffset) * waveAmplitude * Time.deltaTime;

        Vector3 moveDelta = new Vector3(deltaX, deltaY, 0f);
        transform.position += moveDelta;

        if (moveDelta != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, moveDelta);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);
        }

        if (transform.position.x < -11.5f){
            Destroy(gameObject);
        }
    }

    public bool IsOnScreen(){
        float camX = 0f;
        float rightLimit = 8.8f;
        if (Camera.main != null){
            camX = Camera.main.transform.position.x;
            rightLimit = camX + (Camera.main.orthographicSize * Camera.main.aspect);
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            // Enemy must be clearly inside viewport
            if (vp.x > 0.88f || vp.x < -0.05f || vp.y < -0.05f || vp.y > 1.05f) return false;
        }
        return transform.position.x <= (rightLimit - 1.0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet")){
            if (!IsOnScreen()){
                // Bullet harmlessly absorbed while enemy enters the screen
                collision.gameObject.SetActive(false);
                return;
            }

            if (AudioManager.Instance != null){
                AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.squished);
            }
            Instantiate(zappedEffect, transform.position, transform.rotation);
            TryDropBuff();
            Destroy(gameObject);
            GameManager.Instance.AddCritterKill();
        } else if (collision.gameObject.CompareTag("Player")){
            if (!IsOnScreen()) return;

            if (AudioManager.Instance != null){
                AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.burn);
            }
            Instantiate(burnEffect, transform.position, transform.rotation);
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null){
                if (!player.boosting){
                    player.TakeDamage(1);
                } else {
                    TryDropBuff();
                }
            }
            Destroy(gameObject);
            GameManager.Instance.AddCritterKill();
        }
    }

    private void TryDropBuff(){
        // Buff drops reduced by 5% (now 11% chance) with potency scaling over time & distance
        if (Random.value < 0.11f){
            float buffPower = GameManager.Instance != null ? GameManager.Instance.GetBuffPowerMultiplier() : 1f;
            float roll = Random.value;
            if (roll < 0.20f){
                // Temporary Invulnerability Shield (scales from 6s up to 10s)
                if (PlayerController.Instance != null){
                    float duration = 6f * (1f + (buffPower - 1f) * 0.45f);
                    PlayerController.Instance.ActivateTemporaryShield(duration);
                }
            } else if (roll < 0.40f){
                // Progressive Gun Upgrade (Double Shot -> Triple Shot!)
                if (PhaserWeapon.Instance != null){
                    PhaserWeapon.Instance.UpgradeGunProgressive();
                }
            } else if (roll < 0.55f){
                // Rapid Fire & Bullet Velocity Boost
                if (PhaserWeapon.Instance != null){
                    PhaserWeapon.Instance.UpgradeRapidFire();
                }
            } else if (roll < 0.70f){
                // Hull Repair (+1 HP base, +2 HP when buffPower >= 1.4)
                if (PlayerController.Instance != null){
                    int healAmount = buffPower >= 1.4f ? 2 : 1;
                    PlayerController.Instance.Heal(healAmount);
                }
            } else if (roll < 0.85f){
                // Energy Shield Recharge (+35 Energy base, scaling up to 85+)
                if (PlayerController.Instance != null){
                    float energyAmount = Mathf.Round(35f * buffPower);
                    PlayerController.Instance.AddEnergy(energyAmount);
                }
            } else {
                // Thruster Speed Surge (scales from 5s up to 9s)
                if (PlayerController.Instance != null){
                    float surgeDuration = 5f * (1f + (buffPower - 1f) * 0.5f);
                    PlayerController.Instance.SpeedSurge(surgeDuration);
                }
            }
        }
    }
}
