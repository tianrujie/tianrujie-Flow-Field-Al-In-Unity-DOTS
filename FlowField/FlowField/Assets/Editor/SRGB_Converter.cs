using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KG.TA
{
    public class SRGB_Converter
    {
        public static void ConvertGammaToLinear(Texture2D tex)
        {
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    Color color = tex.GetPixel(x, y);

                    float channelR = Mathf.GammaToLinearSpace(color.r);
                    float channelG = Mathf.GammaToLinearSpace(color.g);
                    float channelB = Mathf.GammaToLinearSpace(color.b);
                    color = new Color(channelR, channelG, channelB, color.a);

                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
        }

        public static void ConvertLinearToGamma(Texture2D tex)
        {
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    Color color = tex.GetPixel(x, y);

                    float channelR = Mathf.LinearToGammaSpace(color.r);
                    float channelG = Mathf.LinearToGammaSpace(color.g);
                    float channelB = Mathf.LinearToGammaSpace(color.b);
                    color = new Color(channelR, channelG, channelB, color.a);

                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
        }

        public static void ConvertGammaToLinear(Cubemap cube)
        {
            for (int face = 0; face < 6; face++)
            {
                for (int x = 0; x < cube.width; x++)
                {
                    for (int y = 0; y < cube.height; y++)
                    {
                        Color color = cube.GetPixel((CubemapFace)face, x, y);

                        float channelR = Mathf.GammaToLinearSpace(color.r);
                        float channelG = Mathf.GammaToLinearSpace(color.g);
                        float channelB = Mathf.GammaToLinearSpace(color.b);
                        color = new Color(channelR, channelG, channelB, color.a);

                        cube.SetPixel((CubemapFace)face, x, y, color);
                    }
                }
            }
            cube.Apply();
        }
    }
}