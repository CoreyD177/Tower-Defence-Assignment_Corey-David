using System.Collections.Generic; //Allow use of lists
using UnityEngine; //Connect to Unity Engine

public class MobHandler : MonoBehaviour
{
    #region Variables
    [Header("Movement Variables")]
    [Tooltip("Set the speed for the mob to move at")]
    [SerializeField] private float _mobSpeed = 12f;
    [Header("References")]
    [Tooltip("Add the GameManager class from the GameManager object in here")]
    [SerializeField] private GameManager _gameManager;
    [Tooltip("Add the AStar class from the GameManager object here")]
    [SerializeField] private AStar _aStar;
    //Private waypoint ID to retrieve next waypoint from list. Set to 1 as first entry is start point
    private int _waypointID = 1;
    //Private list of the waypoints we are moving toward to be filled at start
    private List<Node> _waypoints;
    //Private count of walls in GameManagers wall list
    private int _wallCount;
    //Private reference to the previous waypoint in case we need to redetermine path
    private Node _previousWaypoint;
    //Private reference to the current waypoint in case we need to redetermine path
    private Node _currentWaypoint;
    //Variable to store the amount of hits this mob has received
    [SerializeField] public int hits = 0;
    #endregion
    #region Setup & Update
    private void Start()
    {
        //If GameManager or AStar reference is empty find GameManager in scene and assign AStar from it
        if (_gameManager == null || _aStar == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            _aStar = _gameManager.gameObject.GetComponent<AStar>();
        }
        //Fill the waypoint list with the list from the GameManager
        _waypoints = _gameManager.path;
        //Get the current count of walls in list which should be 0 at start
        _wallCount = _gameManager.walls.Count;
        //Set the waypoint variables on start
        _previousWaypoint = _waypoints[0];
        _currentWaypoint = _waypoints[1];
    }
    private void Update()
    {
        if (_gameManager.gameState == GameManager.GameState.InGame)
        {
            //Rotate the mob to look toward its current waypoint
            transform.LookAt(_waypoints[_waypointID].transform.position);
            //If we haven't reached waypoint, move toward it using transform.forward
            if (Vector3.Distance(transform.position, _waypoints[_waypointID].transform.position) > 0.15f)
            {
                transform.position += transform.forward * _mobSpeed * Time.deltaTime;
            }
            //Else adjust our waypoint references and iterate the waypoint ID to make the next waypoint our new current waypoint
            else if (_waypointID < _waypoints.Count - 1)
            {
                _previousWaypoint = _waypoints[_waypointID];
                _waypointID++;
                _currentWaypoint = _waypoints[_waypointID];
            }
        }
    }
    private void FixedUpdate()
    {
        //Check if we have placed a new wall and if we have update the mobs path so it will avoid the wall
        if (_gameManager.walls.Count != _wallCount)
        {
            List<Node> _tempWaypoints;
            if (_currentWaypoint.buildWeight > 500)
            {
                _tempWaypoints = _aStar.FindShortestPath(_previousWaypoint, _aStar.end);
            }
            else
            {
                _tempWaypoints = _aStar.FindShortestPath(_currentWaypoint, _aStar.end);
            }
            //Reset waypoint ID to 0 to go to first entry in list
            _waypointID = 0;
            //Change list to list we just grabbed
            _waypoints = _tempWaypoints;
            //Reset wallcount to current number of walls
            _wallCount = _gameManager.walls.Count;
        }
    }
    #endregion
    #region Collision
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        //If we collide with sydney tower reduce players health and destroy this mob
        if (collision.transform.name == "SydneyTower")
        {
            _gameManager.health -= 10;
            Destroy(gameObject);
            //Change game state to reflect loss if tower has no health left
            if (_gameManager.health <= 0)
            {
                _gameManager.victoryText.text = "Sadly you did not survive the hordes of angry sharks. You failed the golden duck and have been stripped of all your points";
                _gameManager.gameState = GameManager.GameState.PostGame;
                _gameManager.HandleGameState(_gameManager.gameState);
            }
        }
    }
    #endregion
}
