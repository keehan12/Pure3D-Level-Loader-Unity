using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	private float sensitivity = 2.25f;
	private float climbSpeed = 4;
	private float normalMoveSpeed = 16;
	private float fastMultiplier = 5;
	
	private float x = 0;
	private float y = 0;
	
	private GameObject player;
	private GameObject spawn;
	
	void Awake()
	{
		ResetVector();
	}
	
    void Update ()
	{
		if (player == null)
		{
			player = GameObject.Find("Player");
		}
		
		if (spawn == null)
		{
			spawn = GameObject.Find("Spawn");
		}
		
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
					player.transform.position = position;
				}
			}
		}
		
		if (Input.GetMouseButton(1) || Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.Q)|| Input.GetKey (KeyCode.E))
		{
			//original mouse look
			x += Input.GetAxis("Mouse X") * sensitivity;
			y -= Input.GetAxis("Mouse Y") * sensitivity;
			
			if (x < 0)
			{
				x += 360;
			}
			else if (x > 360)
			{
				x -= 360;
			}
			
			if (y < -90)
			{
				y = -90;
			}
			else if (y > 90)
			{
				y = 90;
			}
			
			transform.rotation = Quaternion.Euler(y, x, transform.rotation.eulerAngles.z);
			
			//Up/down
			if (Input.GetKey(KeyCode.E))
			{
				transform.position += transform.up * (climbSpeed) * fastMultiplier * Time.deltaTime;
			}
			
			if (Input.GetKey(KeyCode.Q))
			{
				transform.position -= transform.up * (climbSpeed) * fastMultiplier * Time.deltaTime;
			}
			
			if (Input.GetKey(KeyCode.LeftShift))
			{
				transform.position += transform.forward * (normalMoveSpeed * fastMultiplier) * Input.GetAxis("Vertical") * Time.deltaTime;
				transform.position += transform.right * (normalMoveSpeed * fastMultiplier) * Input.GetAxis("Horizontal") * Time.deltaTime;
				
			}
			else
			{
				transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
				transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
		}
	}
	
	public void ResetVector()
	{
		x = transform.rotation.eulerAngles.y;
		y = transform.rotation.eulerAngles.x;
		transform.rotation = Quaternion.Euler(y, x, transform.rotation.eulerAngles.z);
	}
}