using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public float xPos = 0;
    public float yPos = 0;
    public float zPos = 0;
    public float size = 0;
    public string id;
    public string label;
    public string[] neighbors;

    public Node(string _id)
    {
        id = _id;   
    }
}
