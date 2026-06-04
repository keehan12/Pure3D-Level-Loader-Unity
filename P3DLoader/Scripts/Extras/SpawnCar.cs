using UnityEngine;
using System.IO;
using System.Collections.Generic;
using NetP3DLib.P3D;

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
		else
		{
			//Input controls
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				cars.Add("rocke_v");
				Spawn(spawn.position - new Vector3(0, 2, 0));
			}
			
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				cars.Add("sedanA");
				Spawn(spawn.position - new Vector3(0, 2, 0));
			}
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
			
			//New p3d file from path
			P3DFile p3dFile = new P3DFile(path);
			
			GetComponent<P3DLoader>().CarSkeleton(name, p3dFile, position);
		}
    }
}
