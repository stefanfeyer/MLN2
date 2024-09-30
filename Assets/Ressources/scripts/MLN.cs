using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLN
{
    public float xPos;
    public float yPos;
    public float zPos;
    public string id;
    public string label;
    public List<Layer> layers = new List<Layer>();
    public List<Edge> edges = new List<Edge>();
    public List<KeyValuePair<string, string>> attributes = new List<KeyValuePair<string, string>>();

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    // void Update(){}

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

    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }
}
