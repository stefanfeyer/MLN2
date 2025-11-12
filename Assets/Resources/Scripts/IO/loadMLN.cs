using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using Unity.VisualScripting;
using System.Runtime.InteropServices.ComTypes;

/// <summary>
/// Loader MonoBehaviour for MLN data from GraphML files. Can use a custom or newer reader path.
/// After loading, computes basic metrics and spawns an <see cref="MLNVis"/> for visualisation.
/// </summary>
public class loadMLN : MonoBehaviour
{
    /// <summary>Loaded MLN instance.</summary>
    public MLN mln;

    /// <summary>Working layer reference while parsing.</summary>
    public Layer layer;

    /// <summary>Working node reference while parsing.</summary>
    public Node node;

    /// <summary>Working edge reference while parsing.</summary>
    public Edge edge;

    /// <summary>Optional debug helper.</summary>
    public debugStuff debugStuff;

    /// <summary>Filesystem path to the GraphML file.</summary>
    public string path = "";

    /// <summary>Toggle to choose the newer GraphML reader path.</summary>
    public bool isNewGraphML = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Loads the MLN from <see cref="path"/>, computes metrics, and instantiates a visualisation object.
    /// </summary>
    public void load()
    {
        if (isNewGraphML)
        {
            ReadGraphML reader = new ReadGraphML();
            mln = reader.readMLNGraph(path);
        }
        else
        {
            readGraph(path);
        }

        calcNetworkMetrics();
        Debug.Log("Loaded MLN: " + mln.id);

        GameObject mlnVis = new GameObject();
        mlnVis.AddComponent<MLNVis>().mln = mln;
    }

    /// <summary>
    /// Calculates per-layer density and per-edge layer-relative weights.
    /// </summary>
    private void calcNetworkMetrics()
    {
        foreach (Layer layer in mln.layers)
        {
            layer.calculateLayerDensity();
            foreach (Edge edge in layer.edges)
            {
                edge.calculateWeightRelativeToLayer01();
            }
        }
    }

    /// <summary>
    /// Legacy GraphML reader (XML streaming); populates <see cref="mln"/>, layers, nodes, and edges.
    /// </summary>
    public void readGraph(string path)
    {
        StreamReader stream = new StreamReader(path);
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
                            mln = new MLN(reader.GetAttribute("id"));
                            break;
                        case "layer":
                            layer = new Layer(reader.GetAttribute("id"), mln);
                            readLayer(layer, reader.ReadSubtree());
                            break;
                        case "edge":
                            // Edge nodes come after all layers
                            edge = new Edge(reader.GetAttribute("id"), mln.getNodeByID(reader.GetAttribute("source")), mln.getNodeByID(reader.GetAttribute("target")), mln);
                            readEdge(edge, reader.ReadSubtree());
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

    /// <summary>
    /// Reads a <c>&lt;layer&gt;</c> subtree and its nodes.
    /// </summary>
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
                                layer.label = (string)layerReader.ReadElementContentAs(typeof(string), null);
                            }
                            break;
                        case "node":
                            Node node = new Node(layerReader.GetAttribute("id"), layer, mln);
                            readNode(node, layerReader.ReadSubtree());
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

    /// <summary>
    /// Reads a <c>&lt;node&gt;</c> subtree and its data attributes.
    /// </summary>
    private void readNode(Node node, XmlReader nodeReader)
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
                                default:
                                    node.addAttribute(nodeReader.GetAttribute("key"), (string)nodeReader.ReadElementContentAs(typeof(string), null));
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

    /// <summary>
    /// Reads an <c>&lt;edge&gt;</c> subtree and its data (label/weight).
    /// </summary>
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
                                    edge.label = (string)edgeReader.ReadElementContentAs(typeof(string), null);
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
