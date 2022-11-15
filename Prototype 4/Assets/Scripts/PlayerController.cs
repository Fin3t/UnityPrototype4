using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private GameObject focalPoint;
    private float powerUpStrength = 15.0f;
    public float speed = 5.0f;
    public bool hasPowerup = false;
    public GameObject powerupIndicator;
    public bool isOnGround = true;
    public float mass;
    public bool hasCooldownOnSmash;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
    }

    // Update is called once per frame
    void Update()
    {
        //Verfolgung der Spährenkugel
        powerupIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);
        
        

        //Spielersteuerung mit W,A,S,D und Pfeiltasten
        if (isOnGround)
        {
            
            float forwardInput = Input.GetAxis("Vertical");
            //Einstellung der Geschwindigkeit
            playerRb.AddForce(focalPoint.transform.forward * forwardInput * speed);

        }

        //Jump with Space, freeze the player when in air and crash player to ground

        if (Input.GetKeyDown(KeyCode.Alpha1) && isOnGround && !hasCooldownOnSmash)
        {
            StartCoroutine(SmashAttack());
        }
    }
    //Logik für Powerup
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            powerupIndicator.gameObject.SetActive(true);
            //Löschung des Powerups
            Destroy(other.gameObject);
            //Start der Coroutine (Löschung des Powerups nach 7 Sekunden)
            StartCoroutine(PowerupCountdownRoutine());
        }
    }
    //Countdown für Powerup
    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }
    IEnumerator SmashAttack()
    {
       
            
            playerRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            playerRb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            playerRb.mass = 1000;
            isOnGround = false;
            yield return new WaitForSeconds(1);
            playerRb.AddForce(Vector3.down * 100000, ForceMode.Impulse);

            yield return new WaitUntil(() => transform.position.y <= 0.1f);
            
            playerRb.mass = 1;
            isOnGround = true;
            playerRb.constraints = RigidbodyConstraints.None;
            foreach (var enemy in FindObjectsOfType<Enemy>())
            {
                float distanceFromEnemy = Vector3.Distance(enemy.transform.position, transform.position);
                Vector3 moveDirection = (-transform.position + enemy.transform.position).normalized;
                enemy.gameObject.GetComponent<Rigidbody>().AddForce(moveDirection * (25 - distanceFromEnemy), ForceMode.Impulse);
            }
            hasCooldownOnSmash = true;
            yield return new WaitForSeconds(10);
            hasCooldownOnSmash = false;
        
        
    }

    //Logik für die Kollision mit dem Gegner
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = (collision.gameObject.transform.position - transform.position);

            enemyRigidbody.AddForce(awayFromPlayer * powerUpStrength, ForceMode.Impulse);
        }
    }
}
