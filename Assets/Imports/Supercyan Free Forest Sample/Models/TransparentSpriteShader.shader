Shader "Custom/TransparentSpriteShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _TransparencyThreshold ("Transparency Threshold", Range(0, 1)) = 0.9
        _SwayAmount ("Sway Amount", Range(0, 1)) = 0.1
        _SwaySpeed ("Sway Speed", Range(0, 10)) = 1.0
        _BillboardDistance ("Billboard Distance", Range(1, 500)) = 10.0
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100
Cull Off
        CGPROGRAM
        #pragma surface surf Lambert vertex:SwayVertex
        #pragma target 3.0

        sampler2D _MainTex;
        float _TransparencyThreshold;
        float _SwayAmount;
        float _SwaySpeed;
        float _BillboardDistance;

        struct Input {
            float2 uv_MainTex;
            float3 vertexPosition;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            if (c.r > _TransparencyThreshold && c.g > _TransparencyThreshold && c.b > _TransparencyThreshold) {
                o.Alpha = 0;
            }
            clip(o.Alpha - 0.01);
        }

         void SwayVertex (inout appdata_full v) {
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos);
    float3 up = float3(0, 1, 0);
    float3 right = cross(up, viewDir);
    up = cross(viewDir, right);

    float distanceToCamera = distance(_WorldSpaceCameraPos, worldPos);
    float billboardFactor = saturate((distanceToCamera - _BillboardDistance) / 5.0);

    float3x3 rotMatrix = lerp(float3x3(right, up, viewDir), unity_ObjectToWorld, billboardFactor);
    float baseHeight = 0.0;
    float yOffset = v.vertex.y - baseHeight;
    v.vertex.y = baseHeight;
    v.vertex.xyz = mul(v.vertex, rotMatrix);
    v.vertex.y += yOffset;

    float swayFactor = smoothstep(0.0, 1.0, v.vertex.y);
    float randomSeed = frac(sin(dot(worldPos.xz, float2(12.9898, 78.233))) * 43758.5453);
    float swayingOffset = _SwayAmount * swayFactor * (
        sin((_Time.y + randomSeed) * _SwaySpeed * 0.5 + v.vertex.y * 3.0) * 0.4 +
        sin((_Time.y + randomSeed) * _SwaySpeed * 1.5 + v.vertex.y * 6.0) * 0.35 +
        sin((_Time.y + randomSeed) * _SwaySpeed * 2.5 + v.vertex.y * 10.0) * 0.25
    );
    v.vertex.x += swayingOffset;
}




        ENDCG
    }
    FallBack "Diffuse"
}
