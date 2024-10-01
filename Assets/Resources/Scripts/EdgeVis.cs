using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeVis : MonoBehaviour
{
    private bool init = false;
    public Edge edge;
    public MLN mln;
    private bool lineRendererInit = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!init && edge != null && mln != null)
        {
            init = true;
            initialise();
        }
        if (lineRendererInit)
        {
            drawEdge();
        }
        
    }

    private void initialise()
    {
        edge.edgeVis = this;
        this.name = edge.id;
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lineRenderer.material = go.GetComponent<MeshRenderer>().material;
        Destroy(go);
        lineRenderer.material = Resources.Load<Material>("Materials/edgeMaterial");
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = 2;
        lineRendererInit = true;
    }
    
    private void drawEdge()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, mln.getNode(edge.source).nodeVis.transform.position);
        lineRenderer.SetPosition(1, mln.getNode(edge.target).nodeVis.transform.position);
    }
}
