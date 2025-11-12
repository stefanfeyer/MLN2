using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple force-directed layout variant intended for asynchronous/iterative calls (naming only).
/// Applies symmetrical repulsion among nodes and attraction along edges with optional centring.
/// </summary>
public class ForceDirected3Async
{
    /// <summary>Owning MLN (optional; not required for core step).</summary>
    public MLN mln;

    /// <summary>Strength of node-node repulsive force.</summary>
    public float repulsionStrength = 0.01f;

    /// <summary>Strength of edge attraction.</summary>
    public float attractionStrength = 0.01f;

    /// <summary>Velocity damping factor applied each step.</summary>
    public float damping = 0.85f;

    /// <summary>Clamp for per-step node displacement.</summary>
    public float maxMovement = 0.02f;

    /// <summary>Pull towards origin to keep nodes centred.</summary>
    public float centeringStrength = 1.0f;

    /// <summary>Number of iterations to run when <see cref="applyLayout"/> is called.</summary>
    public int iterations = 1000;

    /// <summary>Constructs the layout helper.</summary>
    public ForceDirected3Async() { }

    /// <summary>
    /// Executes multiple iterations of the layout for the given layer.
    /// </summary>
    /// <param name="layer">Target layer whose nodes will be updated.</param>
    public void applyLayout(Layer layer)
    {
        List<Node> nodesWithEdges = filterNodes(layer);
        for (int i = 0; i <= iterations; i++)
        {
            ApplyForces(layer, nodesWithEdges);
        }
    }

    /// <summary>
    /// Returns the set of nodes that participate in at least one edge on the layer.
    /// </summary>
    private List<Node> filterNodes(Layer layer)
    {
        List<Node> nodesWithEdges = new List<Node>();
        foreach (Edge edge in layer.edges)
        {
            if (!nodesWithEdges.Contains(edge.sourceNode))
            {
                nodesWithEdges.Add(edge.sourceNode);
            }
            if (!nodesWithEdges.Contains(edge.targetNode))
            {
                nodesWithEdges.Add(edge.targetNode);
            }
        }
        return nodesWithEdges;
    }

    /// <summary>
    /// Applies one iteration of forces to a set of nodes (repulsion, attraction, centring), then updates positions.
    /// </summary>
    /// <param name="layer">Layer providing edges.</param>
    /// <param name="nodes">Nodes to process (typically those with edges).</param>
    void ApplyForces(Layer layer, List<Node> nodes)
    {
        // Reset forces
        foreach (Node node in nodes)
        {
            node.nodeVis.force = Vector3.zero;
        }

        // Repulsive forces (all node pairs)
        foreach (Node node0 in nodes)
        {
            foreach (Node node1 in nodes)
            {
                Vector3 delta = node0.nodeVis.transform.localPosition - node1.nodeVis.transform.localPosition;
                float distance = delta.magnitude + 0.01f; // Avoid division by zero
                Vector3 repulsion = (repulsionStrength / distance) * delta.normalized;

                node0.nodeVis.force += repulsion;
                node1.nodeVis.force -= repulsion; // Equal and opposite
            }
        }

        // Attractive forces (edges)
        foreach (Edge edge in layer.edges)
        {
            Vector3 delta = edge.sourceNode.nodeVis.transform.localPosition - edge.targetNode.nodeVis.transform.localPosition;
            float distance = delta.magnitude + 0.01f;
            Vector3 attraction = (distance * attractionStrength) * delta.normalized;

            edge.sourceNode.nodeVis.force -= attraction;
            edge.targetNode.nodeVis.force += attraction;
        }

        // Integrate and clamp displacement
        foreach (Node node in nodes)
        {
            Vector3 toCenter = Vector3.zero - node.nodeVis.transform.localPosition;
            node.nodeVis.force += centeringStrength * toCenter;
            node.nodeVis.force = Vector3.ClampMagnitude(node.nodeVis.force * damping, maxMovement);
            node.nodeVis.transform.localPosition += node.nodeVis.force;
        }
    }
}
