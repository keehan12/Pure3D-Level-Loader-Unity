using UnityEngine;
using UnityEngine.UI;

public class SetRenderSettings : MonoBehaviour
{
    void Awake()
    {
        //Fog
		GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().value = RenderSettings.fogStartDistance;
		GameObject.Find("Canvas").transform.Find("FogEnd (Slider)").GetComponent<Slider>().value = RenderSettings.fogEndDistance;
		SetFog();
    }
	
	public void SetFog()
	{
		//Set slider max values
		GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().maxValue = GameObject.Find("Canvas").transform.Find("FogEnd (Slider)").GetComponent<Slider>().value - 1;
		
		if (GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().value >= GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().maxValue)
		{
			GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().value = GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().maxValue;
		}
		
		//Set render settings
		RenderSettings.fogStartDistance = GameObject.Find("Canvas").transform.Find("FogStart (Slider)").GetComponent<Slider>().value;
		RenderSettings.fogEndDistance = GameObject.Find("Canvas").transform.Find("FogEnd (Slider)").GetComponent<Slider>().value;
	}
}
