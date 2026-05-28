using UnityEngine;
using System.Collections.Generic;
using System.IO;
using NetP3DLib.P3D;

[System.Serializable]
public class Level
{
	//P3D file names without extension
	public List<string> files;
}

public class P3DLoader : MonoBehaviour
{
	//Game's art path
	public string gameArtPath = "D:/Games/The Simpsons - Hit & Run/art";
	
	//Loaded p3d
	private P3DFile p3dFile;
	
	//Levels lists
	public List<Level> levels;
	
	//Offset
	public Vector3 levelChunkRotation; //Recommended: (0, 180, 0)
	public Vector3 objectChunkRotation; //Optional: (90, 0, 0)
	
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
		//Locator is coin by Name
		if (chunk.Name.Contains("coin") || chunk.Name.Contains("Coin"))
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
	}
}