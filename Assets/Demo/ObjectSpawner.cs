using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject prefabToSpawn;
	public Transform spawnPoint;

	public void Spawn()
	{
		if (prefabToSpawn)
		{
			Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
		}
	}
}
