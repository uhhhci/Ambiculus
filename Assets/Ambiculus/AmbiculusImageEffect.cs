using UnityEngine;
[ExecuteInEditMode]
public class AmbiculusImageEffect : MonoBehaviour 
{
	public RenderTexture left;//, right;
	
	void Init(int width, int height)
	{
		left = new RenderTexture(width,height,24);
		left.Create();
	}
	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) 
	{
		if(left == null)
		{
			Init(source.width,source.height);
		}
		Graphics.Blit (source, left); //just copy the texture..		
		Graphics.Blit (source, destination);
	}
}
