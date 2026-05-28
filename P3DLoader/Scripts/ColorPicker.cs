using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	public Color color;
	private Slider hueSlider;
	private float H, S, V;
	
	void Awake()
	{
		//Get null objects
		hueSlider = GameObject.Find("Canvas").transform.Find("Hue (Slider)").GetComponent<Slider>();
		
		//Set color to color
		SetColor();
	}
	
	public void ChangeColor()
	{
		//Color as HSV
		color = Color.HSVToRGB(hueSlider.value, S, V);
		Color.RGBToHSV(color, out H, out S, out V);
		
		GetColor();
	}
	
	public void SetColor()
	{
		//Color as HSV
		Color.RGBToHSV(color, out H, out S, out V);
		hueSlider.value = H;
		
		GetColor();
	}
	
	public void GetColor()
	{
		//Change handle colors
		hueSlider.transform.Find("Handle Slide Area").Find("Handle").GetComponent<Image>().color = Color.HSVToRGB(H, S, V);
		
		//Set fog and sky color
		RenderSettings.skybox.SetColor("_Tint", Color.HSVToRGB(H, 0.75f, 1));
		RenderSettings.fogColor = Color.HSVToRGB(H, 0.75f, 1);
		RenderSettings.ambientLight = Color.HSVToRGB(H, 0.125f, 1);
	}
}
