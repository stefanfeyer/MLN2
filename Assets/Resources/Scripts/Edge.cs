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
    public EdgeVis edgeVis;

    public Edge(string _id, string _source, string _target) 
    {
        id = _id;
        source = _source;
        target = _target;
    }

    public void setWeight(string _weight)
    {
        weight = float.Parse(_weight);
    }
}
