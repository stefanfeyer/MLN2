using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public int source;
    public int target;
    public float weight;
    GameObject cube;
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    //void Update(){}

    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }

    void setColour()
    {
        
    }

    void setPosition()
    {

    }
}
