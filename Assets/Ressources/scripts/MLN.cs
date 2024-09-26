using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLN 
{
    public float xPos;
    public float yPos;
    public float zPos;
    public string id;
    public List<Layer> layers;
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    // void Update(){}

    public void addLayer(Layer layer)
    {
        layers.Add(layer);
    }

    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }
}
