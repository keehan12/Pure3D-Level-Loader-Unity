using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Vector3 moveDirection;
	private Vector3 airMoveDirection;
	private CharacterController controller;
	
	public float speed = 4;
	private float temp;
	[HideInInspector] public bool running;
	public float gravity = 12;
	public float jump = 4;

	void Awake()
	{
		temp = speed;
		Respawn();
	}
	
	void Update()
	{
		if (controller == null)
		{
			controller = GetComponent<CharacterController>();
		}
		
		//Respawn
		if (Input.GetKeyDown(KeyCode.R))
		{
			Respawn();
		}
		
		if (running == true)
		{
			if (speed != temp * 2)
			{
				speed = temp * 2;
			}
		}
		else
		{
			if (speed != temp)
			{
				speed = temp;
			}
		}
		
		if (controller != null)
		{
			if (controller.isGrounded)
			{
				//Running
				if (Input.GetKey(KeyCode.LeftShift))
				{
					running = true;
				}
				else
				{
					running = false;
				}
		
				//Reset air movement
				airMoveDirection = Vector3.zero;
				
				//Set direction
				moveDirection.x = Input.GetAxis("Horizontal");
				moveDirection.z = Input.GetAxis("Vertical");
				
				//Jump
				if (Input.GetKey(KeyCode.Space))
				{
					Jump();
				}
			}
			else
			{
				//Set direction
				moveDirection.x = airMoveDirection.x;
				moveDirection.z = airMoveDirection.z;
			}
		}
		
		//Transform direction
		moveDirection = transform.TransformDirection(moveDirection);
		
		//Diagonal movement speed
		if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
		{
			moveDirection.x *= 0.707f;
			moveDirection.z *= 0.707f;
		}
		
		//Multiplier
		moveDirection.x *= speed;
		moveDirection.z *= speed;
		
		//Follow head angle
		transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
	}
	
	void FixedUpdate()
	{
		if (controller != null)
		{
			if (!controller.isGrounded)
			{
				//Gravity
				moveDirection.y -= gravity * Time.deltaTime;
			}
		}
		
		if (controller != null)
		{
			//Move controller
			controller.Move(moveDirection * Time.deltaTime);
		}
	}
	
	void Jump()
	{
		airMoveDirection.x = moveDirection.x / 1.25f;
		moveDirection.y = jump;
		airMoveDirection.z = moveDirection.z / 1.25f;
		running = false;
	}
	
	void Respawn()
	{
		transform.position = GameObject.Find("Spawn").transform.position;
	}
}
