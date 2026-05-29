using UnityEngine;

public class HeadController : MonoBehaviour
{
	private Transform player;
	private float lerp;
	private float speed;
	private float temp;
	private string direction = "Down";
	private float amount = 0.024f;
	private float multiplier = 1.1f;
	
	//Mouse look
	private float x;
	private float y;
	private float sensitivity = 3;
	
	//Free camera
	private Vector3 moveDirection;
	private Vector3 position;
	private float upDown;
	private GameObject spawn;
	
	void Awake()
	{
		x = transform.rotation.eulerAngles.y;
		y = transform.rotation.eulerAngles.x;
	}
	
	void Update()
	{
		if (player == null)
		{
			player = GameObject.Find("Player").transform;
			speed = player.GetComponent<PlayerController>().speed * multiplier;
			temp = speed;
		}
		
		if (spawn == null)
		{
			spawn = GameObject.Find("Spawn");
		}
		
		if (GameObject.Find("Scripts").GetComponent<Modes>().mode == true)
		{
			//Head movement
			if (player != null)
			{
				if (lerp >= 1)
				{
					direction = "Down";
				}
				
				if (lerp <= 0)
				{
					direction = "Up";
				}
				
				//Speed multiplier
				if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
				{
					if (player.GetComponent<PlayerController>().running == false)
					{
						//Walking
						speed = temp;
					}
					else
					{
						//Running
						speed = temp * 2;
					}
				}
				else
				{
					//Idle
					speed = temp / 10;
				}
			}
			
			//Set position to player position + head movement
			transform.position = new Vector3(player.position.x, player.position.y + Mathf.Lerp(amount, -amount, lerp), player.position.z);
			
			MouseLook();
		}
		else //Free cam
		{
			//Spawn
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
				{
					if (GameObject.Find("Scripts").GetComponent<OverGUI>().overGui == false)
					{
						Vector3 position = new Vector3(hit.point.x, hit.point.y + 2, hit.point.z);
						
						spawn.SetActive(true);
						spawn.transform.position = position;
						player.position = position;
					}
				}
			}
			
			if (Input.GetMouseButton(1) || Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.Q)|| Input.GetKey (KeyCode.E))
			{
				MouseLook();
			}
			
			//Up/down
			if (Input.GetKey(KeyCode.E))
			{
				upDown = 1;
			}
			else if (Input.GetKey(KeyCode.Q))
			{
				upDown = -1;
			}
			else if (!Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
			{
				upDown = 0;
			}
			
			//Move direction
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), upDown, Input.GetAxis("Vertical")).normalized;
			
			//Transform direction
			moveDirection = transform.TransformDirection(moveDirection);
			
			//Speed multiplier
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				//Walking
				speed = temp * 6;
			}
			else
			{
				//Running
				speed = temp * 12;
			}
		}
	}
	
	void FixedUpdate()
	{
		if (GameObject.Find("Scripts").GetComponent<Modes>().mode == true)
		{
			//Lerp by direction
			if (direction == "Up")
			{
				lerp += speed * Time.deltaTime;
			}
			
			if (direction == "Down")
			{
				lerp -= speed * Time.deltaTime;
			}
		}
		else
		{
			//Transform position
			position = moveDirection * speed * Time.deltaTime;
			transform.position += position;
		}
	}
	
	void MouseLook()
	{
		//Mouse look
		x += Input.GetAxis("Mouse X") * sensitivity;
		
		if (x < 0)
		{
			x += 360;
		}
		else if (x > 360)
		{
			x -= 360;
		}
		
		y -= Input.GetAxis("Mouse Y") * sensitivity;
		
		if (y < -90)
		{
			y = -90;
		}
		else if (y > 90)
		{
			y = 90;
		}
		
		//Transform
		transform.rotation = Quaternion.Euler(y, x, transform.rotation.eulerAngles.z);
	}
}
