using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string id;
    public string label;
    public string[] neighbors;
    public NodeVis nodeVis;

    public Node(string _id)
    {
        id = _id;   
    }
}
