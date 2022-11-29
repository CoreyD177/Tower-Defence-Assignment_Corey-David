using System.Collections; //Allow use of coroutines
using System.Collections.Generic; //Allow use of lists
using UnityEngine; //Connect to unity engine
using UnityEngine.SceneManagement; //Allow changing of scenes and setup functions tailored to scene we are in
using UnityEngine.UI; //Allow us to interact with canvas UI

public class GameManager : MonoBehaviour
{
    //Create a static instance of GameManagerManager
    public static GameManager gameManagerInstance;
    #region Variables
    //Enum and variable to store the current state of the game so we can adjust accordingly
    public enum GameState
    {
        Menu,
        InGame,
        Paused,
        PostGame,
    }
    [Header("Gameplay Variables")]
    [Tooltip("Sets the current state of the game so we can interact differently with the various areas of the game")]
    public GameState gameState;
    [Tooltip("Set the amount of waves you want in a game")]
    [SerializeField] private int _waveCount = 5;
    [Tooltip("Sets the health level of the tower at start of game. Player will have lost when health reaches 0")]
    public short health = 100;
    //Public variable to store and adjust points
    [HideInInspector] public int points;
    [Header("References")]
    [Tooltip("Drag the AStar class from the GameManager object in here")]
    [SerializeField] private AStar _aStar;
    [Header("Nodes")]
    [Tooltip("Add the four Fiend nodes")]
    [SerializeField] private Node[] _fiendStartPoints = new Node[4];
    //Hidden public list of shortest path nodes for mobs to use as waypoints
    [HideInInspector] public List<Node> path;
    //List to store walls added to scene
    [HideInInspector] public List<GameObject> walls = new List<GameObject>();
    //List to store Fiends for comparison with walls to see if we should spawn more
    [HideInInspector] public List<GameObject> fiends = new List<GameObject>();
    //Private line renderer reference so we can show the path
    private LineRenderer _lineRenderer;
    //Current wave index variable
    private int _currentWave = 1;
    //Amount of enemies per wave, will be randomized before each wave
    private int _enemyCount;
    //Reference to endgame panel so we can end game
    private GameObject _endPanel;
    //Victory Text element so we can display end message
    [HideInInspector] public Text victoryText;
    //String and int to determine what part of mob was hit last
    [HideInInspector] public string fiendHit = "none";
    [HideInInspector] public int fiendPoints;
    #endregion
    #region Setup & Update
    void Awake()
    {
        //Don't destroy this gameobject when we move to new scene
        DontDestroyOnLoad(this.gameObject);
        //If we already have an active instance that isn't this one, destroy this instance
        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            Destroy(this.gameObject);
        }
        //Else this is the GameManager instance
        else
        {
            gameManagerInstance = this;
        }
        //If aStar reference is null, get it from this game object
        if (_aStar == null)
        {
            _aStar = gameObject.GetComponent<AStar>();
        }
    }
    private void OnEnable()
    {
        //OnEnable start listeninig for scene loads and run the named function
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    private void OnDisable()
    {
        //OnDisable stop listening for scene changes
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) gameState = GameState.Menu;
        else if (scene.buildIndex == 1)
        {
            //Game starts paused so player can build initial towers
            gameState = GameState.Paused;
            //Set the end node for AStar
            _aStar.end = GameObject.Find("EndPoint").GetComponent<Node>();
            //Find the LineRenderer in scene
            _lineRenderer = GameObject.Find("PathRenderer").GetComponent<LineRenderer>();
            //Run the HandleGameState function to setup menu mode
            HandleGameState(gameState);
            //Start new wave function to begin game
            NewWave();
            //Fill Fiend Start position array
            for (int i = 0; i < 4; i++)
            {
                _fiendStartPoints[i] = GameObject.Find("FiendStart").transform.GetChild(i).GetComponent<Node>();
            }
            //Get end game panel and victory text so we can activate it
            _endPanel = GameObject.Find("EndGameCanvas").transform.GetChild(0).gameObject;
            victoryText = _endPanel.transform.GetChild(0).gameObject.GetComponent<Text>();
            //Start fiend spawning coroutine
            StartCoroutine(SpawnFiend());
            //Set points so we can purchase towers at start
            points += 100;
            //Reset health in case we are starting a second game
            health = 100;
        }
        else if (scene.buildIndex == 2)
        {
            //Make state post game for now in bonus scene so we can exit
            gameState = GameState.PostGame;
            HandleGameState(gameState);
        }
    }    
    public void ResetShortestPath()
    {
        //Create a new list of the shortest path
        path = _aStar.FindShortestPath(_aStar.start, _aStar.end);
        //Create an array from list to use for line renderer
        Vector3[] _nodes = new Vector3[path.Count];
        //Int to use as index
        int i = 0;
        //Set previousNode to null
        Node _previousNode = null;
        //For each node in the shortest path draw a line from previous node to current node and then make previous node equal current node so we can do next line
        foreach (Node node in path)
        {
            if (_previousNode != null)
            {
                Debug.DrawLine(node.transform.position + Vector3.up, _previousNode.transform.position + Vector3.up, Color.blue, 2f);
            }
            _previousNode = node;
            //Assign node to array for line renderer and iterate counter
            _nodes[i] = node.gameObject.transform.position;
            i++;
        }
        //Give the line renderer the correct positions
        _lineRenderer.positionCount = path.Count;
        _lineRenderer.SetPositions(_nodes);
    }
    #endregion
    #region Waves
    private void NewWave()
    {
        //Set the start node for AStar to one of the four start points
        _aStar.start = GameObject.Find("StartPoint0" + Random.Range(1, 4)).GetComponent<Node>();
        ResetShortestPath();
        //Set the enemy count for the wave to a random number
        _enemyCount = Random.Range(10, 20) * _currentWave;
        //Start the coroutine to spawn the required amount of enemies
        StartCoroutine("HandleWave");
    }
    private IEnumerator HandleWave()
    {
        //An int to use as a count for the current enemy being spawned
        int count = 0;
        //Do until we have spawned the amount of enemies from newwave function
        do
        {
            //If we are not in game state yield and return so we don't spawn any more mobs while paused
            if (gameState != GameState.InGame) yield return new WaitForSecondsRealtime(0.2f);
            else
            {
                //Instantiate a mob at the start position from the AStar class
                Instantiate(Resources.Load("Prefabs/Mob"), _aStar.start.transform.position, Quaternion.identity);
                //Yield for a random amount of time between 0.5 and 1.5 seconds
                yield return new WaitForSecondsRealtime(Random.Range(0.5f, 1.5f));
                //Increment the count
                count++;
            }
        } while (count < _enemyCount);
        //If we have spawned the required amount of mobs and we have waves left, wait 15 seconds then iterate the wave count and start a new wave
        if (count == _enemyCount && _currentWave < _waveCount)
        {
            yield return new WaitForSecondsRealtime(15f);
            //Iterate the current wave
            _currentWave++;
            NewWave();
        }
        else
        {
            //If no more waves left handle post game messaging
            victoryText.text = "Congratulations, you survived the shark hordes. The golden duck thanks you for your help. You scored " + points + " points.";
            gameState = GameState.PostGame;
            HandleGameState(gameState);
        }
    }
    private IEnumerator SpawnFiend()
    {
        //Wait 10 seconds before spawning next fiend
        yield return new WaitForSecondsRealtime(10f);
        //If there are more walls than fiends in the scene
        if (walls.Count > fiends.Count && gameState == GameState.InGame)
        {
            //Random number to control spawn point
            int _rand = Random.Range(0, 3);
            //Spawn a new fiend and add it to scene
            fiends.Add(Instantiate(Resources.Load<GameObject>("Prefabs/Fiend"), _fiendStartPoints[_rand].transform.position, Quaternion.identity));
            FiendHandler fiend = fiends[fiends.Count - 1].GetComponent<FiendHandler>();
            fiend.startPos = _fiendStartPoints[_rand];
            fiend.target = walls[Random.Range(0, walls.Count - 1)].GetComponentInParent<Node>();
            fiend.gameManager = this;
            fiend.aStar = gameObject.GetComponent<AStar>();
            fiend.SetPath();
        }
        //Restart coroutine to control next spawn interval
        StartCoroutine(SpawnFiend());
    }
    #endregion
    #region Manager Functions
    public void ChangeScene(int sceneIndex)
    {
        //Load the scene that corresponds to the value passed from menu
        SceneManager.LoadScene(sceneIndex);
    }
    public void QuitGame()
    {
        //If unity editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        //Quit the game
        Application.Quit();
    }
    #endregion
    #region Game States
    public void HandleGameState(GameState gameState)
    {
        //Switch between game states to toggle settings
        switch (gameState)
        {
            case GameState.Menu:
                //Cursor unlocked and visible in menus
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameState.InGame:
                Debug.Log("In Game");
                //Cursor locked and hidden in game
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Find("Duck").transform.position = new Vector3(0f, 30f, 0f);
                break;
            case GameState.Paused:
                //Cursor unlocked and visible in pausse menu
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameState.PostGame:
                //Activate end game menu
                _endPanel.SetActive(true);
                //Cursor unlocked and visible in post game
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            default:
                break;
        }
    }
    #endregion
    #region OnGUI
    private void OnGUI()
    {
        //Attach GUI skin
        GUI.skin = Resources.Load("GUISkin") as GUISkin;
        //Show hud elements during ingame and pause game states
        if (gameState == GameState.InGame || gameState == GameState.Paused)
        {
            GUI.TextField(new Rect(0f, 0f, Screen.width, Screen.height / 27), gameState.ToString() + " - Last mob hit was: " + fiendHit + " for " + fiendPoints + " points.");
            GUI.TextField(new Rect(0f, (Screen.height/27) * 26, Screen.width/9, Screen.height/27), "Tower Health: " + health);
            GUI.TextField(new Rect(Screen.width/9, (Screen.height / 27) * 26, Screen.width / 9, Screen.height / 27), "Points: " + points);
            GUI.TextField(new Rect((Screen.width / 9) * 2, (Screen.height / 27) * 26, (Screen.width / 9) * 4, Screen.height / 27), "Shark Attack III: The Last Stand");
            GUI.TextField(new Rect((Screen.width / 9) * 6, (Screen.height / 27) * 26, Screen.width / 9, Screen.height / 27), "Current Wave: " + _currentWave + "/5");
            GUI.TextField(new Rect((Screen.width / 9) * 7, (Screen.height / 27) * 26, Screen.width / 9, Screen.height / 27), "Enemy Count: " + _enemyCount);
            GUI.TextField(new Rect((Screen.width / 9) * 8, (Screen.height / 27) * 26, Screen.width / 9, Screen.height / 27), "Fiends " + fiends.Count);
        }
        //Show buttons during post game
        if (gameState == GameState.PostGame)
        {
            if (GUI.Button(new Rect(0f, 0f, Screen.width / 5, Screen.height / 27), "Main Menu")) ChangeScene(0);
            if (GUI.Button(new Rect((Screen.width / 5) * 4, 0f, Screen.width / 5, Screen.height / 27), "Exit Game")) QuitGame();            
        }
    }
    #endregion
}
