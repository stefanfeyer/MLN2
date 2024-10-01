using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLN
{
    public string id;
    public string label;
    public List<Node> nodes = new List<Node>();
    public List<Layer> layers = new List<Layer>();
    public List<Edge> edges = new List<Edge>();
   
    public MLN(string _id)
    {
        id = _id;
        label = _id;
    }

    public void addLayer(Layer layer)
    {
        layers.Add(layer);
    }

    public void addEdge(Edge edge)
    {
        edges.Add(edge);
    }

    public void addNode(Node node)
    {
        nodes.Add(node);
    }

    public Node getNode(string _id)
    {
        foreach (Node node in nodes)
        {
            if (node.id == _id)
            {
                //Debug.Log("Node ID: " + id);
                return node;
            }
        }
        // make this good
        Debug.Log("Node ID: " + _id);
        throw new MissingReferenceException("Node not found");
    }
}
