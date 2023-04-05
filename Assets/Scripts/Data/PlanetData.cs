using System;

namespace PlanetDataNamespace
{
    [System.Serializable]
    public class Planet
    {
        public int orderInSystem;
        public string compositionDescriptor;
        public string planetType;
        public float mass;
        public float radius;
        public float density;
        public float surfaceGravity;
        public float SMA_AU;
        public float temperature_K;
        public bool hasAtmosphere;
        public float trueAnomaly;
        public PlanetComposition composition;
        public int PlanetSeed;
    }

    [System.Serializable]
    public class PlanetComposition
    {
        public float percentageOfGases;
        public float percentageOfLiquids;
        public float percentageOfSilicates;
        public float percentageOfMetals;
    }
}