using System;

namespace PlanetDataNamespace
{
    [System.Serializable]
    public class Planet
    {
        public int orderInSystem;
        public string compositionDescriptor;
        public string planetType;
        public float mass; //in Earth masses
        public float radius; //in Earth radii
        public float density; //in kg/m3
        public float surfaceGravity; //in m/s2
        public float escapeVelocity; //in m/s
        public float SMA_AU; //in AU
        public float orbitalPeriod; //in years
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