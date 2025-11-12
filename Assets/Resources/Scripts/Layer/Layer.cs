using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

/// <summary>
/// Logical layer of an MLN containing nodes and intra-layer edges, with cached min/max edge weights and density.
/// </summary>
public class Layer
{
    /// <summary>Owning MLN.</summary>
    public MLN mln;

    /// <summary>Unique identifier.</summary>
    public string id;

    /// <summary>Display label.</summary>
    public string label;

    /// <summary>Nodes contained in this layer.</summary>
    public List<Node> nodes = new List<Node>();

    /// <summary>Intra-layer edges.</summary>
    public List<Edge> edges = new List<Edge>();

    /// <summary>Visual component for this layer.</summary>
    public LayerVis layerVis;

    /// <summary>Cached minimum edge weight (initialised on first access).</summary>
    public float minEdgeWeight = 0f;

    /// <summary>Cached maximum edge weight (initialised on first access).</summary>
    public float maxEdgeWeight = 0f;

    /// <summary>Layer density in [0,1], computed via <see cref="calculateLayerDensity"/>.</summary>
    public float layerDensity01 = float.NaN;

    /// <summary>Fast lookup for edges by ID.</summary>
    public Dictionary<string, Edge> edgesDict = new Dictionary<string, Edge>();

    /// <summary>Fast lookup for nodes by ID.</summary>
    public Dictionary<string, Node> nodesDict = new Dictionary<string, Node>();

    private bool isMinEdgeWeightInitialised = false;
    private bool isMaxEdgeWeightInitialised = false;

    /// <summary>
    /// Creates a layer with the given ID. Not added to an MLN automatically.
    /// </summary>
    public Layer(string _id)
    {
        id = _id;
        label = _id;
    }

    /// <summary>
    /// Creates a layer and registers it with the provided MLN.
    /// </summary>
    public Layer(string _id, MLN _mln)
    {
        id = _id;
        label = _id;
        mln = _mln;
        mln.addLayer(this);
    }

    /// <summary>
    /// Creates a layer with explicit label and registers it with the MLN.
    /// </summary>
    public Layer(string _id, string _label, MLN _mln)
    {
        id = _id;
        label = _label;
        mln = _mln;
        mln.addLayer(this);
    }

    /// <summary>Adds a node to this layer and sets its back-reference.</summary>
    public void addNode(Node node)
    {
        nodes.Add(node);
        nodesDict.TryAdd(node.id, node);
        node.layer = this;
    }

    /// <summary>Removes a node from this layer.</summary>
    public void removeNode(Node node)
    {
        nodes.Remove(node);
        node.layer = null;
    }

    /// <summary>Adds an intra-layer edge and caches it in <see cref="edgesDict"/>.</summary>
    public void addEdge(Edge edge)
    {
        edges.Add(edge);
        edgesDict.TryAdd(edge.id, edge);
    }

    /// <summary>Removes an intra-layer edge.</summary>
    public void removeEdge(Edge edge)
    {
        edges.Remove(edge);
    }

    /// <summary>Returns true if the node is part of this layer.</summary>
    public bool containsNode(Node node)
    {
        if (nodes.Contains(node)) return true;
        return false;
    }

    /// <summary>Returns the maximum edge weight on this layer (cached after first pass).</summary>
    public float getMaxEdgeWeight()
    {
        if (isMaxEdgeWeightInitialised)
        {
            return maxEdgeWeight;
        }
        else
        {
            isMaxEdgeWeightInitialised = true;
            maxEdgeWeight = float.NegativeInfinity;
            foreach (Edge edge in edges)
            {
                maxEdgeWeight = Mathf.Max(edge.getWeight(), maxEdgeWeight);
            }
            return maxEdgeWeight;
        }
    }

    /// <summary>Returns the minimum edge weight on this layer (cached after first pass).</summary>
    public float getMinEdgeWeight()
    {
        if (isMinEdgeWeightInitialised)
        {
            return minEdgeWeight;
        }
        else
        {
            isMinEdgeWeightInitialised = true;
            minEdgeWeight = float.PositiveInfinity;
            foreach (Edge edge in edges)
            {
                minEdgeWeight = Mathf.Min(edge.getWeight(), minEdgeWeight);
            }
            return minEdgeWeight;
        }
    }

    /// <summary>Total number of nodes.</summary>
    public float getNumberOfNodes()
    {
        return nodes.Count;
    }

    /// <summary>Total number of intra-layer edges.</summary>
    public float getNumberOfEdges()
    {
        return edges.Count;
    }

    /// <summary>Finds a node by ID with linear search.</summary>
    public Node getNodeByID(string _id)
    {
        foreach (Node node in nodes)
        {
            if (node.id == _id)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>Returns the first node with the given label, or null if not found.</summary>
    public Node getNodeByLabel(string _label)
    {
        foreach (Node node in nodes)
        {
            if (node.label == _label)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Computes layer density (m/(n·(n-1)/2)) and stores in <see cref="layerDensity01"/>.
    /// </summary>
    public void calculateLayerDensity()
    {
        float potentialConnections = (nodes.Count * (nodes.Count - 1)) / 2;
        float actualConnections = edges.Count;
        layerDensity01 = actualConnections / potentialConnections;
    }

    /// <summary>True if this exact edge instance exists on the layer.</summary>
    public bool containsEdge(Edge _edge)
    {
        return edges.Contains(_edge);
    }

    /// <summary>True if the two nodes are neighbours (based on the first node's neighbour set).</summary>
    public bool areNodesNeighbours(Node node1, Node node2)
    {
        return node1.hashNeighbors.Contains(node2);
    }

    /// <summary>Returns the edge by ID if present, otherwise null.</summary>
    public Edge getEdgeByID(string id)
    {
        if (edgesDict.ContainsKey(id))
        {
            return edgesDict[id];
        }
        return null;
    }

    /// <summary>
    /// Returns all directed edges between node1→node2 and node2→node1 that are stored on their node edge lists.
    /// </summary>
    public List<Edge> getEdgesBetween(Node node1, Node node2)
    {
        List<Edge> edges = new List<Edge>();
        foreach (Edge edge in node1.edges)
        {
            if (edge.targetNode == node2)
            {
                edges.Add(edge);
            }
        }
        foreach (Edge edge in node2.edges)
        {
            if (edge.targetNode == node1)
            {
                edges.Add(edge);
            }
        }
        return edges;
    }
}
