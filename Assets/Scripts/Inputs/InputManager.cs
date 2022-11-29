using UnityEngine; //Connect to Unity Engine
using UnityEngine.InputSystem; //Use unity's input system

public class InputManager : MonoBehaviour
{
    #region Variables
    [Header("Movement Variables")]
    [Tooltip("Set the speed you want the duck to rotate at.")]
    [SerializeField] private float _rotateSpeed = 5f;
    [Tooltip("Set the speed you want the duck/camera to move at during pause/build scene")]
    [SerializeField] private float _moveSpeed = 15f;
    [Header("References")]
    [Tooltip("Add the GameManager from the scene in here.")]
    [SerializeField] private GameManager _gameManager;
    //Private vector2 to store input data for movement
    private Vector2 _moveDirection;
    //A Vector2 to store the direction information received from inputs
    private Vector2 _rotation;
    //private variable to store the hit info from the physics raycast
    private RaycastHit _hitInfo;
    #endregion
    #region Setup & Update
    private void Start()
    {
        //If GameManager reference is empty
        if (_gameManager == null)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }
    private void Update()
    {
        //If we are paused we can move the duck to get closer to build locations
        if (_gameManager.gameState == GameManager.GameState.Paused)
        {
            transform.Translate(-new Vector3(-_moveDirection.x, 0f, -_moveDirection.y) * _moveSpeed * Time.deltaTime);
        }
        //Apply rotation from inputs, using ClampRotation function to restrict x axis rotation to accepted range
        transform.eulerAngles += (new Vector3(-_rotation.x, _rotation.y, 0f));
        

    }
    private void FixedUpdate()
    {
        //Use fixedupdate to constrain position and rotation between desired values
        if (transform.position.y < 7f || transform.position.y > 32) transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 7f, 32f), transform.position.z);
        transform.eulerAngles = new Vector3(ClampRotation(transform.eulerAngles.x, -23f, 50f), transform.eulerAngles.y, 0f);
    }
    private float ClampRotation(float rotationX, float minXRotation, float maxXRotation)
    {
        //Divide rotationX by 360 degrees and store the remainder
        rotationX = rotationX % 360f;
        //If rotation x is larger than 180 remove 360 to bring it back within -180 to 180 range
        if (rotationX > 180f) rotationX -= 360f;
        //Else if it is less than -180 add 360 to bring it back within -180 to 180 range
        else if (rotationX < -180) rotationX += 360;
        //Now that we have put rotation value in correct range we can clamp it properly within the range we want. 
        rotationX = Mathf.Clamp(rotationX, minXRotation, maxXRotation);
        //Return the clamped value for use in rotation
        return rotationX;
    }
    #endregion
    #region Input Callback Functions
    public void RotateDuck(InputAction.CallbackContext context)
    {
        //Store the value of input from input system callback clamped between -1 and 1 and multiply by rotation speed
        _rotation.y = Mathf.Clamp(context.ReadValue<Vector2>().x, -1f, 1f) * _rotateSpeed;
        _rotation.x= Mathf.Clamp(context.ReadValue<Vector2>().y, -1f, 1f) * _rotateSpeed;
    }
    public void ClickHandler(InputAction.CallbackContext context)
    {
        //If user has released the button. We do this check so enclosed functions will not trigger 3 times
        if (context.canceled)
        {
            if (_gameManager.gameState == GameManager.GameState.Paused)
            {
                //Create a ray for a physics raycast
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Cast the ray and store info about object we hit
                if (Physics.Raycast(ray, out _hitInfo, 160f))
                {

                    //If we hit a tower or wall pad trigger the build function
                    if (_hitInfo.collider.transform.tag == "Tower" || _hitInfo.collider.transform.tag == "Wall")
                    {
                        _hitInfo.transform.gameObject.GetComponent<BuildLocation>().Build();
                    }
                }
            }
            //If we are in ingame state fire a bullet and set bullets references
            else if (_gameManager.gameState == GameManager.GameState.InGame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                ProjectileHandler bullet = Instantiate(Resources.Load<GameObject>("Prefabs/Projectile"), Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f)), Camera.main.transform.rotation).GetComponent<ProjectileHandler>();
                bullet.duck = this.gameObject;
                bullet.gameManager = _gameManager;
                bullet.movementDir = transform.forward;
            }
        }
    }
    public void PauseHandler(InputAction.CallbackContext context)
    {
        //If we have pushed the button
        if (context.started)
        {
            //If state is InGame change it to paused
            if (_gameManager.gameState == GameManager.GameState.InGame)
            {
                _gameManager.gameState = GameManager.GameState.Paused;
            }
            //If state is Paused change it to InGame
            else if (_gameManager.gameState == GameManager.GameState.Paused)
            {
                _gameManager.gameState = GameManager.GameState.InGame;
            }
            //Trigger the HandleGameState function to adjust options
            _gameManager.HandleGameState(_gameManager.gameState);
        }
    }
    public void MovementHandler(InputAction.CallbackContext context)
    {
        //Get movement direction from inputs
        _moveDirection = context.ReadValue<Vector2>();
    }
    #endregion
    #region OnGUI
    private void OnGUI()
    {
        //Attach GUISkin
        GUI.skin = Resources.Load("GUISkin") as GUISkin;
        //Draw an OnGUI box to use as a reticule
        GUI.Box(new Rect((Screen.width / 128) * 63.5f, (Screen.height / 72) * 35.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 62.5f, (Screen.height / 72) * 35.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 61.5f, (Screen.height / 72) * 35.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 64.5f, (Screen.height / 72) * 35.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 65.5f, (Screen.height / 72) * 35.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 63.5f, (Screen.height / 72) * 34.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 63.5f, (Screen.height / 72) * 33.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 63.5f, (Screen.height / 72) * 36.5f, Screen.width / 128, Screen.height / 72), " ");
        GUI.Box(new Rect((Screen.width / 128) * 63.5f, (Screen.height / 72) * 37.5f, Screen.width / 128, Screen.height / 72), " ");
    }
    #endregion
}
