using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public string id;
    public string source;
    public string target;
    public float weight;
    public string label;
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    //void Update(){}
    public Edge(string _id, string _source, string _target) 
    {
        id = _id;
        source = _source;
        target = _target;
    }

    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }

    public void setWeight(string _weight)
    {
        weight = float.Parse(_weight);
    }
    void setColour()
    {
        
    }

    void setPosition()
    {

    }
}
