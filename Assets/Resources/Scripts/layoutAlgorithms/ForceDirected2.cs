using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Fruchterman–Reingold-style force-directed layout constrained to a layer's bounding box in local space.
/// Uses a cooling schedule and applies repulsive (node-node) and attractive (edge) forces over a fixed iteration count.
/// </summary>
public class ForceDirected2
{
    /// <summary>Owning MLN instance.</summary>
    public MLN mln;

    /// <summary>Cooling/damping factor controlling the temperature schedule.</summary>
    public float dampingFactor = 1000f;

    /// <summary>Total number of iterations to perform.</summary>
    public float iterations = 1000f;

    private float layerSizeX;
    private float layerSizeY;
    private float layerSizeZ;
    private float area;
    private float temperature;
    private float optDistance;

    /// <summary>
    /// Constructs the layout helper for a given MLN.
    /// </summary>
    /// <param name="_mln">Owning MLN.</param>
    public ForceDirected2(MLN _mln)
    {
        mln = _mln;
    }

    /// <summary>
    /// Runs the force-directed layout on a single layer, updating node local positions in place.
    /// </summary>
    /// <param name="layer">Target layer to layout.</param>
    public void forceDirectedLayout(Layer layer)
    {
        layerSizeX = layer.layerVis.getScale().x;
        layerSizeY = layer.layerVis.getScale().y;
        layerSizeZ = layer.layerVis.getScale().z;

        area = layerSizeX * layerSizeY * layerSizeZ;
        temperature = layerSizeX / 5;
        optDistance = Mathf.Sqrt(area / layer.nodes.Count); // Characteristic spacing

        for (int i = 0; i < iterations; i++)
        {
            // Reset forces and apply repulsion
            foreach (Node nodeV in layer.nodes)
            {
                nodeV.nodeVis.force = Vector3.zero;

                foreach (Node nodeU in layer.nodes)
                {
                    if (nodeU != nodeV)
                    {
                        Vector3 difference =
                            (nodeV.nodeVis.transform.localPosition - nodeU.nodeVis.transform.localPosition) != Vector3.zero
                                ? nodeV.nodeVis.transform.localPosition - nodeU.nodeVis.transform.localPosition
                                : new Vector3(0.0001f, 0.0001f, 0.0001f);

                        nodeV.nodeVis.force += (difference / difference.magnitude) * repulse(nodeU, nodeV);
                    }
                }
            }

            // Edge attraction
            foreach (Edge edge in layer.edges)
            {
                Node sourceNodeU = edge.sourceNode;
                Node targetNodeV = edge.targetNode;
                Vector3 difference =
                    (targetNodeV.nodeVis.transform.localPosition - sourceNodeU.nodeVis.transform.localPosition) != Vector3.zero
                        ? targetNodeV.nodeVis.transform.localPosition - sourceNodeU.nodeVis.transform.localPosition
                        : new Vector3(0.0001f, 0.0001f, 0.0001f);

                targetNodeV.nodeVis.force -= difference.normalized * attract(sourceNodeU, targetNodeV);
                sourceNodeU.nodeVis.force += difference.normalized * attract(sourceNodeU, targetNodeV);
            }

            // Displacement, clamped to layer bounds with temperature margin
            foreach (Node node in layer.nodes)
            {
                node.nodeVis.transform.localPosition +=
                    node.nodeVis.force.normalized * Mathf.Min(node.nodeVis.force.magnitude, temperature);

                float x = Mathf.Min(layerSizeX / 2 - temperature, Mathf.Max(-layerSizeX / 2 + temperature, node.nodeVis.transform.localPosition.x));
                float y = Mathf.Min(layerSizeY / 2 - temperature, Mathf.Max(-layerSizeY / 2 + temperature, node.nodeVis.transform.localPosition.y));
                float z = Mathf.Min(layerSizeZ / 2 - temperature, Mathf.Max(-layerSizeZ / 2 + temperature, node.nodeVis.transform.localPosition.z));
                node.nodeVis.transform.localPosition = new Vector3(x, y, z);
            }

            temperature = coolAlt(i + 3);
        }
    }

    /// <summary>
    /// Repulsive force magnitude between nodes U and V.
    /// </summary>
    private float repulse(Node nodeU, Node nodeV)
    {
        Vector3 posNodeU = nodeU.nodeVis.transform.localPosition;
        Vector3 posNodeV = nodeV.nodeVis.transform.localPosition;
        float distance = Vector3.Distance(posNodeU, posNodeV) + 0.00001f;
        return optDistance * optDistance / distance;
    }

    /// <summary>
    /// Attractive force magnitude between connected nodes U and V.
    /// </summary>
    private float attract(Node nodeU, Node nodeV)
    {
        Vector3 posNodeU = nodeU.nodeVis.transform.localPosition;
        Vector3 posNodeV = nodeV.nodeVis.transform.localPosition;
        float distance = Vector3.Distance(posNodeU, posNodeV) + 0.00001f;
        return distance * distance / optDistance + temperature;
    }

    /// <summary>
    /// Cooling schedule: decreases the maximum displacement as iterations progress.
    /// </summary>
    private float coolAlt(int iteration)
    {
        float scaleFactor = 1.0f * iterations / dampingFactor;
        return scaleFactor / (iteration + scaleFactor);
    }
}
