using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Simple stress minimisation layout: computes desired pairwise distances per layer (based on edge weights)
/// and iteratively moves nodes to reduce the difference between actual and desired distances.
/// </summary>
public class StressMin
{
    /// <summary>Target MLN.</summary>
    public MLN mln;

    /// <summary>Desired distances for node pairs, per layer.</summary>
    public Dictionary<(Node, Node), float> desiredDistances = new Dictionary<(Node, Node), float>();

    /// <summary>Learning rate for gradient-style updates.</summary>
    public float learningrate = 0.01f;

    /// <summary>Constructs and runs the layout.</summary>
    public StressMin(MLN mln)
    {
        this.mln = mln;
        applyLayout();
    }

    /// <summary>
    /// Computes desired distances and adjusts node positions.
    /// </summary>
    public void applyLayout()
    {
        calculateDesiredDistances();
        // adjustNodePositions();
        adjustNodePositionsOpt();
    }

    /// <summary>
    /// Sets desired distances by edge presence/weight: closer for stronger edges, farther otherwise.
    /// </summary>
    private void calculateDesiredDistances()
    {
        foreach (Layer layer in mln.layers)
        {
            foreach (Node node1 in layer.nodes)
            {
                foreach (Node node2 in layer.nodes)
                {
                    if (node1 == node2) { continue; }
                    List<Edge> edges = layer.getEdgesBetween(node1, node2);
                    if (edges.Count > 0)
                    {
                        desiredDistances[(node1, node2)] = Mathf.Min(0.1f * (1 / edges[0].weight), 0.3f);
                    }
                    else
                    {
                        desiredDistances[(node1, node2)] = 0.6f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Naïve iterative position update using pairwise forces (kept for reference).
    /// </summary>
    private void adjustNodePositions()
    {
        for (int i = 0; i < 100; i++)
        {
            foreach (Layer layer in mln.layers)
            {
                foreach (Node node1 in layer.nodes)
                {
                    foreach (Node node2 in layer.nodes)
                    {
                        if (node1 == node2) { continue; }
                        float actual = Vector3.Distance(node1.nodeVis.transform.localPosition, node2.nodeVis.transform.localPosition);
                        float desired = desiredDistances[(node1, node2)];
                        float diff = actual - desired;

                        Vector3 direction = (node1.nodeVis.transform.localPosition - node2.nodeVis.transform.localPosition).normalized;
                        node1.nodeVis.force += diff * direction;
                        node2.nodeVis.force -= diff * direction;
                    }
                }
            }

            foreach (Layer layer in mln.layers)
            {
                foreach (Node node in layer.nodes)
                {
                    node.nodeVis.transform.position -= learningrate * node.nodeVis.force;
                    node.nodeVis.force = Vector3.zero;
                }
            }
        }
    }

    /// <summary>
    /// Optimised update: accumulates forces for each pair once per iteration, then applies updates.
    /// </summary>
    private void adjustNodePositionsOpt()
    {
        for (int iteration = 0; iteration < 100; iteration++)
        {
            // 1) Accumulate pairwise forces
            foreach (Layer layer in mln.layers)
            {
                List<Node> nodes = layer.nodes;
                int nodeCount = nodes.Count;

                for (int i = 0; i < nodeCount; i++)
                {
                    Node node1 = nodes[i];
                    Vector3 p1 = node1.nodeVis.transform.localPosition;

                    for (int j = i + 1; j < nodeCount; j++)
                    {
                        Node node2 = nodes[j];
                        Vector3 p2 = node2.nodeVis.transform.localPosition;

                        Vector3 dir = p1 - p2;
                        float actualDist = dir.magnitude;
                        float desiredDist = desiredDistances[(node1, node2)];
                        float diff = actualDist - desiredDist;

                        if (actualDist > 0.0001f)
                        {
                            Vector3 force = (diff / actualDist) * dir;
                            node1.nodeVis.force += force;
                            node2.nodeVis.force -= force;
                        }
                    }
                }
            }

            // 2) Apply position updates
            foreach (Layer layer in mln.layers)
            {
                foreach (Node node in layer.nodes)
                {
                    node.nodeVis.transform.localPosition -= learningrate * node.nodeVis.force;
                    node.nodeVis.force = Vector3.zero;
                }
            }
        }
    }
}
