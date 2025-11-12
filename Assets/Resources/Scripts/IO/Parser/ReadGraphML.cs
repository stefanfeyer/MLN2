using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// GraphML reader for MLNs. Supports mono-layer (flat graph) and nested MLN formats,
/// populating layers, nodes, edges, positions, colours, weights, and arbitrary node attributes.
/// </summary>
public class ReadGraphML
{
    MLN mln;

    /// <summary>Constructs a new GraphML reader.</summary>
    public ReadGraphML() { }

    /// <summary>
    /// Reads a mono-layer graph (single graph element) from disk.
    /// </summary>
    /// <param name="path">Filesystem path of the GraphML file.</param>
    /// <returns>Constructed MLN with a single layer.</returns>
    public MLN readMonoLayerGraph(string path)
    {
        string fileName = Path.GetFileName(path);
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
                        case "graphml":
                            mln = new MLN(fileName);
                            break;
                        case "key":
                            break;
                        case "graph":
                            Layer layer = new Layer(reader.GetAttribute("id"), mln);
                            readLayer(layer, reader.ReadSubtree());
                            break;
                        case "edge":
                            Edge edge = new Edge(reader.GetAttribute("id"), mln.getNodeByID(reader.GetAttribute("source")), mln.getNodeByID(reader.GetAttribute("target")), mln);
                            readEdge(edge, reader.ReadSubtree());
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        return mln;
    }

    /// <summary>
    /// Reads an MLN graph (layers as nodes containing nested graphs).
    /// </summary>
    /// <param name="path">Filesystem path of the GraphML file.</param>
    /// <returns>Constructed MLN.</returns>
    public MLN readMLNGraph(string path)
    {
        string fileName = Path.GetFileName(path);
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
                        case "graphml":
                            mln = new MLN(fileName);
                            break;
                        case "key":
                            break;
                        case "graph":
                            readLayer1lvl(reader.ReadSubtree());
                            break;
                        case "edge":
                            Edge edge = new Edge(reader.GetAttribute("id"), mln.getNodeByID(reader.GetAttribute("source")), mln.getNodeByID(reader.GetAttribute("target")), mln);
                            readEdge(edge, reader.ReadSubtree());
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        return mln;
    }

    /// <summary>
    /// Handles first nested level: layer nodes that contain child graphs.
    /// </summary>
    private void readLayer1lvl(XmlReader layerReader)
    {
        while (layerReader.Read())
        {
            switch (layerReader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (layerReader.Name)
                    {
                        case "node":
                            Layer layer = new Layer(layerReader.GetAttribute("id"), mln);
                            readLayer2lvl(layer, layerReader.ReadSubtree());
                            break;
                        case "edge":
                            Edge edge = new Edge(layerReader.GetAttribute("id"), mln.getNodeByID(layerReader.GetAttribute("source")), mln.getNodeByID(layerReader.GetAttribute("target")), mln);
                            readEdge(edge, layerReader.ReadSubtree());
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
    /// Reads layer label and delegates to actual layer contents.
    /// </summary>
    private Layer readLayer2lvl(Layer layer, XmlReader layerReader)
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
                        case "graph":
                            readLayer(layer, layerReader.ReadSubtree());
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
    /// Reads the inner graph for a layer and its nodes.
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
    /// Reads node data (label, xyz, rgb, and generic attributes).
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
                                case "x":
                                    string xElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initialPosition.x = float.Parse(xElem);
                                    node.hasInitialPosition = true;
                                    break;
                                case "y":
                                    string yElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initialPosition.y = float.Parse(yElem);
                                    node.hasInitialPosition = true;
                                    break;
                                case "z":
                                    string zElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initialPosition.z = float.Parse(zElem);
                                    node.hasInitialPosition = true;
                                    break;
                                case "r":
                                    string rElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initalColor.r = float.Parse(rElem);
                                    node.hasInitialColor = true;
                                    break;
                                case "g":
                                    string gElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initalColor.g = float.Parse(gElem);
                                    node.hasInitialColor = true;
                                    break;
                                case "b":
                                    string bElem = (string)nodeReader.ReadElementContentAs(typeof(string), null);
                                    node.initalColor.b = float.Parse(bElem);
                                    node.hasInitialColor = true;
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
    /// Reads edge label/weight for a given edge context.
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
