using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multilayer network container with nodes, layers and edges, including helpers for degree and weight ranges,
/// inter-layer/self-edge tracking, and super-graph creation.
/// </summary>
public class MLN
{
    /// <summary>Unique identifier.</summary>
    public string id;

    /// <summary>Display label.</summary>
    public string label;

    /// <summary>All nodes across all layers.</summary>
    public List<Node> nodes = new List<Node>();

    /// <summary>All layers in this MLN.</summary>
    public List<Layer> layers = new List<Layer>();

    /// <summary>Fast lookup for layers by ID.</summary>
    public Dictionary<string, Layer> layersDict = new Dictionary<string, Layer>();

    /// <summary>All edges across all layers (including inter-layer edges).</summary>
    public List<Edge> edges = new List<Edge>();

    /// <summary>Whether the MLN has been fully initialised (unused in this snippet).</summary>
    public bool isComplete = false;

    /// <summary>Top-level visual associated with this MLN.</summary>
    public MLNVis mlnVis;

    /// <summary>Edges linking nodes from different layers.</summary>
    public List<Edge> edgesBetweenLayers = new List<Edge>();

    /// <summary>Edges linking nodes with the same label across layers.</summary>
    public List<Edge> selfEdges = new List<Edge>();

    /// <summary>Optional aggregate view built via <see cref="createSuperGraph"/>.</summary>
    public MLN superGraph;

    private float maxEdgeWeight;
    private float minEdgeWeight;

    private bool isMinEdgeWeightInitialised = false;
    private bool isMaxEdgeWeightInitialised = false;

    /// <summary>Fast lookup for nodes by ID.</summary>
    Dictionary<string, Node> nodesDict = new Dictionary<string, Node>();

    /// <summary>
    /// Constructs an MLN with the given ID.
    /// </summary>
    public MLN(string _id)
    {
        id = _id;
        label = _id;
    }

    /// <summary>Adds a layer and indexes it by ID.</summary>
    public void addLayer(Layer layer)
    {
        layers.Add(layer);
        layersDict.Add(layer.id, layer);
    }

    /// <summary>Adds an edge to the global list.</summary>
    public void addEdge(Edge edge)
    {
        edges.Add(edge);
    }

    /// <summary>Adds a node and indexes it by ID.</summary>
    public void addNode(Node node)
    {
        nodesDict.TryAdd(node.id, node);
        nodes.Add(node);
    }

    /// <summary>Returns the maximum node degree in the MLN.</summary>
    public int getMaxNodeDegree()
    {
        int maxNodeDegree = int.MinValue;
        foreach (Node node in nodes)
        {
            maxNodeDegree = Mathf.Max(node.getDegree(), maxNodeDegree);
        }
        return maxNodeDegree;
    }

    /// <summary>Returns the minimum node degree in the MLN.</summary>
    public int getMinNodeDegree()
    {
        int minNodeDegree = int.MaxValue;
        foreach (Node node in nodes)
        {
            minNodeDegree = Mathf.Min(node.getDegree(), minNodeDegree);
        }
        return minNodeDegree;
    }

    /// <summary>Tracks an inter-layer edge.</summary>
    public void addEdgeBetweenLayers(Edge edge)
    {
        edgesBetweenLayers.Add(edge);
    }

    /// <summary>Removes an inter-layer edge from tracking.</summary>
    public void removeEdgeBetweenLayers(Edge edge)
    {
        edgesBetweenLayers.Remove(edge);
    }

    /// <summary>Tracks a self-edge (same label across layers).</summary>
    public void addSelfEdge(Edge edge)
    {
        selfEdges.Add(edge);
    }

    /// <summary>Removes a self-edge from tracking.</summary>
    public void removeSelfEdge(Edge edge)
    {
        selfEdges.Remove(edge);
    }

    /// <summary>Returns a layer by ID, or throws if not found.</summary>
    public Layer getLayerByID(string _id)
    {
        foreach (Layer layer in layers)
        {
            if (layer.id == _id)
            {
                return layer;
            }
        }
        Debug.Log("layer ID: " + _id);
        throw new MissingReferenceException("layer not found");
    }

    /// <summary>Returns a node by ID, or null if missing.</summary>
    public Node getNodeByID(string _id)
    {
        if (nodesDict.ContainsKey(_id))
        {
            return nodesDict[_id];
        }
        else
        {
            Debug.Log("Node not found: " + _id);
            return null;
        }
    }

    /// <summary>
    /// Creates self-edges between nodes sharing the same label across adjacent layers.
    /// </summary>
    public void createSelfEdges()
    {
        int id = 0;
        for (int i = 0; i < layers.Count - 1; i++)
        {
            foreach (Node node in layers[i].nodes)
            {
                for (int j = 1; j < layers.Count; j++)
                {
                    if (i + j == layers.Count)
                    {
                        break;
                    }
                    if (layers[i + j].getNodeByLabel(node.label) != null)
                    {
                        Edge edge = new Edge("selfEdge" + id, node, layers[i + 1].getNodeByLabel(node.label), this);
                        id++;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>Returns the maximum edge weight across the MLN (cached).</summary>
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
            int counter = 0;
            foreach (Edge edge in edges)
            {
                maxEdgeWeight = Mathf.Max(edge.getWeight(), maxEdgeWeight);
                // if (edge.getWeight() > 15) counter++;
            }
            return maxEdgeWeight;
        }
    }

    /// <summary>Returns the minimum edge weight across the MLN (cached).</summary>
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

    /// <summary>
    /// Builds a "super graph" that merges nodes with identical labels across all layers and sums parallel edge weights.
    /// </summary>
    /// <returns>The aggregated super-graph MLN.</returns>
    public MLN createSuperGraph()
    {
        superGraph = new MLN(id + "SuperGraph");
        Layer sgLayer = new Layer("sgLayer" + id, superGraph);

        foreach (Layer oLayer in layers)
        {
            foreach (Node oNode in oLayer.nodes)
            {
                if (sgLayer.getNodeByLabel(oNode.label) == null)
                {
                    Node nNode = new Node(oNode.label, oNode.label, sgLayer, superGraph);
                }
            }
        }

        int edgeID = 0;
        foreach (Layer oLayer in layers)
        {
            foreach (Edge oEdge in oLayer.edges)
            {
                Node sgSourceNode = sgLayer.getNodeByLabel(oEdge.sourceNode.label);
                Node sgTargetNode = sgLayer.getNodeByLabel(oEdge.targetNode.label);
                List<Edge> edgeList = sgLayer.getEdgesBetween(sgSourceNode, sgTargetNode);

                if (edgeList.Count > 1) Debug.Log("more than one edge in sgLayer between 2 nodes");

                if (edgeList.Count == 0)
                {
                    Edge edge = new Edge(edgeID++.ToString(), sgSourceNode, sgTargetNode, superGraph);
                    edge.setWeight(oEdge.weight);
                }
                if (edgeList.Count == 1)
                {
                    edgeList[0].weight += oEdge.weight;
                }
            }
        }
        return superGraph;
    }
}
