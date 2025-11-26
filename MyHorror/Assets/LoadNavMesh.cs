using UnityEngine;
using UnityEngine.AI;

public class LoadNavMesh : MonoBehaviour
{
    public NavMeshData navMeshAsset; // drag your prebuilt NavMeshData.asset here
    public Vector3 navMeshPosition = Vector3.zero;
    public Quaternion navMeshRotation = Quaternion.identity;

    void Start()
    {
        // Add the NavMeshData at a specific position and rotation
        NavMeshDataInstance instance = NavMesh.AddNavMeshData(navMeshAsset, navMeshPosition, navMeshRotation);

        if (instance.valid)
            Debug.Log("NavMeshData added successfully!");
        else
            Debug.LogWarning("Failed to add NavMeshData!");
    }
}
