using System.Collections; //Use coroutines
using UnityEngine; //Connect to unity engine

public class ProjectileHandler : MonoBehaviour
{
    #region Variables
    //Speed of projectile to shoot at fiends.
    private float _speed = 50f;
    //Direction of projectile movement
    public Vector3 movementDir;
    //Reference to duck object and GameManager
    [HideInInspector] public GameObject duck;
    [HideInInspector] public GameManager gameManager;
    //Reference to character controller
    [SerializeField] private CharacterController _charC;
    //Bool to check if we've hit a fiend to stop projectile being destroyed before fiend
    private bool _hitFiend = false;
    #endregion
    #region Setup & Movement
    private void Awake()
    {
        //At start set correct rotation and get character controller
        _charC = GetComponent<CharacterController>();        
    }
    private void Start()
    {
        //Set rotation to duck rotation minus 45f
        Quaternion.LookRotation(duck.transform.forward - new Vector3(0f, 45f, 0f));
    }
    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.InGame)
        {
            //Move in direction spawned
            _charC.Move(transform.forward * _speed * Time.deltaTime);
            //If we are too far away from base kill this object
            if (Vector3.Distance(transform.position, duck.transform.position) > 150f) Destroy(gameObject);
        }
    }
    #endregion
    #region Trigger & Collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Fiend")
        {
            //store the fiendhandler class so we can modify its colliders
            FiendHandler fiend = other.gameObject.GetComponent<FiendHandler>();
            //Change rigidbodies and colliders to enable ragdoll and disable animator and mark the fiend as hit
            fiend.mainCollider.enabled = false;
            fiend.mainRigidbody.isKinematic = true;
            fiend.animator.enabled = false;
            for (int i = 0; i < fiend.limbColliders.Length; i++)
            {
                fiend.limbColliders[i].enabled = true;
                fiend.limbRigidbodies[i].isKinematic = false;
            }
            //Set fiend hit bool so fiend stops when hit
            fiend.hit = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //If projectile collides with fiend determine which part of the fiend was hit and adjust score accordingly
        if (collision.collider.CompareTag("Fiend"))
        {
            //Grab FiendHandler script
            FiendHandler fiend = collision.gameObject.GetComponentInParent<FiendHandler>();
            //Set hitFiend to true so projectile can't die before coroutine ends
            _hitFiend = true;
            if (collision.collider.name == "mixamorig:Head")
            {
                collision.rigidbody.AddExplosionForce(10000f, transform.position, 10f);
                gameManager.points += 50;
                gameManager.fiendHit = "Head";
                gameManager.fiendPoints = 50;
            }
            else if (collision.collider.name == "mixamorig:Hips" || collision.collider.name == "mixamorig:Spine1")
            {
                collision.rigidbody.AddExplosionForce(10000f, transform.position, 10f);
                gameManager.points += 10;
                gameManager.fiendHit = "Torso";
                gameManager.fiendPoints = 10;
            }
            else if (collision.collider.name == "mixamorig:LeftArm" || collision.collider.name == "mixamorig:LeftForeArm" || collision.collider.name == "mixamorig:RightArm" || collision.collider.name == "mixamorig:RightForeArm")
            {
                collision.rigidbody.AddExplosionForce(10000f, transform.position, 10f);
                gameManager.points += 20;
                gameManager.fiendHit = "Arm";
                gameManager.fiendPoints = 20;
            }
            else if (collision.collider.name == "mixamorig:LeftUpLeg" || collision.collider.name == "mixamorig:LeftLeg" || collision.collider.name == "mixamorig:RightLeg" || collision.collider.name == "mixamorig:RightUpLeg")
            {
                collision.rigidbody.AddExplosionForce(10000f, transform.position, 10f);
                gameManager.points += 20;
                gameManager.fiendHit = "Leg";
                gameManager.fiendPoints = 20;
            }
            //Disable colliders so we can't hit same object twice
            foreach (Collider coll in collision.gameObject.GetComponentInParent<FiendHandler>().limbColliders) coll.enabled = false;
            //Start coroutine to destroy fiend after collision
            StartCoroutine(DestroyFiend(fiend));
        }
    }
    private IEnumerator DestroyFiend(FiendHandler fiend)
    {
        //Wait for a second so we can see ragdoll flying
        yield return new WaitForSeconds(1f);        
        Debug.Log(fiend.gameObject.name);
        //Search fiend list and remove hit fiend from list
        for (int i = 0; i < gameManager.fiends.Count; i++)
        {
            if (fiend.gameObject == gameManager.fiends[i]) gameManager.fiends.RemoveAt(i);
        }
        //Destroy fiend object and set hitfiend back to false so projectile will die
        Destroy(fiend.gameObject);
        Destroy(gameObject);
    }
    #endregion
}
