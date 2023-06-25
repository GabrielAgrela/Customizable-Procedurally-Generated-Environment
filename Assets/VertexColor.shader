Shader "Custom/VertexColorTextureLit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert
        #include "UnityCG.cginc"

        struct Input {
            float2 uv_MainTex;
            fixed4 color : COLOR;
        };

        sampler2D _MainTex;


        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.color = v.color;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            if (all(IN.color == fixed4(1, 1, 1, 1))) {
                o.Albedo = c.rgb;
            }
            else {
                o.Albedo = (c * IN.color).rgb;
            }
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
