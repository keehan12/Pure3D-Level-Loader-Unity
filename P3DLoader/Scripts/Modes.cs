using UnityEngine;

public class Modes : MonoBehaviour
{
	[HideInInspector] public bool mode;
	private bool canvasMode;
	
	//Objects
	private GameObject freeCamera;
	private GameObject player;
	private GameObject spawn;
	private GameObject canvas;
	
	void Awake()
	{
		player = GameObject.Find("Player");
		spawn = GameObject.Find("Spawn");
		canvas = GameObject.Find("Canvas");
	}
	
	void Start()
	{
		Mode();
		CanvasMode();
	}
	
	void Update()
	{
		//Camera mode
		if (Input.GetKeyDown(KeyCode.V))
		{
			mode = !mode;
			Mode();
		}
		
		//Canvas
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			canvasMode = !canvasMode;
			
			CanvasMode();
		}
	}
	
	void Mode()
	{
		if (mode == false)
		{
			//Fly camera on
			player.GetComponent<PlayerController>().enabled = false;
			
			//Cursor
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			//Player on
			player.GetComponent<PlayerController>().enabled = true;
			
			//Hide spawner
			spawn.SetActive(false);
			
			//Cursor
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
	
	void CanvasMode()
	{
		//Canvas toggle
		if (canvasMode == false)
		{
			canvas.SetActive(false);
		}
		else
		{
			canvas.SetActive(true);
			
			//Set to free camera
			mode = false;
			Mode();
		}
	}
}
