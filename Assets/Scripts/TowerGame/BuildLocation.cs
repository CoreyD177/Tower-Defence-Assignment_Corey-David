using UnityEngine; //Connect to Unity Engine

public class BuildLocation : MonoBehaviour
{
    #region Variables
    [Header("Location Variables")]
    [Tooltip("Bool check to test if location having a tower will affect path chosen")]
    public bool locationFree = true;
    [Header("References")]
    [Tooltip("Add the GameManager from the object in heirarchy here")]
    [SerializeField] private GameManager _gameManager;
    //Private tag variable to determine if build location is a Tower or Wall
    private string _locationTag;
    //Private array of Enemies within radius
    private Collider[] _enemiesNearby = new Collider[20];
    //Private reference to tower created
    private TowerHandler _towerHandler;
    #endregion
    #region Setup
    private void Start()
    {
        //If GameManager reference is null find it in scene
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        //Set the tag by retrieving tag from game object
        _locationTag = gameObject.tag;
        //If location tag is OrigTowers setup TowerHandler reference as we won't set it up in build function
        if (_locationTag == "OrigTowers")
        {
            _towerHandler = gameObject.GetComponentInChildren<TowerHandler>();
        }
    }
    private void FixedUpdate()
    {
        //If we have a tower on the pad and we are in game
        if (!locationFree && _locationTag != "Wall" && _gameManager.gameState == GameManager.GameState.InGame)
        {
            //Layer mask is 6, so we use bitshift to find correct int
            int _layerMask = 1 << 6;
            //Use Physics overlap sphere to add enemies to array filtering out layer
            int _enemyNumbers = Physics.OverlapSphereNonAlloc(transform.position + (-transform.forward * 4f) + (transform.right * 5f), 12f, _enemiesNearby, _layerMask);
            //If enemies array is not empty give the tower the first enemy on the list
            if (_enemiesNearby[0] != null) _towerHandler.currentEnemy = _enemiesNearby[0];
        }
    }
    #endregion
    #region Manage Location
    public void Build()
    {
        if (locationFree)
        {
            //If location is tagged for towers instantiate a tower, else instantiate a wall, update the buildWeight and reset the shortest path, plus adjust the points to reflect purchase
            if (_locationTag == "Tower" && _gameManager.points >= 10)
            {
                GameObject _tower = Instantiate(Resources.Load<GameObject>("Prefabs/Tower"), new Vector3(transform.position.x, 2.5f, transform.position.z), Quaternion.identity);
                _tower.transform.parent = gameObject.transform;
                _towerHandler = _tower.GetComponentInChildren<TowerHandler>();
                transform.parent.GetComponent<Node>().buildWeight += 100;
                _gameManager.points -= 10;
                _gameManager.ResetShortestPath();
            }
            else if (_locationTag == "Wall" && _gameManager.points >= 20)
            {
                _gameManager.points -= 20;
                _gameManager.walls.Add(Instantiate(Resources.Load<GameObject>("Prefabs/Wall"), new Vector3(transform.position.x, 1.5f, transform.position.z), transform.rotation));
                _gameManager.walls[_gameManager.walls.Count - 1].transform.parent = gameObject.transform;
                transform.parent.GetComponent<Node>().buildWeight += Mathf.Infinity;
                _gameManager.ResetShortestPath();
                //If we have fiends make them target the wall
                if (_gameManager.walls.Count == 1 && _gameManager.fiends.Count > 0)
                {
                    foreach (GameObject obj in _gameManager.fiends)
                    {
                        FiendHandler fiend = obj.GetComponent<FiendHandler>();
                        fiend.startPos = fiend.target;
                        fiend.target = _gameManager.walls[Random.Range(0, _gameManager.walls.Count - 1)].GetComponentInParent<Node>();
                        fiend.SetPath();
                    }
                }
            }
        }
        //Set location free bool to false
        locationFree = false;
    }
    #endregion
}
