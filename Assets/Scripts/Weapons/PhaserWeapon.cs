using UnityEngine;

public class PhaserWeapon : MonoBehaviour
{
    public static PhaserWeapon Instance;

    //[SerializeField] private GameObject prefab;
    [SerializeField] private ObjectPooler bulletPool;

    public float speed;
    public int damage;

    public int bulletsCount = 1;
    public float shootCooldown = 0.22f;
    private float lastShootTime = -1f;

    void Awake(){
        if (Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        if (damage <= 0) damage = 1;
        if (speed <= 0) speed = 12f;
    }

    public void Shoot(){
        if (Time.time - lastShootTime < shootCooldown) return;
        lastShootTime = Time.time;

        if (AudioManager.Instance != null){
            AudioManager.Instance.PlayModifiedSound(AudioManager.Instance.shoot);
        }

        if (bulletsCount <= 1){
            FireBullet(transform.position, Quaternion.identity, Vector3.right);
        } else if (bulletsCount == 2){
            FireBullet(transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity, Vector3.right);
            FireBullet(transform.position + new Vector3(0, -0.25f, 0), Quaternion.identity, Vector3.right);
        } else {
            // Triple Shot: 3 bullets with fanned trajectory
            FireBullet(transform.position, Quaternion.identity, Vector3.right);
            FireBullet(transform.position + new Vector3(0, 0.32f, 0), Quaternion.Euler(0, 0, 7f), new Vector3(0.99f, 0.12f, 0f).normalized);
            FireBullet(transform.position + new Vector3(0, -0.32f, 0), Quaternion.Euler(0, 0, -7f), new Vector3(0.99f, -0.12f, 0f).normalized);
        }
        Physics2D.SyncTransforms();
    }

    private void FireBullet(Vector3 spawnPos, Quaternion rot, Vector3 dir){
        GameObject bullet = bulletPool.GetPooledObject();
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = rot;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null){
            rb.position = spawnPos;
            rb.linearVelocity = Vector2.zero;
        }
        PhaserBullet pb = bullet.GetComponent<PhaserBullet>();
        if (pb != null) pb.direction = dir;
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol != null && PlayerController.Instance != null){
            Collider2D[] pCols = PlayerController.Instance.GetComponents<Collider2D>();
            foreach (var pc in pCols){
                Physics2D.IgnoreCollision(pc, bulletCol, true);
            }
        }
        bullet.SetActive(true);
    }

    public void UpgradeDoubleShot(){
        bulletsCount = 2;
        if (PlayerController.Instance != null){
            PlayerController.Instance.PlayBuffAnimation("DOUBLE SHOT!");
        }
    }

    public void UpgradeTripleShot(){
        bulletsCount = 3;
        if (PlayerController.Instance != null){
            PlayerController.Instance.PlayBuffAnimation("TRIPLE SHOT!");
        }
    }

    public void UpgradeGunProgressive(){
        if (bulletsCount <= 1){
            UpgradeDoubleShot();
        } else {
            UpgradeTripleShot();
        }
    }

    public void UpgradeRapidFire(){
        shootCooldown = Mathf.Max(0.09f, shootCooldown * 0.7f);
        speed = Mathf.Min(25f, speed * 1.25f);
        if (PlayerController.Instance != null){
            PlayerController.Instance.PlayBuffAnimation("RAPID FIRE!");
        }
    }
}
