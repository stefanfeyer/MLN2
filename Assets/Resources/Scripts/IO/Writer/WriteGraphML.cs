using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// GraphML writer for MLNs. Serialises layers, nodes (with positions/colours/attributes), and edges to disk.
/// Also supports writing an aggregated super-graph.
/// </summary>
public class WriteGraphML
{
    MLN mln;
    string name = "tmp";

    /// <summary>Base output directory.</summary>
    string path = @"Assets/Resources/Graphs/exportedGraphs/";

    /// <summary>
    /// Constructs a writer for the given MLN and output name (file stem).
    /// </summary>
    public WriteGraphML(MLN _mln, string _name)
    {
        mln = _mln;
        name = _name;
        // Optionally call writeGraphML() here if automatic export is desired.
    }

    /// <summary>
    /// Writes a GraphML &lt;key&gt; element for a node/edge attribute.
    /// </summary>
    private void writeKey(XmlWriter writer, string sFor, string attrName, string attrType, string sID)
    {
        writer.WriteStartElement("key");
        writer.WriteAttributeString("for", sFor);
        writer.WriteAttributeString("attr.name", attrName);
        writer.WriteAttributeString("attr.type", attrType);
        writer.WriteAttributeString("id", sID);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Serialises the MLN (multi-layer nested GraphML) to disk.
    /// </summary>
    public void writeGraphML()
    {
        string pathWithName = path + name.Split(".")[0] + "N.graphml";
        XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
        XmlWriter writer = XmlWriter.Create(pathWithName, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("graphml", "http://graphml.graphdrawing.org/xmlns");

        writeKey(writer, "node", "label", "string", "label");
        writeKey(writer, "node", "id", "string", "id");
        writeKey(writer, "edge", "weight", "double", "weight");
        writeKey(writer, "node", "x", "double", "x");
        writeKey(writer, "node", "y", "double", "y");
        writeKey(writer, "node", "z", "double", "z");
        writeKey(writer, "node", "r", "double", "r");
        writeKey(writer, "node", "g", "double", "g");
        writeKey(writer, "node", "b", "double", "b");

        writer.WriteStartElement("graph");
        writer.WriteAttributeString("id", name);

        foreach (Layer layer in mln.layers)
        {
            writeMLNLayer(layer, writer);
        }

        foreach (Edge edge in mln.edges)
        {
            writeEdge(edge, writer);
        }

        writer.WriteEndElement(); // graph
        writer.WriteEndElement(); // graphml
        writer.WriteEndDocument();

        writer.Flush();
        writer.Close();

        Debug.Log(name.Split(".")[0] + "NL.graphml stored");
    }

    /// <summary>
    /// Writes a layer node containing its inner graph of nodes.
    /// </summary>
    private void writeMLNLayer(Layer layer, XmlWriter writer)
    {
        writer.WriteStartElement("node");
        writer.WriteAttributeString("id", "" + layer.id);
        writeDataKeyElement("label", layer.label, writer);

        writer.WriteStartElement("graph");
        writer.WriteAttributeString("id", "" + layer.id);

        foreach (Node node in layer.nodes)
        {
            writeNode(node, writer);
        }

        writer.WriteEndElement(); // graph
        writer.WriteEndElement(); // node
    }

    /// <summary>
    /// Writes a node and its attributes, including visual position and colour if available.
    /// </summary>
    private void writeNode(Node node, XmlWriter writer)
    {
        writer.WriteStartElement("node");
        writer.WriteAttributeString("id", "" + node.id);

        writeDataKeyElement("label", node.label, writer);

        if (node.nodeVis)
        {
            writeDataKeyElement("x", node.nodeVis.transform.localPosition.x.ToString(), writer);
            writeDataKeyElement("y", node.nodeVis.transform.localPosition.y.ToString(), writer);
            writeDataKeyElement("z", node.nodeVis.transform.localPosition.z.ToString(), writer);

            writeDataKeyElement("r", node.nodeVis.color.r.ToString(), writer);
            writeDataKeyElement("g", node.nodeVis.color.g.ToString(), writer);
            writeDataKeyElement("b", node.nodeVis.color.b.ToString(), writer);
        }

        foreach (KeyValuePair<string, string> entry in node.attributes)
        {
            writeDataKeyElement(entry.Key, entry.Value, writer);
        }
        writer.WriteEndElement(); // node
    }

    /// <summary>
    /// Writes an edge with weight attribute.
    /// </summary>
    private void writeEdge(Edge edge, XmlWriter writer)
    {
        writer.WriteStartElement("edge");
        writer.WriteAttributeString("id", "" + edge.id);
        writer.WriteAttributeString("source", "" + edge.sourceNode.id);
        writer.WriteAttributeString("target", "" + edge.targetNode.id);

        writeDataKeyElement("weight", edge.getWeight().ToString(), writer);

        writer.WriteEndElement(); // edge
    }

    /// <summary>
    /// Helper to emit a &lt;data key=".."&gt;VALUE&lt;/data&gt; element.
    /// </summary>
    private static void writeDataKeyElement(string key, string value, XmlWriter writer)
    {
        writer.WriteStartElement("data");
        writer.WriteAttributeString("key", key);
        writer.WriteValue("" + value);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes the super-graph (nodes merged by label, edge weights summed) to disk.
    /// </summary>
    public void writeSuperGraph()
    {
        MLN supergraph = mln.createSuperGraph();
        string pathWithName = path + name + "superGraph.graphml";
        XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
        XmlWriter writer = XmlWriter.Create(pathWithName, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("graphml", "http://graphml.graphdrawing.org/xmlns");

        writeKey(writer, "node", "label", "string", "label");
        writeKey(writer, "node", "id", "string", "id");
        writeKey(writer, "edge", "weight", "double", "weight");
        writeKey(writer, "node", "x", "double", "x");
        writeKey(writer, "node", "y", "double", "y");
        writeKey(writer, "node", "z", "double", "z");
        writeKey(writer, "node", "r", "double", "r");
        writeKey(writer, "node", "g", "double", "g");
        writeKey(writer, "node", "b", "double", "b");

        writer.WriteStartElement("graph");
        writer.WriteAttributeString("id", name);

        foreach (Node node in supergraph.layers[0].nodes)
        {
            writeNode(node, writer);
        }

        foreach (Edge edge in supergraph.edges)
        {
            writeEdge(edge, writer);
        }

        writer.WriteEndElement(); // graph
        writer.WriteEndElement(); // graphml
        writer.WriteEndDocument();

        writer.Flush();
        writer.Close();

        Debug.Log(name + "superGraph.graphml stored");
    }

    // Legacy writer (kept for reference)
    private void writeMonoLayerGraphLayer() { }

    /// <summary>Legacy flat-MLN writer kept for reference.</summary>
    private void writeGraphMLold()
    {
        if (name == null) { generateName(); }

        string pathWithName = path + name + ".graphml";
        XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
        XmlWriter writer = XmlWriter.Create(pathWithName, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("mln");
        writer.WriteAttributeString("id", name);

        foreach (Layer layer in mln.layers)
        {
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("id", "" + layer.id);

            writer.WriteStartElement("data");
            writer.WriteAttributeString("key", "label");
            writer.WriteValue("" + layer.label);
            writer.WriteEndElement();

            foreach (Node node in layer.nodes)
            {
                writer.WriteStartElement("node");
                writer.WriteAttributeString("id", "" + node.id);

                writer.WriteStartElement("data");
                writer.WriteAttributeString("key", "label");
                writer.WriteValue("" + node.label);
                writer.WriteEndElement();

                foreach (KeyValuePair<string, string> entry in node.attributes)
                {
                    writer.WriteStartElement("data");
                    writer.WriteAttributeString("key", entry.Key);
                    writer.WriteValue("" + entry.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // node
            }
            writer.WriteEndElement(); // layer
        }

        foreach (Edge edge in mln.edges)
        {
            writer.WriteStartElement("edge");
            writer.WriteAttributeString("id", "" + edge.id);
            writer.WriteAttributeString("source", "" + edge.sourceNode.id);
            writer.WriteAttributeString("target", "" + edge.targetNode.id);

            writer.WriteStartElement("data");
            writer.WriteAttributeString("key", "weight");
            writer.WriteValue(edge.getWeight());
            writer.WriteEndElement(); // data

            writer.WriteEndElement(); // edge
        }

        writer.WriteEndElement(); // mln
        writer.WriteEndDocument();

        writer.Flush();
        writer.Close();
        Debug.Log("MLN stored: " + name);
    }

    /// <summary>Generates a default export name.</summary>
    private void generateName() { name = "tmp"; }
}
