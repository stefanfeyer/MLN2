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
    public Dictionary<string, string> attributess;
    public int[] neighbors;
    GameObject sphere;
    public List<KeyValuePair<string, string>> attributes;

    // Start is called before the first frame update
    //void Start(){}

    // Update is called once per frame
    //void Update(){}

    // Start is called before the first frame update
    public Node()
    {
        //sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.SetParent(this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 _position = new Vector3(xPos, yPos, zPos);
        //this.transform.position = _position;
    }

    void setPosition(Vector3 position)
    {
        //this.transform.position = position;
    }

    public void addAttribute(string key, string value)
    {
        attributes.Add(new KeyValuePair<string, string>(key, value));
    }
}
