using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer
{
    public float xPos;
    public float yPos;
    public float zPos;
    public string id;
    public string label;
    public List<Node> nodes;
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    //void Update(){}
    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }
}
