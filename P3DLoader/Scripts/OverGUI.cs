using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class OverGUI : MonoBehaviour
{
	[HideInInspector] public bool overGui;
	private GraphicRaycaster graphicRaycaster;
	
	void Awake()
	{
		graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
	}
	
    void Update()
    {
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		List<RaycastResult> results = new List<RaycastResult>();
		graphicRaycaster.Raycast(pointerEventData, results);
		
		if (results.Count > 0)
		{
			if (overGui == false)
			{
				overGui = true;
			}
		}
		else
		{
			if (overGui == true)
			{
				overGui = false;
			}
		}
    }
}
