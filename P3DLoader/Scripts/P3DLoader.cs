using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using NetP3DLib.P3D;

[System.Serializable]
public class Level
{
	//P3D file names without extension
	public List<string> files;
}

[System.Serializable]
public class Car
{
	public string name;
	public Vector3 position;
	public List<CarJoint> joints;
}

[System.Serializable]
public class CarJoint
{
	public string parent;
	public Vector3 position;
	public Quaternion rotation;
	public string model;
}

[System.Serializable]
public class CustomParent
{
	public string car;
	public string child;
	public string parent;
	public string model;
}

[System.Serializable]
public class CarWheels
{
	public string car;
	public List<Wheel> wheel;
}

[System.Serializable]
public class Wheel
{
	public string joint;
	public string model;
}

public class P3DLoader : MonoBehaviour
{
	//Game's art path
	public string gameArtPath = "D:/Games/The Simpsons - Hit & Run/art";
	
	//Loaded p3d
	private P3DFile p3dFile;
	
	[Space(8)]
	[Header("Models")]
	public List<Level> levels;
	public Vector3 levelChunkRotation; //Recommended: (0, 180, 0)
	public Vector3 objectChunkRotation; //Optional: (90, 0, 0)
	
	[Space(8)]
	[Header("Car")]
	public List<CarWheels> wheels;
	public List<CustomParent> customParents;
	
	//Total objects
	[HideInInspector] public List<GameObject> objects;
	
	public void LoadChunk(int level, string file)
    {
		string path = gameArtPath + "/" + file + ".p3d";
		
		if (File.Exists(path))
		{
			//Instantiate static mesh
			if (Resources.Load("Level " + level + "/" + file))
			{
				GameObject obj = Instantiate(Resources.Load("Level " + level + "/" + file) as GameObject, new Vector3(0, 0, 0), Quaternion.Euler(levelChunkRotation));
				objects.Add(obj);
			}
			
			//New p3d file from path
			p3dFile = new P3DFile(path);
			
			//Read root chunks
			if (p3dFile != null)
			{
				//Inst Stat Phys
				var instStatPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.InstStatPhysChunk>();
				
				foreach (var chunk in instStatPhysChunks)
				{
					InstanceList(level, chunk, chunk.Name);
				}
				
				//Dyna Phys
				var dynaPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.DynaPhysChunk>();
				
				foreach (var chunk in dynaPhysChunks)
				{
					InstanceList(level, chunk, chunk.Name);
				}
				
				//Anim Dyna Phys
				var animDynaPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.AnimDynaPhysChunk>();
				
				foreach (var chunk in animDynaPhysChunks)
				{
					InstanceList(level, chunk, chunk.Name);
				}
				
				//Locator
				var locatorChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.LocatorChunk>();
					
				foreach (var chunk in locatorChunks)
				{
					Locator(level, chunk);
				}
				
				//Skeleton
				var skeletonChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.SkeletonChunk>();
				
				foreach (var chunk in skeletonChunks)
				{
					Skeleton(level, chunk, null);
				}
				
				Debug.Log("Found p3d: " + file);
			}
		}
		else
		{
			Debug.Log("No p3d: " + file);
		}
    }
	
