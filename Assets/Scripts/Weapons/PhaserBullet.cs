using UnityEngine;

public class PhaserBullet : MonoBehaviour
{
    public Vector3 direction = Vector3.right;
    private Rigidbody2D rb;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable(){
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.position = transform.position;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        transform.position += direction * (PhaserWeapon.Instance.speed * Time.deltaTime);
        CheckBoundary();
    }

    void FixedUpdate()
    {
        CheckBoundary();
    }

    private void CheckBoundary()
    {
        float camX = 0f;
        float halfWidth = 9.17f;
        if (Camera.main != null){
            camX = Camera.main.transform.position.x;
            halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        }
        float rightLimit = camX + halfWidth;
        // Despawn bullet slightly before the screen edge so bullets never reach or touch off-screen enemies
        if (transform.position.x >= (rightLimit - 0.25f) || transform.position.x < (camX - halfWidth - 1f) || transform.position.y > 6f || transform.position.y < -6f){
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Critter")){
            Critter1 critter = collision.gameObject.GetComponent<Critter1>();
            if (critter != null && !critter.IsOnScreen()){
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(false);
        } else if (collision.gameObject.CompareTag("Boss")){
            HandleBossHit(collision.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BossProjectile>() != null){
            Destroy(other.gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (other.CompareTag("Boss")){
            HandleBossHit(other.gameObject);
        }
    }

    private void HandleBossHit(GameObject bossObj)
    {
        int dmg = PhaserWeapon.Instance != null ? PhaserWeapon.Instance.damage : 1;
        Boss3 boss3 = bossObj.GetComponentInParent<Boss3>();
        Boss2 boss2 = bossObj.GetComponentInParent<Boss2>();
        Boss1 boss1 = bossObj.GetComponentInParent<Boss1>();

        if (boss3 != null){
            if (boss3.IsOnScreen()){
                boss3.TakeDamage(dmg);
            }
        } else if (boss2 != null){
            if (boss2.IsOnScreen()){
                boss2.TakeDamage(dmg);
            }
        } else if (boss1 != null){
            if (boss1.IsOnScreen()){
                boss1.TakeDamage(dmg);
            }
        }
        gameObject.SetActive(false);
    }
}
