using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class ImageProcess : MonoBehaviour {

	private GameObject m_CacheGameObj;
	public GameObject CacheGameObj {
		get{
			if(!m_CacheGameObj){
				m_CacheGameObj = this.gameObject;
			}

			return m_CacheGameObj;
		}
	}

	private UnityEngine.UI.Image m_Image;
	private Sprite m_OriSprite;
	private Texture2D m_Texture;

	public bool DoRecovery;
	public bool DoGray;
	public bool DoFloydSteinberg;
    public bool DoOrderedDithering;
    [Range(0, 25)]
    public float OrderedDitheringLevel = 8;
	// Use this for initialization
	void Start () {
		m_Image = CacheGameObj.GetComponent<UnityEngine.UI.Image>();
		m_OriSprite = m_Image.sprite;

		m_Texture = new Texture2D(m_Image.sprite.texture.width, m_Image.sprite.texture.height);

		CopyTexture(m_OriSprite.texture, m_Texture);
		m_Image.sprite =  Sprite.Create(m_Texture, new Rect(0, 0, m_Texture.width, m_Texture.height), new Vector2(0.5f, 0.5f));
	}
	
	// Update is called once per frame
	void Update () {

		if (DoRecovery) 
        {
			CopyTexture(m_OriSprite.texture, m_Texture);
//            DoRecovery = false;
		}


		if(DoGray)
		{
			ConvertGray(m_Texture);
			DoGray = false;
		}

		if(DoFloydSteinberg)
		{
			FloydSteinberg(m_Texture);
			DoFloydSteinberg = false;
		}

        if (DoOrderedDithering)
        {
            OrderedDithering(m_Texture, OrderedDitheringLevel);
//            DoOrderedDithering = false;
        }
	}


	private void CopyTexture(Texture2D orig, Texture2D des)
	{
		des.SetPixels(orig.GetPixels());
		des.Apply();
	}

	private void ConvertGray(Texture2D texture)
	{
		int width = texture.width;
		int height = texture.height;

		Color[] pix = texture.GetPixels(0, 0, width, height);

		int current = 0;
		Color color = Color.white;
		for(int h = 0; h < height; h++)
		{
			for(int w = 0; w < width; w++)
			{
				current = h * width + w;

				color.r = pix[current].r * 0.299f + pix[current].g * 0.587f + pix[current].b * 0.114f;
				color.g = color.r;
				color.b = color.r;

				pix[current] = color;
			}
		}

		texture.SetPixels(pix);
		texture.Apply();
	}

	private void FloydSteinberg(Texture2D texture)
	{
		int width = texture.width;
		int height = texture.height;
		Color[] pix = texture.GetPixels(0, 0, width, height);

		int current = 0;
		Color oldpixel = Color.white;
		Color newpixel = Color.white;
		Color quant_error = Color.white;

		float v7_16 = 7f/16f;
		float v3_16 = 3f/16f;
		float v5_16 = 5f/16f;
		float v1_16 = 1f/16f;

		for(int h = 0; h < height; h++)
		{
			for(int w = 0; w < width; w++)
			{
				current = h * width + w;

				oldpixel = pix[current];

				newpixel.r = (oldpixel.r > 0.5f) ? 1.0f : 0f; 
				newpixel.g = (oldpixel.g > 0.5f) ? 1.0f : 0f;
				newpixel.b = (oldpixel.b > 0.5f) ? 1.0f : 0f;
				newpixel.a = oldpixel.a;

				pix[current] = newpixel;

				quant_error = oldpixel - newpixel;
				if(w + 1 < width)
				{
					current = h * width + (w + 1);
					pix[current] = pix[current] + quant_error * v7_16;				

					if(h + 1 < height )
					{
						current = (h + 1) * width + (w + 1);
						pix[current] = pix[current] + quant_error * v1_16;				

					}

				}

				if(w - 1 > 0 && h + 1 < height)
				{
					current = (h + 1) * width + (w - 1);
					pix[current] = pix[current] + quant_error * v3_16;				

				}

				if(h + 1 < height)
				{
					current = (h + 1) * width + w ;
					pix[current] = pix[current] + quant_error * v5_16;	
				}
					
			}
		}
		texture.SetPixels(pix);
		texture.Apply();


	}


    private float[,] m_Adjustments = 
            {{1, 9, 3, 11},
            {13, 5, 15, 7},
            {4, 12, 2, 10},
            {16, 8, 14, 6}}; 
    
    private void OrderedDithering(Texture2D texture, float level)
    {
        float mult = 1f / 17f;
        var adjustMatrix = (float[,])m_Adjustments.Clone();

        int singleLengrh = adjustMatrix.GetLength(0);
        // modify adjust matrix
        for(int i = 0; i < singleLengrh; i++)
        {
            for (int j = 0; j < singleLengrh; j++)
            {
                adjustMatrix[i, j] = (adjustMatrix[i, j] - level) * mult;
            }
        }

        int width = texture.width;
        int height = texture.height;
        Color[] pix = texture.GetPixels(0, 0, width, height);

        int current = 0;
        Color oldpixel = Color.white;
        Color newpixel = Color.white;

        for (int h = 0; h < height; h++)
        {
            for(int w = 0; w < width; w++)
            {
                current = h * width + w;
                oldpixel = pix[current];

                float coordX = w % singleLengrh;
                float coordY = h % singleLengrh;

                newpixel = oldpixel + (oldpixel * adjustMatrix[(int)coordY, (int)coordX]);

//                newpixel.r = (newpixel.r > 0.5f) ? 1.0f : 0f; 
//                newpixel.g = (newpixel.g > 0.5f) ? 1.0f : 0f;
//                newpixel.b = (newpixel.b > 0.5f) ? 1.0f : 0f;
                newpixel.r = oldpixel.r;
                newpixel.g = oldpixel.g;
                newpixel.b = oldpixel.b;
//                newpixel.a = oldpixel.a;
                newpixel.a =  (newpixel.a > 0.5f) ? 1.0f : 0f;


                pix[current] = newpixel;
            }
        }


        texture.SetPixels(pix);
        texture.Apply();

    }


}
