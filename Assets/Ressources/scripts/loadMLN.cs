using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using Unity.VisualScripting;

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
                        case "key":
                            break;
                        case "mln":
                            Debug.Log("mln");
                            mln = new MLN(reader.GetAttribute("id"));
                            //XmlReader mlnReader = reader.ReadSubtree();
                            break;
                        case "layer":
                            Debug.Log("layer");
                            layer = new Layer(reader.GetAttribute("id"));
                            readLayer(layer, reader.ReadSubtree());
                            mln.addLayer(layer);
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

    private Layer readLayer(Layer layer, XmlReader layerReader)
    {
        while (layerReader.Read())
        {
            switch (layerReader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (layerReader.Name)
                    {
                        case "data":
                            if (layerReader.GetAttribute("key") == "label")
                            {
                                layer.label = layerReader.Value;
                            }
                            break;
                        case "node":
                            Node node = new Node(layerReader.GetAttribute("id"));
                            readNode(node, layerReader.ReadSubtree());
                            layer.addNode(node);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        return layer;
    }

    void readNode(Node node, XmlReader nodeReader)
    {
        while (nodeReader.Read())
        {
            switch (nodeReader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (nodeReader.Name)
                    {
                        case "data":
                            switch (nodeReader.GetAttribute("key"))
                            {
                                case "label":
                                    node.label = nodeReader.Value;
                                    break;
                                case "nodeid":
                                    // node.id = nodeReader.Value;   
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "node":

                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
