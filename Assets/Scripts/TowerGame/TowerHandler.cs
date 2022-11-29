using System.Collections; //Allow use of Coroutines
using UnityEngine; //Connect to Unity Engine

public class TowerHandler : MonoBehaviour
{
    #region Variables
    //Public reference to the current enemy to be set by the BuildLocation
    [HideInInspector] public Collider currentEnemy;
    [Header("Tower Variables")]
    [Tooltip("Set the rate of fire for the tower")]
    [SerializeField] private float _rateOfFire = 2f;
    //Private float to control delay
    private float _attackDelay = 0;
    //Private reference to line renderer for laser
    private LineRenderer _lineRenderer;
    #endregion
    #region Tower Functions
    void Start()
    {
        //Attach the line renderer so we can use lines as a laser
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }
    void Update()
    {
        //If we have an enemy in range
        if (currentEnemy != null)
        {
            //Look at the enemy
            transform.LookAt(currentEnemy.transform.position);
            //If we have delayed long enough reset the delay, set the line renderer positions and start the Coroutine to handle laser shooting
            if (_attackDelay == 0 && GameManager.gameManagerInstance.gameState == GameManager.GameState.InGame)
            {
                _attackDelay = _rateOfFire;
                Vector3[] _positions = new Vector3[2] { transform.GetChild(0).position, currentEnemy.transform.position };
                _lineRenderer.SetPositions(_positions);
                StartCoroutine("ShootLaser");
            }
        }
        //Each update we have delay time left move towards 0 by delta time
        _attackDelay = Mathf.MoveTowards(_attackDelay, 0, Time.deltaTime);
    }
    private IEnumerator ShootLaser()
    {
        //Enable the line renderer to display laser
        _lineRenderer.enabled = true;
        //Get the Mobhandler from the enemy so we can modify values
        MobHandler _mob = currentEnemy.gameObject.GetComponent<MobHandler>();
        //Iterate mob hits to say we have hit one more time
        _mob.hits++;
        //If we have hit mob the required aqmount destroy the mob and add score to game manager
        if (_mob.hits == 3) Destroy(currentEnemy.gameObject);
        GameManager.gameManagerInstance.points += 2;
        //Wait a small amount of time
        yield return new WaitForSeconds(_attackDelay/3);
        //Deactivate laser
        _lineRenderer.enabled = false;
    }
    #endregion
}
