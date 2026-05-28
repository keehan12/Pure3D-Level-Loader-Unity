using UnityEngine;

public class Sprite : MonoBehaviour
{
	private Vector3 rotation;
	
	void Update()
	{
		rotation.y = Camera.main.transform.rotation.eulerAngles.y + 180;

		transform.rotation = Quaternion.Euler(rotation);
	}
}
