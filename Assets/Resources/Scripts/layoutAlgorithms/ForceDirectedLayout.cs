using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MonoBehaviour that runs a basic force-directed layout across all layers when toggled.
/// Uses inverse-square repulsion and log-based spring attraction.
/// </summary>
public class ForceDirectedLayout : MonoBehaviour
{
    /// <summary>Active MLN to lay out.</summary>
    public MLN mln;

    /// <summary>Repulsion constant for node-node forces.</summary>
    public float repulsionConstant = 2f;

    /// <summary>Spring constant for edge attraction.</summary>
    public float springConstant = 1f;

    /// <summary>Per-step damping applied during integration.</summary>
    public float damping = 0.99f;

    /// <summary>Target spring length used by the log-based spring model.</summary>
    public float idealSpringLength = 0.2f;

    /// <summary>Toggle to run/stop the layout during Update().</summary>
    public bool doIt = false;

    /// <summary>Initialisation flag (unused here).</summary>
    public bool isInit = false;

    private void start()
    {
        // Reserved for optional initialisation.
    }

    int iterations = 1000;

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            doIt = !doIt;
        }
        if (doIt)
        {
            doit();
        }
    }

    /// <summary>
    /// Performs one layout step: clear forces, apply repulsion and springs, then update positions.
    /// </summary>
    private void doit()
    {
        if (iterations >= 0)
        {
            iterations--;
            foreach (Node node in mln.nodes)
            {
                node.nodeVis.force = Vector3.zero;
            }
            ApplyRepulsiveForces();
            ApplySpringForces();
            UpdateNodePositions();
        }
    }

    /// <summary>
    /// Applies inverse-square repulsive forces between all pairs of nodes per layer.
    /// </summary>
    private void ApplyRepulsiveForces()
    {
        foreach (Layer layer in mln.layers)
        {
            foreach (Node nodeV in layer.nodes)
            {
                foreach (Node nodeU in layer.nodes)
                {
                    if (nodeU != nodeV)
                    {
                        Vector3 difference = nodeV.nodeVis.transform.localPosition - nodeU.nodeVis.transform.localPosition;
                        float distance = difference.magnitude;
                        if (distance < 0.01f) distance = 0.01f;
                        Vector3 direction = difference.normalized;
                        Vector3 force = (repulsionConstant / (distance * distance)) * direction;
                        nodeV.nodeVis.force += force;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Applies spring-like attraction along edges using a logarithmic force model.
    /// </summary>
    private void ApplySpringForces()
    {
        foreach (Layer layer in mln.layers)
        {
            foreach (Edge edge in layer.edges)
            {
                Node nodeU = edge.sourceNode;
                Node nodeV = edge.targetNode;

                Vector3 difference = nodeV.nodeVis.transform.localPosition - nodeU.nodeVis.transform.localPosition;
                float distance = difference.magnitude;
                if (distance < 0.01f) distance = 0.01f;
                Vector3 direction = difference.normalized;

                Vector3 force = springConstant * Mathf.Log10(distance / idealSpringLength) * direction;

                Vector3 attractionForce = force - nodeU.nodeVis.force;

                nodeU.nodeVis.force = nodeU.nodeVis.force + attractionForce;
                nodeV.nodeVis.force = nodeV.nodeVis.force - attractionForce;
            }
        }
    }

    /// <summary>
    /// Integrates per-node forces into positions with clamped step size.
    /// </summary>
    private void UpdateNodePositions()
    {
        foreach (Node node in mln.nodes)
        {
            Debug.Log("force: " + (node.nodeVis.force.normalized * Mathf.Min(node.nodeVis.force.magnitude, (1f / 5f))));
            node.nodeVis.transform.localPosition =
                node.nodeVis.transform.localPosition +
                (node.nodeVis.force.normalized * Mathf.Min(node.nodeVis.force.magnitude, (1f / 5f)));
        }
    }
}
