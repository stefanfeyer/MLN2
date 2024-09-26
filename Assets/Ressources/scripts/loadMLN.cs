using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;

public class loadMLN : MonoBehaviour
{
    public MLN mln;
    public Layer layer;
    public Node node;
    public Edge edge;
    // Start is called before the first frame update
    void Start()
    {
        readGraph();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void readGraph()
    {
        StreamReader stream = new StreamReader("Assets/Ressources/Graphs/simple_MLN_mandrill_1.graphml");
        XmlReaderSettings settings = new XmlReaderSettings();
        XmlReader reader = XmlReader.Create(stream, settings);

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "mln":
                            Debug.Log("mln");
                            mln = new MLN();
                            if (reader.HasAttributes)
                            {
                                string attributeValue = reader.GetAttribute(0);
                                string attributeKey = reader.LocalName;
                                Debug.Log("attribute: " + attributeKey + " " + attributeValue);
                            }
                            break;
                        case "layer":
                            Debug.Log("layer");
                            layer = new Layer();
                            //mln.addLayer(layer);
                            break;
                        case "node":
                            Debug.Log("node");
                            node = new Node();
                            break;
                        case "edge":
                            Debug.Log("edge");
                            edge = new Edge();
                            break;
                        case "data":
                            Debug.Log("data");
                            break;
                        default:
                            break;
                    }
                    break;
                case XmlNodeType.Attribute:
                    Debug.Log("attribute0: " + reader.GetAttribute(0));
                    break;
                case XmlNodeType.EndElement:
                    Debug.Log("end element: " + reader.Name);
                    break;
                case XmlNodeType.Text:
                    Debug.Log("text: " + reader.Value);
                    break;
                default:
                    break;
            }
        }
    }
}
