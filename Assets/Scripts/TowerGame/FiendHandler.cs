using System.Collections.Generic; //Allow use of lists
using UnityEngine; //Connect to Unity Engine

public class FiendHandler : MonoBehaviour
{
    #region Variables
    [Header("Movement")]
    [Tooltip("Set the speed you want the mob to move at")]
    [SerializeField] private float _speed = 5f;
    //Starting position for AStar calculation
    [HideInInspector] public Node startPos;
    //Target position for AStar calculation
    [HideInInspector] public Node target;
    //List to store waypoints to target
    private List<Node> _waypoints;
    //Reference to AStar script
    public AStar aStar;
    //Reference to GameManager script
    public GameManager gameManager;
    //Index of node we are moving towards
    private int _nodeIndex = 0;
    //Public reference to the animator
    public Animator animator;
    //private reference to our previous position
    private Vector3 _previousPosition;
    //public references to our colliders and rigidbodys
    public Rigidbody mainRigidbody;
    public BoxCollider mainCollider;
    public Rigidbody[] limbRigidbodies;
    public Collider[] limbColliders;
    //Public bool to check if we've hit the fiend
    [HideInInspector] public bool hit = false;
    #endregion
    #region Setup & Update
    // Start is called before the first frame update
    void Start()
    {
        //Setup our references
        if (animator == null) animator = GetComponent<Animator>();
    }
    public void SetPath()
    {
        _nodeIndex = 0;
        //find shortest path
        List<Node> _tempWaypoints;
        _tempWaypoints = aStar.FindShortestPath(startPos, target);
        //Change list to list we just grabbed
        _waypoints = _tempWaypoints;
    }
    void Update()
    {
        //Set previous distance
        _previousPosition = transform.position; 
        //If we have a waypoint to go to and we are in game
        if (target != null && hit != true && gameManager.gameState == GameManager.GameState.InGame)
        {
            //Move the fiend
            transform.LookAt(_waypoints[_nodeIndex].transform);
            transform.position += transform.forward * _speed * Time.deltaTime;
            if (Vector3.Distance(_waypoints[_nodeIndex].transform.position, transform.position) < 0.07f && _nodeIndex < _waypoints.Count - 1) _nodeIndex++;
            animator.SetFloat("Speed", 1f);
        }
        //else stop the animator
        else
        {
            animator.SetFloat("Speed", 0f);
        }
        //If fiend is too far away from base destroy it
        if (Vector3.Distance(transform.position, gameManager.gameObject.transform.position) > 120f)
        {
            foreach (GameObject fiend in gameManager.fiends)
            {
                if (fiend == gameObject) Destroy(fiend);
            }
        }
    }
    #endregion
    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        //If fiend triggers on wall destroy the wall, set punch trigger, and reset location free
        if (other.gameObject.tag == "BreakWalls")
        {
            animator.SetTrigger("Punch");
            other.gameObject.GetComponentInParent<BuildLocation>().locationFree = true;
            Destroy(other.gameObject);
            //Remove the wall from the list
            for (int i = 0; i < gameManager.walls.Count; i++)
            {
                if (other.gameObject == gameManager.walls[i]) gameManager.walls.RemoveAt(i);
            }
            //If we have more walls give fiends a new target
            if (gameManager.walls.Count > 0)
            {
                startPos = target;
                target = gameManager.walls[Random.Range(0, gameManager.walls.Count - 1)].GetComponentInParent<Node>();
                SetPath();
            }
            //else destroy the fiends already in scene and clear list
            else
            {
                target = null;
                foreach (GameObject fiend in gameManager.fiends) Destroy(fiend.gameObject);
                gameManager.fiends.Clear();
            }
        }
    }
    #endregion
}
