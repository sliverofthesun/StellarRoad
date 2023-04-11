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
        public int nOfSatellites; //in m/s
        public float SMA_AU; //in AU
        public float orbitalPeriod; //in years
        public float eq_temperature; //in Kelvin
        public float surface_temperature; //temperature at the surface of the planet or at 1 ATM pressure (if gas giant)
        public bool hasAtmosphere;
        public float trueAnomaly;
        public PlanetComposition composition;
        public AtmosphereComposition atmoComposition;
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

    [System.Serializable]
    public class AtmosphereComposition
    {
        public float greenhouse_effect; //float > 0
        public float surfacePressure; //in ATM pressures (1 = 1 earth pressure). Atmospheric height.
        public float molMass; //in kg/mol
        public float atmoHeight; //in km
    }
}