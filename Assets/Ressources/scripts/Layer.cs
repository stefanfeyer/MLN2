using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Layer
{
    public float xPos;
    public float yPos;
    public float zPos;
    public string id;
    public string label { get; set; }
    public List<Node> nodes = new List<Node>();
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    //void Update(){}

    public Layer(string _id) 
    {
        id = _id;
    }
    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
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
