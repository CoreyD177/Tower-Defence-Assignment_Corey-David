using System.Collections.Generic; //Allow use of lists
using UnityEngine; //Connect to Unity Engine

public class Node : MonoBehaviour
{
    #region Variables
    //Private variable to set distance to end and public property to retrieve but not modify it
    private float _distanceToEndPoint = 0f;
    public float DistanceToEndPoint { get { return _distanceToEndPoint; } }
    //Private variable to control the weight of the node for finding the shortest path and a public property to get and set it. Initialize at max value as we will be calculating smaller distances.
    private float _nodeWeight = int.MaxValue;
    public float NodeWeight { get { return _nodeWeight; } set { _nodeWeight = value + buildWeight; } }
    //Add heuristic to path weight for AStar Algorithm
    public float NodeWeightHeuristic { get { return _nodeWeight + _distanceToEndPoint; } set { _nodeWeight = value; } }
    //A private variable to store the previous node point while following the shortest path and a public property to get and set
    private Node _previousNode = null;
    public Node PreviousNode { get { return _previousNode; } set { _previousNode = value;} }
    //A private list of neighbour nodes and a public property to retrieve them. Serialize the variable so we can add neighbours in Unity
    [SerializeField] private List<Node> _neighbourNodes = new List<Node>();
    public List<Node> NeighborNodes { get { return _neighbourNodes; } }
    //Float to store build weight to add to node weight if we have towers or walls built on this section
    [HideInInspector] public float buildWeight = 0;
    #endregion
    #region Setup
    private void Start()
    {
        //On start run the reset function to clear the path weights, previous node and distance
        ResetNode();
        //Validate the neighbourNodes list to make sure this node exists in the neighbourNode list of its neighbours
        ValidateNeighbours();
    }
    public void ResetNode()
    {
        //Reset the path weight, previous node and distance to default values
        _nodeWeight = int.MaxValue;
        _distanceToEndPoint = 0f;
        _previousNode = null;
    }
    public void SetDistanceToEndPoint(Vector3 endPosition)
    {
        //Set the distance from current position to the position of the end point
        _distanceToEndPoint = Vector3.Distance(transform.position, endPosition);
    }
    #endregion
    #region Neighbours
    public void AddNeighbour(Node node)
    {
        //Add the node passed to this function to the neighbours list
        _neighbourNodes.Add(node);
    }
    private void OnValidate()
    {
        //On changes made in  Unity editor run the ValidateNeighbours function
        ValidateNeighbours();
    }
    private void ValidateNeighbours()
    {
        foreach (Node node in _neighbourNodes)
        {
            //If we don't have a node in the current list entry continue without doing anything
            if (node == null) continue;
            //If neighbour list in neighbour node does not contain this object, add it
            if (!node._neighbourNodes.Contains(this))
            {
                node._neighbourNodes.Add(this);
            }
        }
    }
    #endregion
    #region Gizmos
    private void OnDrawGizmos()
    {
        foreach (Node node in _neighbourNodes) { 
            //If we dont have a node in the current list entry continue without doing anything
            if (node == null) continue;
            //Draw a magenta coloured line from current position to neighbours position
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
    #endregion
}
