using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Layer
{
    public string id;
    public string label;
    public List<Node> nodes = new List<Node>();
    public LayerVis layerVis;

    public Layer(string _id) 
    {
        id = _id;
    }

    public void addNode(Node node)
    {
        nodes.Add(node);
    }

    public void removeNode(Node node)
    {
        nodes.Remove(node);
    }
}
