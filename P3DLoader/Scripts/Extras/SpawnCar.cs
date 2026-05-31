using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SpawnCar : MonoBehaviour
{
	[HideInInspector] public List<string> cars;
	private Transform spawn;
	
	void Update()
	{
		if (spawn == null)
		{
			spawn = GameObject.Find("Spawn").transform;
		}
		
		//Input controls
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			cars.Add("rocke_v");
			Spawn(spawn.position);
		}
	}
	
	public void Spawn(Vector3 position)
	{
		if (cars.Count > 0)
		{
			foreach (string car in cars)
			{
				GetCar(car, position);
			}
			
			cars = new List<string>();
		}
	}
	
    void GetCar(string name, Vector3 position)
    {
        if (File.Exists(GetComponent<P3DLoader>().gameArtPath + "/cars/" + name + ".p3d"))
		{
			string path = GetComponent<P3DLoader>().gameArtPath + "/cars/" + name + ".p3d";
			
			GetComponent<P3DLoader>().CarSkeleton(path, position);
		}
    }
}
