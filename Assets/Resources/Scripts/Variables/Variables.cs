using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global configuration/static variables for MLN visualisation and layout.
/// </summary>
public static class Variables
{
    // Layer arrangement identifiers
    public const string ID2DLA = "2D";
    public const string ID25DLA = "25D";
    public const string ID3DLA = "3D";

    // Layer dimension identifiers
    public const string ID1DLD = "1D";
    public const string ID2DLD = "2D";
    public const string ID3DLD = "3D";

    // Visual scales
    public static float initialNodeScaleFactor = 1f;
    public static float minEdgeWidth = 0.1f;
    public static float maxEdgeWidth = 0.2f;

    // Layer distances and scaling
    public static float initialLayerDistance = 1f;
    public static float distanceBetweenLayer2DFactor = 1.15f;
    public static float distanceBetweenLayer25DFactor = 0.4f;
    public static float distanceBetweenLayer3DFactor = 1f;
    public static float layerScaleFactor = 1f;

    // Scene defaults
    public static Vector3 CENTER = Vector3.zero;

    // Edge colours for weight gradients
    public static Color LOWWEIGHTEDGECOLOR = Color.green;
    public static Color HIGHWEIGHTEDGECOLOR = Color.blue;

    // Sorting layer / draw-order handling
    public static bool isLayerDrawingHandlingActive = true;

    public static int layerBoundaryDrawingSortingOrder = 0;
    public static int edgeDrawingSortingOrder = 1;
    public static int nodeDrawingSortingOrder = 2;
    public static int labelDrawingSortingOrder = 3;
}
