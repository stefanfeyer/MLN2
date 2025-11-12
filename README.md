# Graph Analysis and Visualisation in Unity

An interactive framework for exploring **multilayer networks (MLNs)** in 2D, 2.5D, and 3D space — built entirely in **Unity (C#)**.  
It provides data import/export, multiple layout algorithms, and a modular visualisation pipeline for scientific and analytic use cases.

---

## 🎯 Overview

This Unity project implements a visual analytics tool for multilayer networks (e.g., animal behaviour, social networks, or multimodal datasets).  
It supports reading GraphML files, visualising layers, and running graph layout algorithms interactively.

**Key capabilities:**
- Load multilayer networks from GraphML (`.graphml`)
- Visualise nodes and edges across multiple layers
- Adjustable 2D, 2.5D, and 3D layer arrangements
- Node colouring by degree or attribute
- Edge thickness mapped to weight (absolute or relative)
- Graph export to GraphML (including aggregated “supergraph”)
- Unity editor integration and GUI control panel

---

## 🧩 Architecture

The framework follows a **Model-View-Controller** pattern adapted for Unity:

| Component | Role |
|------------|------|
| **MLN, Layer, Node, Edge** | Core data structures representing the network |
| **MLNVis, LayerVis, NodeVis, EdgeVis** | Visual representations in the Unity scene |
| **Layouts** (`ForceDirected*`, `StressMin`, `CircularLayout`) | Layout algorithms for node positioning |
| **I/O** (`ReadGraphML`, `WriteGraphML`) | GraphML import/export |
| **UI** (`MLNPanel`, `loadPanel`) | Control panel and runtime configuration |
| **Variables.cs** | Global settings (colours, scales, identifiers) |

All classes are written in **C#** with XML-style documentation comments for IDE integration.

---

## ⚙️ Layout Algorithms

### Force-Directed Layouts
Several versions (`ForceDirected2`–`ForceDirected6`) experiment with attraction/repulsion mechanics, optimised force accumulation, and layered constraints.

### Stress Minimisation
Implements iterative gradient descent to minimise positional “stress” between desired and actual node distances, producing compact, balanced layouts.

### Circular Layout
Places all nodes evenly on a circle per layer — ideal for small or symmetric graphs.

---

## 💡 Example Workflow

1. **Import Graph**
   - Place `.graphml` files under `Assets/Resources/Graphs/CurrentGraphs/`
   - Use the in-scene `loadPanel` to generate graph buttons

2. **Explore**
   - Run scene → MLN visualisation loads automatically
   - Toggle between 2D / 2.5D / 3D layer arrangements
   - Apply colouring, edge weight mapping, or layouts from the UI

3. **Export**
   - Save the current MLN and its supergraph as `.graphml` via the `Store Graph` button

---

## 🧠 Technologies

- **Engine:** Unity 2022+
- **Language:** C# (.NET 4.x)
- **Visuals:** Unity MeshRenderer, TextMeshPro
- **Data:** XML / GraphML
- **Input:** Unity XR Interaction Toolkit (optional)
- **UI:** Unity UI, Prefabs

---

## 🧭 Use Cases

- Network visualisation in VR/AR or 3D space  
- Exploratory data analysis of multilayer graphs  
- Research prototypes in **immersive analytics**, **social network analysis**, or **biological networks**  

---

## 🧑‍💻 Author

**Stefan Feyer**  
PhD Student in Computer Science – Immersive Analytics & Network Visualisation  
University of Konstanz  

> *This framework was developed for experimental analysis of multilayer networks and is adaptable for any graph-based visualisation project.*

---

## 📄 License

GPL License 
