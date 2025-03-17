using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCreator
{
    Constants constants = new Constants();


    public Texture2D CreateTerrainTexture(LevelData[,] terrainData, int scale)
    {

        Texture2D terrainTexture = new Texture2D(terrainData.GetLength(0) * scale, terrainData.GetLength(1) * scale, TextureFormat.ARGB32, false);
        terrainTexture.filterMode = FilterMode.Point;
        Color[] terrainColors =
        {
            constants.TERRAIN_COLOR_WATER,
            constants.TERRAIN_COLOR_SAND,
            constants.TERRAIN_COLOR_DEAD_GRASS,
            constants.TERRAIN_COLOR_GRASS,
            constants.TERRAIN_COLOR_ROCK
        };

        for (int i = 0; i < terrainData.GetLength(0); i++)
        {
            for (int j = 0; j < terrainData.GetLength(1); j++)
            {
                for (int a = 0; a < scale; a++)
                {
                    for (int b = 0; b < scale; b++)
                    {
                        Color baseColor = terrainColors[(int)terrainData[i, j].terrainType];
                        if (terrainData[i,j].terrainType == TerrainType.rock)
                        {
                            float colorChange = Random.Range(-0.025f, 0.025f);
                            baseColor += new Color(colorChange, colorChange, colorChange);
                        }
                        else
                        {
                            baseColor.r += (Random.Range(-0.025f, 0.025f));
                            baseColor.g += (Random.Range(-0.025f, 0.025f));
                            baseColor.b += (Random.Range(-0.025f, 0.025f));
                        }

                        terrainTexture.SetPixel((i * scale) + a,
                                                (j * scale) + b,
                                                baseColor);
                    }
                }

            }
        }

        terrainTexture.Apply();

        return terrainTexture;
    }

    public Texture2D CreatePlacementTexture(int[,] terrainData, int scale)
    {

        Texture2D terrainTexture = new Texture2D(terrainData.GetLength(0) * scale, terrainData.GetLength(1) * scale, TextureFormat.ARGB32, false);
        terrainTexture.filterMode = FilterMode.Point;

        for (int i = 0; i < terrainData.GetLength(0); i++)
        {
            for (int j = 0; j < terrainData.GetLength(1); j++)
            {
                Color baseColor = (terrainData[i,j] == 1) ? new Color(1f, 1f, 1f, 0.3f) : new Color(0f, 0f, 0f, 0f);

                terrainTexture.SetPixel((i * scale),
                                        (j * scale),
                                        baseColor);
            }
        }
        //terrainTexture.alphaIsTransparency = true;
        terrainTexture.Apply();

        return terrainTexture;
    }

    public Texture2D CreateNoiseTexture(float[,] noiseMap)
    {
        Texture2D texture = new Texture2D(noiseMap.GetLength(0), noiseMap.GetLength(1));

        Color[] colorMap = new Color[noiseMap.GetLength(0) * noiseMap.GetLength(1)];
        for (int i = 0; i < noiseMap.GetLength(0); i++)
        {
            for (int j = 0; j < noiseMap.GetLength(1); j++)
            {
                colorMap[j * noiseMap.GetLength(0) + i] = Color.Lerp(Color.black, Color.white, noiseMap[i, j]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

}
