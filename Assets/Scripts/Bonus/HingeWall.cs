using System.Collections; //Use Coroutines
using UnityEngine; //Connect to unity engine

public class HingeWall : MonoBehaviour
{
    #region Variables
    //private reference to the hingejoint
    private HingeJoint _joint;
    //private int to differentiate when each wall will open
    private int _random;
    #endregion
    void Start()
    {
        //Randomise number for open order
        _random = Random.Range(0, 9);
        //Fill hinge connection
        _joint = GetComponent<HingeJoint>();
        //Start the coroutine to activate the hinge
        StartCoroutine(ActivateHinge());
    }
    private IEnumerator ActivateHinge()
    {
        if (_random >= 5)
        {
            //Change the use motor bool to the opposite of its current value
            _joint.useMotor = !_joint.useMotor;
            
        }
        else
        {
            _random = 9;
        }
        //Wait 5 seconds
        yield return new WaitForSecondsRealtime(5f);
        //Restart new version of this coroutine to do it again
        StartCoroutine(ActivateHinge());
    }
}
