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
        Debug.Log("MLN done: " + mln.id);
        GameObject mlnVis = new GameObject();   
        mlnVis.AddComponent<CreateMLNVis>().mln = mln;
        
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
                            mln = new MLN(reader.GetAttribute("id"));
                            break;
                        case "layer":
                            layer = new Layer(reader.GetAttribute("id"));
                            readLayer(layer, reader.ReadSubtree());
                            mln.addLayer(layer);
                            break;
                        case "edge":
                            edge = new Edge(reader.GetAttribute("id"), reader.GetAttribute("source"), reader.GetAttribute("target"));
                            readEdge(edge, reader.ReadSubtree());
                            mln.addEdge(edge);
                            // adjazentliste
                            break;
                        case "data":
                            Debug.Log("data");
                            break;
                        default:
                            break;
                    }
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
                                layer.label = (string) layerReader.ReadElementContentAs(typeof(string), null);
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
                                    node.label = (string)nodeReader.ReadElementContentAs(typeof(string), null);
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

    private void readEdge(Edge edge, XmlReader edgeReader)
    {
        while (edgeReader.Read())
        {
            switch (edgeReader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (edgeReader.Name)
                    {
                        case "data":
                            switch (edgeReader.GetAttribute("key"))
                            {
                                case "label":
                                    edge.label = (string) edgeReader.ReadElementContentAs(typeof(string), null);
                                    break;
                                case "weight":
                                    edge.setWeight((string)edgeReader.ReadElementContentAs(typeof(string), null));
                                    break;
                                default:
                                    break;
                            }
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
