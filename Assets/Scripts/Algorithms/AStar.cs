using System.Collections.Generic; //Allow the use of lists
using UnityEngine; //Connect to Unity Engine

public class AStar : MonoBehaviour
{
    #region Variables
    //private array of nodes as GameObjects
    private GameObject[] _nodes;
    [Header("Path")]
    [Tooltip("Add the start point node here")]
    public Node start;
    [Tooltip("Add the end point node here")]
    public Node end;
    #endregion
    #region Algorithm
    public List<Node> FindShortestPath(Node start, Node end)
    {
        //Fill the _nodes array with Nodes from the heirarchy
        _nodes = GameObject.FindGameObjectsWithTag("Node");
        //If DijkstraAlgortithm function returns true
        if (AStarAlgorithm(start, end))
        {
            //Create a new list to hold the nodes
            List<Node> result = new List<Node>();
            //Make a node variable initialised to end point
            Node node = end;
            //Insert node to result list and change node reference to previous node on path for next iteration until node is null
            do
            {
                result.Insert(0, node);
                node = node.PreviousNode;
            } while (node != null);
            //Return the result list
            return result;
        }
        //If we make it to this line return nothing
        return null;
    }
    private bool AStarAlgorithm(Node start, Node end)
    {
        //Create a new list to store unexplored nodes
        List<Node> _unexplored = new List<Node>();
        //For each GameObject in our list of nodes from the heirarchy try to retrieve the Node class and reset its values, set the distance to endpoint then add it to unexplored list
        foreach (GameObject obj in _nodes)
        {
            Node n = obj.GetComponent<Node>();
            if (n == null) continue;
            n.ResetNode();
            n.SetDistanceToEndPoint(end.transform.position);
            _unexplored.Add(n);
        }
        //If unexplored list does not contain our start and end point return false
        if (!_unexplored.Contains(start) && !_unexplored.Contains(end)) return false;
        //Set the start path weight to 0
        start.NodeWeight = 0f;
        //While we still have nodes in unexplored list
        while (_unexplored.Count > 0)
        {
            //Sort unexplored list based on pathweight
            _unexplored.Sort((x, y) => x.NodeWeightHeuristic.CompareTo(y.NodeWeightHeuristic));
            //Current variable to store the first entry of unexplored list as shortest path possibility
            Node _current = _unexplored[0];
            //If current reference is our end point break the loop
            if (_current == end) break;
            //Remove the first entry of unexplored list
            _unexplored.RemoveAt(0);
            //For each neighbour node in our current shortest path node reference
            foreach (Node neighbourNode in _current.NeighborNodes)
            {
                //If neighbour node is equal to previous node in the path or does not exist in unexplored list ignore it
                if (_current.PreviousNode == neighbourNode) continue;
                if (!_unexplored.Contains(neighbourNode)) continue;
                //Store the distance between current node and neighbour node
                float _distance = Vector3.Distance(neighbourNode.transform.position, _current.transform.position);
                //Add the current nodes path weight to the distance
                _distance += _current.NodeWeight;
                //If distance is less then the path weight of the neighbour node set the neighbour nodes path weight to the distance and its previousNode reference to current node
                if (_distance < neighbourNode.NodeWeight)
                {
                    neighbourNode.NodeWeight = _distance;
                    neighbourNode.PreviousNode = _current;
                }
            }
        }
        //If we have made it to this point return true
        return true;
    }
    #endregion
}
