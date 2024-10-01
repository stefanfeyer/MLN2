using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMLNVis : MonoBehaviour
{
    public MLN mln;
    bool init = false;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!init && mln != null)
        {
            init = true;
            createMLNVis();
        }
    }

    public void createMLNVis()
    {
        this.name = mln.label;
        createLayers();
        createEdges();
        
    }

    


    public void createLayers()
    {
        foreach (Layer layer in mln.layers)
        {
            GameObject layerVis = new GameObject(layer.label);
            layerVis.AddComponent<LayerVis>().layer = layer;
            layerVis.transform.SetParent(this.transform);
            createNodes(layer, layerVis);
        }
    }

    public void createNodes(Layer layer, GameObject layerVis)
    {
        foreach ( Node node in layer.nodes )
        {
            GameObject nodeVis = new GameObject(node.label);
            nodeVis.AddComponent<NodeVis>().node = node;
            nodeVis.transform.SetParent(layerVis.transform);
        }
    }

    public void createEdges()
    {
        GameObject edgeParent = new GameObject("edges");
        edgeParent.transform.SetParent(this.transform);
        foreach ( Edge edge in mln.edges )
        {
            GameObject edgeVis = new GameObject(edge.label);
            edgeVis.transform.SetParent(edgeParent.transform);
            edgeVis.AddComponent<EdgeVis>();
            edgeVis.GetComponent<EdgeVis>().edge = edge;
            edgeVis.GetComponent<EdgeVis>().mln = mln;
        }
    }
}