	void InstanceList(int level, Chunk chunk, string name)
	{
		//Instance List
		var instanceListChunks = chunk.GetChunksOfType<NetP3DLib.P3D.Chunks.InstanceListChunk>();
		
		foreach (var instanceList in instanceListChunks)
		{
			//Scene Graph
			var sceneGraphChunks = instanceList.GetChunksOfType<NetP3DLib.P3D.Chunks.ScenegraphChunk>();
			
			foreach (var sceneGraph in sceneGraphChunks)
			{
				//Scenegraph Root
				var sceneGraphChildren = sceneGraph.GetChunksOfType<NetP3DLib.P3D.Chunks.OldScenegraphRootChunk>();
				
				foreach (var root in sceneGraphChildren)
				{
					//Old Scenegraph Branch
					var sceneGraphBranchChunks = root.GetChunksOfType<NetP3DLib.P3D.Chunks.OldScenegraphBranchChunk>();
			
					foreach (var branch in sceneGraphBranchChunks)
					{
						//Old Scenegraph Transform
						var transforms = branch.GetChunksOfType<NetP3DLib.P3D.Chunks.OldScenegraphTransformChunk>();
						
						foreach (var transform in transforms)
						{
							//Old Scenegraph Transform
							var childTransforms = transform.GetChunksOfType<NetP3DLib.P3D.Chunks.OldScenegraphTransformChunk>();
						
							foreach (var childTransform in childTransforms)
							{
								//Matrix4x4 Transform data
								float M11 = childTransform.Transform.M11;
								float M12 = childTransform.Transform.M12;
								float M13 = childTransform.Transform.M13;
								float M14 = childTransform.Transform.M14;
								
								float M21 = childTransform.Transform.M21;
								float M22 = childTransform.Transform.M22;
								float M23 = childTransform.Transform.M23;
								float M24 = childTransform.Transform.M24;
								
								float M31 = childTransform.Transform.M31;
								float M32 = childTransform.Transform.M32;
								float M33 = childTransform.Transform.M33;
								float M34 = childTransform.Transform.M34;
								
								float M41 = childTransform.Transform.M41;
								float M42 = childTransform.Transform.M42;
								float M43 = childTransform.Transform.M43;
								float M44 = childTransform.Transform.M44;
								
								//Set position and rotation from matrix
								Matrix4x4 matrix = new Matrix4x4(new Vector4(M11, M12, M13, M14), new Vector4(M21, M22, M23, M24), new Vector4(M31, M32, M33, M34), new Vector4(M41, M42, M43, M44));
								Vector3 position = matrix.GetColumn(3);
								Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(1), matrix.GetColumn(2));
								
								//Instantiate resources if available by Name
								if (Resources.Load("Level " + level + "/" + name))
								{
									GameObject obj = Instantiate(Resources.Load("Level " + level + "/" + name) as GameObject, position, rotation);
									obj.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);
									
									//Add to objects list
									objects.Add(obj);
								}
								
								//Debug if data exists
								Debug.Log(childTransform.Name + ", position: " + position + ", rotation: " + rotation);
							}
						}
					}
				}
			}
		}		
	}
	
	void Locator(int level, NetP3DLib.P3D.Chunks.LocatorChunk chunk)
	{
		//Locator is Coin
		if (Convert.ToString(chunk.LocatorType) == "Coin")
		{
			//Get Position data
			Vector3 position = new Vector3(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
			
			//Instantiate resources if available by name
			if (Resources.Load("Level/coinShape_000"))
			{
				GameObject obj = Instantiate(Resources.Load("Level/coinShape_000") as GameObject, position, Quaternion.Euler(objectChunkRotation));
				
				//Add to objects list
				objects.Add(obj);
				
				//Rotate upright
				obj.transform.rotation = Quaternion.Euler(-90, 0, 0);
			}
			
			//Debug if data exists
			Debug.Log(chunk.Name + ", position: " + position);
		}
		
		//Locator is Car Start
		if (Convert.ToString(chunk.LocatorType) == "CarStart")
		{
			string path = "";
			
			if (!Convert.ToString(chunk.Name).Contains("_v"))
			{
				if (File.Exists(gameArtPath + "/cars/" + chunk.Name + "_v" + ".p3d"))
				{
					path = gameArtPath + "/cars/" + chunk.Name + "_v" + ".p3d";
				}
				else if (File.Exists(gameArtPath + "/cars/" + chunk.Name + ".p3d"))
				{
					path = gameArtPath + "/cars/" + chunk.Name + ".p3d";
				}
			}
			else
			{
				if (File.Exists(gameArtPath + "/cars/" + chunk.Name + "_v" + ".p3d"))
				{
					path = gameArtPath + "/cars/" + chunk.Name + "_v" + ".p3d";
				}
				else if (File.Exists(gameArtPath + "/cars/" + chunk.Name + ".p3d"))
				{
					path = gameArtPath + "/cars/" + chunk.Name + ".p3d";
				}
			}
				
			if (path != "")
			{
				//New p3d file from path
				var carP3d = new P3DFile(path);
				
				var skeletonChunks = carP3d.GetChunksOfType<NetP3DLib.P3D.Chunks.SkeletonChunk>();
				
				foreach (var skeleton in skeletonChunks)
				{
					//Get Position data
					Vector3 position = new Vector3(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
					
					//New car
					Car car = new Car();
					car.name = skeleton.Name;
					car.position = position + new Vector3(0, 1, 0);
					
					//Car skeleton
					List<GameObject> carSkeleton = Skeleton(level, skeleton, car);
					
					if (carSkeleton != null)
					{
						//Empty parent
						GameObject parent = new GameObject();
						parent.transform.position = car.position;
						parent.name = car.name;
					
						foreach (GameObject joint in carSkeleton)
						{
							joint.transform.SetParent(parent.transform);
						}
						
						//Add to objects list
						objects.Add(parent);
					}
					
					//Debug if data exists
					Debug.Log(chunk.Name + ", position: " + position);
				}
			}
		}
	}
	
	List<GameObject> Skeleton(int level, NetP3DLib.P3D.Chunks.SkeletonChunk chunk, Car car)
	{	
		List<GameObject> joints = new List<GameObject>();
		
		//Skeleton Joint
		var skeletonJointChunks = chunk.GetChunksOfType<NetP3DLib.P3D.Chunks.SkeletonJointChunk>();
		
		foreach (var skeletonJoint in skeletonJointChunks)
		{
			//Matrix4x4 Transform data
			float M11 = skeletonJoint.RestPose.M11;
			float M12 = skeletonJoint.RestPose.M12;
			float M13 = skeletonJoint.RestPose.M13;
			float M14 = skeletonJoint.RestPose.M14;
			
			float M21 = skeletonJoint.RestPose.M21;
			float M22 = skeletonJoint.RestPose.M22;
			float M23 = skeletonJoint.RestPose.M23;
			float M24 = skeletonJoint.RestPose.M24;
			
			float M31 = skeletonJoint.RestPose.M31;
			float M32 = skeletonJoint.RestPose.M32;
			float M33 = skeletonJoint.RestPose.M33;
			float M34 = skeletonJoint.RestPose.M34;
			
			float M41 = skeletonJoint.RestPose.M41;
			float M42 = skeletonJoint.RestPose.M42;
			float M43 = skeletonJoint.RestPose.M43;
			float M44 = skeletonJoint.RestPose.M44;
			
			//Set position and rotation from matrix
			Matrix4x4 matrix = new Matrix4x4(new Vector4(M11, M12, M13, M14), new Vector4(M21, M22, M23, M24), new Vector4(M31, M32, M33, M34), new Vector4(M41, M42, M43, M44));
			Vector3 position = matrix.GetColumn(3);
			Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(1), matrix.GetColumn(2));
			
			if (car == null)
			{
				//Instantiate resources if available by Name
				if (Resources.Load("Level " + level + "/" + skeletonJoint.Name + "Shape"))
				{
					GameObject obj = Instantiate(Resources.Load("Level " + level + "/" + skeletonJoint.Name + "Shape") as GameObject, position, rotation);
					obj.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);
				}
			}
			else
			{
				//New car joint
				CarJoint joint = new CarJoint();
				joint.parent = skeletonJoint.Name;
				joint.position = position;
				joint.rotation = rotation;
				joint.model = skeletonJoint.Name + "Shape";
				
				//Parent
				GameObject parent = new GameObject();
				parent.name = skeletonJoint.Name;
				parent.transform.position = car.position + position;
				parent.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);;
				
				List<string> carsWithCustomParents = new List<string>();
				
				//Set custom parent
				for (int i = 0; i < customParents.Count; i++)
				{
					carsWithCustomParents.Add(customParents[i].car);
					
					if (customParents[i].car == car.name)
					{
						//Base models
						if (customParents[i].child != skeletonJoint.Name)
						{
							JointCar(car, joint, parent);
						}
						else if (customParents[i].parent == skeletonJoint.Name) //Custom parents
						{
							if (customParents[i].child != skeletonJoint.Name)
							{
								joint.model = customParents[i].model;
								JointCar(car, joint, parent);
							}
						}
					}
				}
				
				//Base models for car without custom parents
				if (!carsWithCustomParents.Contains(car.name))
				{
					JointCar(car, joint, parent);
				}
				
				//Wheels
				for (int i = 0; i < wheels.Count; i++)
				{
					for (int a = 0; a < wheels[i].wheel.Count; a++)
					{
						if (wheels[i].car == car.name)
						{
							if (wheels[i].wheel[a].joint == skeletonJoint.Name)
							{
								joint.model = wheels[i].wheel[a].model;
								JointCar(car, joint, parent);
							}
						}
					}
				}
				
				//Add joint to parent GameObject list
				joints.Add(parent);
			}
			
			//Debug if data exists
			Debug.Log(skeletonJoint.Name + ", position: " + position + ", rotation: " + rotation);
		}
		
		return joints;
	}
	
	GameObject ModelCar(Car car, CarJoint joint)
	{
		if (Resources.Load("Cars/" + car.name + "/" + joint.model))
		{
			Quaternion rotation = Quaternion.Euler(joint.rotation.eulerAngles.x + objectChunkRotation.x, joint.rotation.eulerAngles.y + objectChunkRotation.y, joint.rotation.eulerAngles.z + objectChunkRotation.z);
	
			GameObject obj = Instantiate(Resources.Load("Cars/" + car.name + "/" + joint.model) as GameObject, car.position + joint.position, rotation);
			
			return obj;
		}
		
		return null;
	}
	
	void JointCar(Car car, CarJoint joint, GameObject parent)
	{
		GameObject carJoint = ModelCar(car, joint);
							
		if (carJoint != null)
		{
			carJoint.transform.SetParent(parent.transform, true);
		}
	}
}