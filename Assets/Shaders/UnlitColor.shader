Shader "Custom/EmissiveColor" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Emission ("Emission", Range(0, 2)) = 1
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float4 _Color;
        float _Emission;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
            o.Specular = 0.9;
            o.Gloss = 0.9;
            o.Emission = _Color.rgb * _Emission;
        }
        ENDCG
    }
    FallBack "Diffuse"
}