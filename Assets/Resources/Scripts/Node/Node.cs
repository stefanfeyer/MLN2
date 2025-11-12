using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logical node in an MLN with attributes, neighbours, and optional initial visual state.
/// </summary>
public class Node
{
    /// <summary>Unique identifier.</summary>
    public string id;

    /// <summary>Display label.</summary>
    public string label;

    /// <summary>Neighbour list (duplicated with <see cref="hashNeighbors"/> for quick tests).</summary>
    public List<Node> neighbors = new List<Node>();

    /// <summary>Incident edges.</summary>
    public List<Edge> edges = new List<Edge>(); // replace as hashList

    /// <summary>Set of neighbours for quick membership checks.</summary>
    public HashSet<Node> hashNeighbors = new HashSet<Node>();

    /// <summary>Visual component.</summary>
    public NodeVis nodeVis;

    /// <summary>Owning layer.</summary>
    public Layer layer;

    /// <summary>Arbitrary attributes (e.g., metadata, categories).</summary>
    public Dictionary<string, string> attributes = new Dictionary<string, string>();

    /// <summary>Owning MLN.</summary>
    public MLN mln;

    /// <summary>Optional initial position to apply when spawning visuals.</summary>
    public Vector3 initialPosition = Vector3.zero;

    /// <summary>True if <see cref="initialPosition"/> is valid.</summary>
    public bool hasInitialPosition = false;

    /// <summary>Optional initial colour to apply when spawning visuals.</summary>
    public Color initalColor = new Color(0, 0, 0, 1);

    /// <summary>True if <see cref="initalColor"/> is valid.</summary>
    public bool hasInitialColor = false;

    /// <summary>Creates a node with ID=label and registers it with the MLN.</summary>
    public Node(string _id, MLN _mln)
    {
        id = _id;
        label = _id;
        mln = _mln;
        mln.addNode(this);
    }

    /// <summary>Creates a node with explicit label and registers it with the MLN.</summary>
    public Node(string _id, string _label, MLN _mln)
    {
        id = _id;
        label = _label;
        mln = _mln;
        mln.addNode(this);
    }

    /// <summary>Creates a node on a layer and registers with layer + MLN.</summary>
    public Node(string _id, string _label, Layer _layer, MLN _mln)
    {
        id = _id;
        label = _label;

        layer = _layer;
        layer.addNode(this);

        mln = _mln;
        mln.addNode(this);
    }

    /// <summary>Creates a node (ID=label) on a layer and registers with layer + MLN.</summary>
    public Node(string _id, Layer _layer, MLN _mln)
    {
        id = _id;
        label = _id;

        layer = _layer;
        layer.addNode(this);

        mln = _mln;
        mln.addNode(this);
    }

    /// <summary>Adds a neighbour by node reference (kept for compatibility; not hash-based).</summary>
    public void addNeighbour(Node node)
    {
        if (!neighbors.Contains(node))
        {
            neighbors.Add(node);
        }
    }

    /// <summary>Removes a neighbour from the list.</summary>
    public void removeNeighbour(Node node)
    {
        neighbors.Remove(node);
    }

    /// <summary>Returns degree based on the neighbour list.</summary>
    public int getDegree()
    {
        return neighbors.Count;
    }

    /// <summary>Sets the display label.</summary>
    public void setLabel(string _label)
    {
        label = _label;
    }

    /// <summary>Returns the display label.</summary>
    public string getLabel() { return label; }

    /// <summary>Returns all attribute keys.</summary>
    public List<string> getAttributeNames()
    {
        return new List<string>(attributes.Keys);
    }

    /// <summary>Gets an attribute by key (throws if missing).</summary>
    public string getAttribute(string key)
    {
        return attributes[key];
    }

    /// <summary>Adds an attribute key/value pair.</summary>
    public void addAttribute(string key, string value)
    {
        attributes.Add(key, value);
    }

    /// <summary>Adds an incident edge and updates neighbour sets.</summary>
    public void addEdge(Edge edge)
    {
        edges.Add(edge);
        addNeighbour(edge);
    }

    /// <summary>Updates neighbour structures based on an incident edge.</summary>
    public void addNeighbour(Edge edge)
    {
        if (edge.sourceNode == this) { hashNeighbors.Add(edge.targetNode); addNeighbour(edge.targetNode); }
        if (edge.targetNode == this) { hashNeighbors.Add(edge.sourceNode); addNeighbour(edge.sourceNode); }
    }

    /// <summary>Sets the initial colour used by <see cref="NodeVis"/>.</summary>
    public void setInitialColor(Color color)
    {
        hasInitialColor = true;
        initalColor = color;
    }
}
