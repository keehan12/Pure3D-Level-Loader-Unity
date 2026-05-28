using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
	public int level = 1;
	public List<UnityEngine.Sprite> levelValueSprites;
	private Image levelValueSprite;
	
	void Awake()
	{
		levelValueSprite = GameObject.Find("Canvas").transform.Find("Level Value").GetComponent<Image>();
	}
	
	void Start()
	{
		UpdateValueSprite();
		LoadLevel(level);
	}
	
	void UpdateValueSprite()
	{
		levelValueSprite.sprite = levelValueSprites[level - 1];
	}
	
	public void Right()
	{
		if (level < 7)
		{
			level++;
			UpdateValueSprite();
			DestroyLevel();
			LoadLevel(level);
		}
	}
	
	public void Left()
	{
		if (level > 1)
		{
			level--;
			UpdateValueSprite();
			DestroyLevel();
			LoadLevel(level);
		}
	}
	
	void DestroyLevel()
	{
		if (GetComponent<P3DLoader>().objects.Count > 0)
		{
			for (int i = 0; i < GetComponent<P3DLoader>().objects.Count; i++)
			{
				Destroy(GetComponent<P3DLoader>().objects[i]);
			}
			
			GetComponent<P3DLoader>().objects = new List<GameObject>();
		}
	}
	
	void LoadLevel(int level)
	{
		if (GetComponent<P3DLoader>().objects.Count == 0)
		{
			if (GetComponent<P3DLoader>().levels.Count > level - 1)
			{
				if (GetComponent<P3DLoader>().levels[level - 1].files.Count > 0)
				{
					for (int i = 0; i < GetComponent<P3DLoader>().levels[level - 1].files.Count; i++)
					{
						GetComponent<P3DLoader>().LoadChunk(level, GetComponent<P3DLoader>().levels[level - 1].files[i]);
					}
				}
				else
				{
					Debug.Log("Level chunks not set");
				}
			}
			else
			{
				Debug.Log("Level not set");
			}
		}
	}
}
