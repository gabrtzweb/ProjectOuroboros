using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HighlightMesh : MonoBehaviour {
    void Start() {
        Mesh mesh = new Mesh();
        
        Vector3[] verts = new Vector3[] {
            new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0),
            new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1)
        };
        
        int[] indices = new int[] {
            0,1, 1,2, 2,3, 3,0, // Fundo
            4,5, 5,6, 6,7, 7,4, // Topo
            0,4, 1,5, 2,6, 3,7  // Pilares
        };
        
        mesh.vertices = verts;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
