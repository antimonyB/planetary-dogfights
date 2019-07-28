using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidgidNoiseFilter : INoiseFilter {

    NoiseSettings.RidgidNoiseSettings settings;
    Noise noise = new Noise();

    public RidgidNoiseFilter(NoiseSettings.RidgidNoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;
        float noiseIn = 0;
        float spike = 0;
        float layer;

        /*for (int i = 0; i < settings.numLayers; i++)
        {
            float v = 1-Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * settings.weightMultiplier);

            noiseValue += v * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }*/
        noiseValue += (Mathf.Clamp01(noise.Evaluate(point * 50 + settings.centre))*0.1f+0.9f)*0.15f;

        //Mountain Filter
        noiseIn = noise.Evaluate(point * 10 + settings.centre);
        layer = Mathf.Pow(Mathf.Clamp01(noiseIn + 0.1f), 2) * 1f;
        //noiseValue += layer;

        //Mountains
        noiseIn = 1 - Mathf.Abs(noise.Evaluate(point * 30 + settings.centre));
        layer = Mathf.Pow(noiseIn, 2)*layer*2;
        noiseValue += layer;

        //noiseIn = 1 - Mathf.Abs(noise.Evaluate(point * 40 + settings.centre));
        //layer = Mathf.Pow(noiseIn, 2)*5 * layer;
        //noiseValue += layer;

        float layer2 = layer;

        noiseIn = 1 - Mathf.Abs(noise.Evaluate(point * 200 + settings.centre));
        layer = Mathf.Pow(noiseIn, 2)*0.5f * Mathf.Pow(Mathf.Clamp01(layer+0.5f)-0.5f,2);
        noiseValue += layer;

        noiseIn = 1 - Mathf.Abs(noise.Evaluate(point * 210 + settings.centre));
        layer = -Mathf.Pow(noiseIn, 2) * 0.5f * Mathf.Pow(Mathf.Clamp01(layer2 + 0.5f) - 0.5f, 2);
        noiseValue += layer;

        noiseIn = 1 - Mathf.Abs(noise.Evaluate(point * 500 + settings.centre));
        layer = Mathf.Pow(noiseIn, 2)/10 * layer;
        noiseValue += layer;
        //noiseValue *= spike;

        noiseIn = noise.Evaluate(point * 400 + settings.centre);
        layer = Mathf.Clamp01(Mathf.Pow((noiseIn - 0.0f), 2)) * 0.05f;
        noiseValue += layer;

        //noiseValue = noiseValue - settings.minValue; 
        //noiseValue += spike;
        return noiseValue * settings.strength;
    }
}
