using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
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
	public bool xml; //False: FBX, OBJ
	public Vector3 levelChunkRotation; //Recommended: (0, 180, 0)
	public Vector3 objectChunkRotation; //Recommended: (-90, 180, 0)
	
	[Space(8)]
	[Header("Car")]
	public List<CarWheels> wheels;
	public List<CustomParent> customParents;
	
	[Space(8)]
	[Header("Other")]
	public bool billboards;
	
	//Total objects
	[HideInInspector] public List<GameObject> objects;
	[HideInInspector] public List<ShadersList> shaders = new List<ShadersList>();
	
	public void LoadChunk(int level, string file)
    {
		LoadShaders(level, file);
		
		string path = gameArtPath + "/" + file + ".p3d";
		
		if (File.Exists(path))
		{
			//New p3d file from path
			p3dFile = new P3DFile(path);
			
			//Read root chunks
			if (p3dFile != null)
			{
				//Inst Stat Phys
				var instStatPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.InstStatPhysChunk>();
				
				foreach (var chunk in instStatPhysChunks)
				{
					InstStatPhys(level, chunk, file);
				}
				
				//Dyna Phys
				var dynaPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.DynaPhysChunk>();
				
				foreach (var chunk in dynaPhysChunks)
				{
					DynaPhys(level, chunk, file);
				}
				
				//Anim Dyna Phys
				var animDynaPhysChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.AnimDynaPhysChunk>();
				
				foreach (var chunk in animDynaPhysChunks)
				{
					AnimDynaPhys(level, chunk, file);
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
				
				//Old Billboard Quad Group
				if (billboards == true)
				{
					var billboardQuadGroup = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.OldBillboardQuadGroupChunk>();
					
					foreach (var chunk in billboardQuadGroup)
					{
						BillboardQuadGroup(level, chunk);
					}
				}
				
				Debug.Log("Found p3d: " + file);
			}
			
			//Instantiate static mesh
			if (Resources.Load("Level " + level + "/" + file))
			{
				if (xml == false)
				{
					GameObject obj = Instantiate(Resources.Load("Level " + level + "/" + file) as GameObject, new Vector3(0, 0, 0), Quaternion.Euler(levelChunkRotation));
					
					//Add to objects list
					objects.Add(obj);
				}
				else
				{
					GameObject obj = GenerateMesh("Level " + level + "/", file, file, "");
					
					//Add to objects list
					objects.Add(obj);
				}
			}
		}
		else
		{
			Debug.Log("No p3d: " + file);
		}
    }
	
	[System.Serializable]
	public class ShadersList
	{
		public string file;
		public List<ShaderData> shader;
	}
	
	void LoadShaders(int level, string file)
	{
		string path = gameArtPath + "/" + file + ".p3d";
		
		if (File.Exists(path))
		{
			//New p3d file from path
			p3dFile = new P3DFile(path);
			
			//Read root chunks
			if (p3dFile != null)
			{
				//Shader - levels
				var shaderChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.ShaderChunk>();
				
				List<ShaderData> shaderDatas = new List<ShaderData>();
				
				foreach (var chunk in shaderChunks)
				{
					shaderDatas.Add(GetShader(shaderDatas, chunk));
				}
				
				shaders.Add(new ShadersList { file = file, shader = shaderDatas });
			}
		}
	}
	
	public void LoadMissionsShaders(int level)
	{
		//Level folder
		string path = gameArtPath + "/missions/level0" + level + "/level" + ".p3d";
		
		if (File.Exists(path))
		{
			//New p3d file from path
			p3dFile = new P3DFile(path);
			
			//Read root chunks
			if (p3dFile != null)
			{
				//Shader - missions
				var shaderChunks = p3dFile.GetChunksOfType<NetP3DLib.P3D.Chunks.ShaderChunk>();
				
				List<ShaderData> shaderDatas = new List<ShaderData>();
				
				foreach (var chunk in shaderChunks)
				{
					shaderDatas.Add(GetShader(shaderDatas, chunk));
				}
				
				shaders.Add(new ShadersList { file = "level0" + level, shader = shaderDatas });
			}
		}
	}
	
	[System.Serializable]
	public class ShaderData
	{
		public string material;
		public string texture;
		public bool translucent;
		public int alphaTest;
		public int lighting;
	}
	
	ShaderData GetShader(List<ShaderData> shaderDatas, NetP3DLib.P3D.Chunks.ShaderChunk chunk)
	{
		bool translucent = chunk.HasTranslucency;
		
		//ShaderTextureParameter
		var shaderTextureParameterChunks = chunk.GetChunksOfType<NetP3DLib.P3D.Chunks.ShaderTextureParameterChunk>();
		
		foreach (var shaderTextureParameter in shaderTextureParameterChunks)
		{
			ShaderData shader = (new ShaderData { material = chunk.Name, texture = shaderTextureParameter.Value, translucent = translucent, alphaTest = Convert.ToInt32(chunk.GetIntegerParameter("ATST")), lighting = Convert.ToInt32(chunk.GetIntegerParameter("LIT")) });
			
			bool contains = false;
			
			for (int i = 0; i < shaders.Count; i++)
			{
				if (shaderDatas.Contains(shader))
				{
					contains = true;
				}
			}
			
			if (contains == false)
			{
				if (shader.texture != "EnvMap.bmp")
				{
					return shader;
				}
			}
		}
		
		return null;
	}
	
	Material ShaderMaterial(MeshData data)
	{
		Material material = new Material(Shader.Find("Custom/Opaque"));
				
		if (data.material.translucent == true)
		{
			material.shader = Shader.Find("Custom/Transparent");
			
			if (data.material.lighting == 1 && data.material.alphaTest == 0)
			{
				material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
			}
		}
		
		if (data.material.alphaTest == 1)
		{
			material.shader = Shader.Find("Custom/AlphaTest");
		}
		
		material.name = data.material.material;
		
		return material;
	}
	
	void AnimDynaPhys(int level, Chunk chunk, string p3dName)
	{
		//Anim Obj Wrapper
		var animObjWrapperChunks = chunk.GetChunksOfType<NetP3DLib.P3D.Chunks.AnimObjWrapperChunk>();
			
		foreach (var animObjWrapper in animObjWrapperChunks)
		{
			//Composite Drawable
			var compositeDrawableChunks = animObjWrapper.GetChunksOfType<NetP3DLib.P3D.Chunks.CompositeDrawableChunk>();
			
			foreach (var compositeDrawable in compositeDrawableChunks)
			{
				//Get Prop List
				CompositeDrawablePropList(level, chunk, compositeDrawable, p3dName, animObjWrapper.Name);
			}
		}
	}
	
	void InstStatPhys(int level, NetP3DLib.P3D.Chunks.InstStatPhysChunk chunk, string p3dName)
	{
		//Get mesh from Inst Stat Phys
		GetMesh(level, chunk, chunk.Name, p3dName, chunk.Name);
	}
	
	void DynaPhys(int level, NetP3DLib.P3D.Chunks.DynaPhysChunk chunk, string p3dName)
	{
		//Get mesh from Dyna Phys
		GetMesh(level, chunk, chunk.Name, p3dName, chunk.Name);
	}
	
	void CompositeDrawablePropList(int level, Chunk chunk, NetP3DLib.P3D.Chunks.CompositeDrawableChunk compositeDrawable, string p3dName, string name)
	{
		//Composite Drawable Prop List
		var compositeDrawablePropListChunks = compositeDrawable.GetChunksOfType<NetP3DLib.P3D.Chunks.CompositeDrawablePropListChunk>();
		
		foreach (var compositeDrawablePropList in compositeDrawablePropListChunks)
		{
			//Composite Drawable Prop
			var compositeDrawablePropChunks = compositeDrawablePropList.GetChunksOfType<NetP3DLib.P3D.Chunks.CompositeDrawablePropChunk>();
			
			foreach (var compositeDrawableProp in compositeDrawablePropChunks)
			{
				//Get mesh from Composite Drawable
				GetMesh(level, chunk, name, p3dName, compositeDrawableProp.Name);
			}
		}
	}
	
	void GetMesh(int level, Chunk chunk, string name, string p3dName, string file)
	{
		if (Resources.Load("Level " + level + "/" + name + "/" + file))
		{
			string path = "Level " + level + "/" + name + "/";
			string fallback = "Level " + level + "/";
			
			InstanceList(chunk, path, file, p3dName, fallback);
		}
		else if (Resources.Load("Level " + level + "/" + file))
		{
			string path = "Level " + level + "/";
			
			InstanceList(chunk, path, file, p3dName, path);
		}
	}
	
	void InstanceList(Chunk chunk, string path, string file, string p3dName, string fallback)
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
								Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
								
								//Instantiate resources if available by Name
								if (xml == false)
								{
									GameObject obj = Instantiate(Resources.Load(path + file) as GameObject, new Vector3(0, 0, 0), Quaternion.Euler(levelChunkRotation));
									obj.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);
									
									//Add to objects list
									objects.Add(obj);
								}
								else
								{
									GameObject obj = GenerateMesh(path, file, p3dName, fallback);
									obj.transform.position = position;
									obj.transform.rotation = rotation;
									
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
				if (xml == false)
				{
					GameObject obj = Instantiate(Resources.Load("Level/coinShape_000") as GameObject, position, Quaternion.Euler(objectChunkRotation));
					obj.transform.rotation = Quaternion.Euler(objectChunkRotation);
					
					//Add to objects list
					objects.Add(obj);
				}
				else
				{
					GameObject obj = GenerateMesh("Level/", "coinShape_000", "level0" + level, "");
					obj.transform.position = position;
					
					//Add to objects list
					objects.Add(obj);
				}
			}
		}
		
		//Locator is Car Start
		if (Convert.ToString(chunk.LocatorType) == "CarStart")
		{
			string path = "";
			
			//Get Position data
			Vector3 position = new Vector3(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
			
			//Search for car path
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
			
			//Car Skeleton
			if (path != "")
			{
				//New p3d file from path
				P3DFile carP3D = new P3DFile(path);
				
				CarSkeleton(chunk.Name, carP3D, position);
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
			Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
			
			if (car == null)
			{
				//Instantiate resources if available by Name
				if (Resources.Load("Level " + level + "/" + skeletonJoint.Name + "Shape"))
				{
					if (xml == false)
					{
						GameObject obj = Instantiate(Resources.Load("Level " + level + "/" + skeletonJoint.Name + "Shape") as GameObject, position, rotation);
						obj.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);
						
						//Add to objects list
						objects.Add(obj);
					}
					else
					{
						GameObject obj = GenerateMesh("Level " + level + "/", skeletonJoint.Name + "Shape", chunk.Name, "");
						obj.transform.position = position;
						obj.transform.rotation = rotation;
						
						//Add to objects list
						objects.Add(obj);
					}
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
				
				if (xml == false)
				{
					parent.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);;
				}
				else
				{
					parent.transform.localRotation = rotation;
				}
				
				//Set custom parent
				List<string> carsWithCustomParents = new List<string>();
				
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
						
						//Custom parents
						if (customParents[i].parent == skeletonJoint.Name)
						{
							if (customParents[i].child != skeletonJoint.Name)
							{
								joint.model = customParents[i].model;
								JointCar(car, joint, parent);
							}
						}
					}
				}
				
				//Wheels
				string current = "";
				
				for (int i = 0; i < wheels.Count; i++)
				{
					for (int a = 0; a < wheels[i].wheel.Count; a++)
					{
						if (wheels[i].car == car.name)
						{
							if (wheels[i].wheel[a].joint == skeletonJoint.Name)
							{
								current = wheels[i].wheel[a].joint;
								joint.model = wheels[i].wheel[a].model;
								JointCar(car, joint, parent);
							}
						}
					}
				}
				
				//Base models for car without custom parents
				if (!carsWithCustomParents.Contains(car.name))
				{
					if (current != skeletonJoint.Name)
					{
						JointCar(car, joint, parent);
					}
				}
				
				//Add joint to parent GameObject list
				joints.Add(parent);
				
				//Add to objects list
				objects.Add(parent);
			}
			
			//Debug if data exists
			Debug.Log(skeletonJoint.Name + ", position: " + position + ", rotation: " + rotation);
		}
		
		return joints;
	}
	
	void BillboardQuadGroup(int level, NetP3DLib.P3D.Chunks.OldBillboardQuadGroupChunk chunk)
	{
		//Instance List
		var billboardQuadChunks = chunk.GetChunksOfType<NetP3DLib.P3D.Chunks.OldBillboardQuadChunk>();
		
		foreach (var billboardQuad in billboardQuadChunks)
		{
			string shader = chunk.Shader;
			string texture = "";
		
			GameObject billboard = Instantiate(Resources.Load("Source/Billboard") as GameObject);
			
			//Billboard translation
			Vector3 position = new Vector3(billboardQuad.Translation.X, billboardQuad.Translation.Y, billboardQuad.Translation.Z);
			
			//Billboard scale
			Vector2 scale = new Vector2(billboardQuad.Width, billboardQuad.Height);
			
			//Transform
			billboard.transform.position = position;
			billboard.transform.localScale = scale;
			
			//Texture from data
			for (int a = 0; a < shaders.Count; a++)
			{
				for (int b = 0; b < shaders[a].shader.Count; b++)
				{
					if (shader == shaders[a].shader[b].material)
					{
						texture = shaders[a].shader[b].texture;
						
						if (Resources.Load("Level " + level + "/" + texture))
						{
							Material material = new Material(Shader.Find("Custom/Opaque"));
							
							if (shaders[a].shader[b].translucent == true)
							{
								material.shader = Shader.Find("Custom/Transparent");
								
								if (shaders[a].shader[b].lighting == 1 && shaders[a].shader[b].alphaTest == 0)
								{
									material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
								}
							}
							
							if (shaders[a].shader[b].alphaTest == 1)
							{
								material.shader = Shader.Find("Custom/AlphaTest");
							}
							
							material.name = shader;
							billboard.transform.GetChild(0).GetComponent<MeshRenderer>().material = material;
							billboard.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = Resources.Load("Level " + level + "/" + texture) as Texture2D;
						}
					}
				}
			}
			
			//Add component
			billboard.AddComponent<Sprite>();
			
			//Add to objects list
			objects.Add(billboard);
		}
	}
	
	public void CarSkeleton(string name, P3DFile carP3D, Vector3 position)
	{
		//Car shaders
		var shaderChunks = carP3D.GetChunksOfType<NetP3DLib.P3D.Chunks.ShaderChunk>();
		
		List<ShaderData> shaderDatas = new List<ShaderData>();
		
		foreach (var chunk in shaderChunks)
		{
			shaderDatas.Add(GetShader(shaderDatas, chunk));
		}
		
		shaders.Add(new ShadersList { file = name, shader = shaderDatas });
		
		var skeletonChunks = carP3D.GetChunksOfType<NetP3DLib.P3D.Chunks.SkeletonChunk>();
		
		foreach (var skeleton in skeletonChunks)
		{
			//New car
			Car car = new Car();
			car.name = skeleton.Name;
			car.position = position + new Vector3(0, 1, 0);
			
			//Car skeleton
			List<GameObject> carSkeleton = Skeleton(0, skeleton, car);
			
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
			Debug.Log(car.name + ", position: " + position);
		}
	}
	
	void JointCar(Car car, CarJoint joint, GameObject parent)
	{
		GameObject carJoint = ModelCar(car, joint, car.name);
							
		if (carJoint != null)
		{
			carJoint.transform.SetParent(parent.transform, true);
		}
	}
	
	GameObject ModelCar(Car car, CarJoint joint, string p3dName)
	{
		if (Resources.Load("Cars/" + car.name + "/" + joint.model))
		{
			Quaternion rotation = joint.rotation;
			
			if (xml == false)
			{
				GameObject obj = Instantiate(Resources.Load("Cars/" + car.name + "/" + joint.model) as GameObject, car.position + joint.position, rotation);
				obj.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x + objectChunkRotation.x, rotation.eulerAngles.y + objectChunkRotation.y, rotation.eulerAngles.z + objectChunkRotation.z);
				
				return obj;
			}
			else
			{
				GameObject obj = GenerateMesh("Cars/" + car.name + "/", joint.model, p3dName, "");
				obj.transform.position = car.position + joint.position;
				obj.transform.rotation = rotation;
				
				return obj;
			}
		}
		
		return null;
	}
	
	//Generated mesh data
	public class MeshData
	{
		public MeshMaterial material = new MeshMaterial();
		public List<Vector3> vertices = new List<Vector3>();
		public List<Vector2> uvs = new List<Vector2>();
		public List<Color32> colors = new List<Color32>();
		public List<int> triangles = new List<int>();
	}
	
	//Generated mesh material
	public class MeshMaterial
	{
		public string mesh;
		public bool color;
		public string material;
		public string texture;
		public bool translucent;
		public int alphaTest;
		public int lighting;
	}
	
	//Mesh generation
	public GameObject GenerateMesh(string path, string file, string name, string fallback)
	{
		GameObject parent = new GameObject();
		parent.name = file;
		
		TextAsset textAsset = Resources.Load(path + file) as TextAsset;
		XmlTextReader reader = new XmlTextReader(new StringReader(textAsset.text));
		
		List<MeshData> data = new List<MeshData>();
		int index = 0;
		
		while (reader.Read())
		{
			if (reader.Name == "Mesh" && reader.NodeType == XmlNodeType.Element && reader.Depth == 1)
			{
				string mesh = reader.GetAttribute("Name");
				
				while (reader.Read())
				{
					if (reader.Name == "Shader" && reader.NodeType == XmlNodeType.Element && reader.Depth == 2)
					{
						data.Add(new MeshData());
						data[index].material.material = reader.GetAttribute("Name");
						data[index].material.mesh = mesh;
						
						while (reader.Read())
						{
							if (reader.Name == "Vertex" && reader.NodeType == XmlNodeType.Element)
							{
								data[index].vertices.Add(new Vector3(
								float.Parse(reader.GetAttribute("PositionX")),
								float.Parse(reader.GetAttribute("PositionY")),
								float.Parse(reader.GetAttribute("PositionZ"))
								));
								
								data[index].uvs.Add(new Vector2(
								float.Parse(reader.GetAttribute("U")),
								float.Parse(reader.GetAttribute("V"))
								));
								
								data[index].colors.Add(new Color32(
								Convert.ToByte(reader.GetAttribute("Red")),
								Convert.ToByte(reader.GetAttribute("Blue")),
								Convert.ToByte(reader.GetAttribute("Green")),
								Convert.ToByte(reader.GetAttribute("Alpha"))
								));
							}
							
							if (reader.Name == "Primitive" && reader.NodeType == XmlNodeType.Element)
							{
								data[index].triangles.Add(Convert.ToInt32(reader.GetAttribute("Vertex1")));
								data[index].triangles.Add(Convert.ToInt32(reader.GetAttribute("Vertex2")));
								data[index].triangles.Add(Convert.ToInt32(reader.GetAttribute("Vertex3")));
							}
							
							if (reader.Name == "Shader" && reader.NodeType == XmlNodeType.EndElement)
							{
								break;
							}
						}
						
						index++;
					}
				}
				
				if (reader.Name == "Mesh" && reader.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
			}
		}
		
		for (int i = 0; i < data.Count; i++)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = data[i].vertices.ToArray();
			mesh.uv = data[i].uvs.ToArray();
			mesh.triangles = data[i].triangles.ToArray();
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			
			GameObject obj = new GameObject();
			obj.AddComponent<MeshFilter>();
			obj.GetComponent<MeshFilter>().mesh = mesh;
			obj.AddComponent<MeshRenderer>();
			obj.AddComponent<MeshCollider>();
			
			//Shader
			for (int a = 0; a < shaders.Count; a++)
			{
				for (int b = 0; b < shaders[a].shader.Count; b++)
				{
					MeshShader(shaders[a].shader[b], data[i].material);
					
					//Override with shader from file if it exists
					if (shaders[a].file == name)
					{
						MeshShader(shaders[a].shader[b], data[i].material);
					}
				}
			}
			
			if (data[i].material.lighting == 0)
			{
				mesh.colors32 = data[i].colors.ToArray();
			}
			
			if (Resources.Load(path + data[i].material.texture))
			{
				Material material = ShaderMaterial(data[i]);
				obj.GetComponent<MeshRenderer>().material = material;
				obj.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load(path + data[i].material.texture) as Texture2D;
			}
			else if (Resources.Load(fallback + data[i].material.texture))
			{
				Material material = ShaderMaterial(data[i]);
				obj.GetComponent<MeshRenderer>().material = material;
				obj.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load(fallback + data[i].material.texture) as Texture2D;
			}
			
			obj.name = data[i].material.mesh;
			obj.transform.SetParent(parent.transform);
		}
		
		return parent;
	}
	
	void MeshShader(ShaderData shader, MeshMaterial material)
	{
		if (material.material == shader.material)
		{
			material.texture = shader.texture;
			material.translucent = shader.translucent;
			material.alphaTest = shader.alphaTest;
			material.lighting = shader.lighting;
		}
	}
}