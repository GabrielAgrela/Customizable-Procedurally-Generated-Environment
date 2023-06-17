Shader "LowPolyWater/WaterShaded" {
Properties { 

    _MainTex ("Main Texture", 2D) = "white" {}
    _MainTexTiling ("Main Texture Tiling", Vector) = (1, 1, 0, 0)
    _SpecColor ("Specular Material Color", Color) = (1,1,1,1) 
    _Shininess ("Shininess", Float) = 10
    _ShoreTex ("Shore & Foam texture ", 2D) = "black" {} 
     
    _InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.2 ,0.39, 0.5, 1.0)

    _BumpTiling ("Foam Tiling", Vector) = (1.0 ,1.0, -2.0, 3.0)
    _BumpDirection ("Foam movement", Vector) = (1.0 ,1.0, -1.0, 1.0) 

    _Foam ("Foam (intensity, cutoff)", Vector) = (0.1, 0.375, 0.0, 0.0) 
    [MaterialToggle] _isInnerAlphaBlendOrColor("Fade inner to color or alpha?", Float) = 0 
}


CGINCLUDE 

	#include "UnityCG.cginc" 
	#include "UnityLightingCommon.cginc" // for _LightColor0


	sampler2D _ShoreTex;
	sampler2D _MainTex;
	sampler2D_float _CameraDepthTexture;
  
	uniform float4 _BaseColor;  
    uniform float _Shininess;
	 float4 _MainTexTiling;
	uniform float4 _InvFadeParemeter;
    
	uniform float4 _BumpTiling;
	uniform float4 _BumpDirection;
 
	uniform float4 _Foam; 
  	float _isInnerAlphaBlendOrColor; 
	#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz 


	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
 
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 grabPassPos : TEXCOORD4; 
		half3 worldRefl : TEXCOORD6;
		float4 posWorld : TEXCOORD7;
        float3 normalDir : TEXCOORD8;

		UNITY_FOG_COORDS(5)
	}; 
 
	inline half4 Foam(sampler2D shoreTex, half4 coords) 
	{
		half4 foam = (tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw)) - 0.125;
		return foam;
	}

	v2f vert(appdata_full v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);

		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;
 
		half3	offsets = half3(0,0,0);
		half3	nrml = half3(0,1,0);
		
		v.vertex.xyz += offsets;
		 
		half2 tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;
		
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.screenPos=ComputeScreenPos(o.pos); 
		o.normalInterpolator.xyz = nrml;
		o.viewInterpolator.w = saturate(offsets.y);
		o.normalInterpolator.w = 1; 
		
		UNITY_TRANSFER_FOG(o,o.pos);
 		half3 worldNormal = UnityObjectToWorldNormal(v.normal); 
   		float4x4 modelMatrix = unity_ObjectToWorld;
        float4x4 modelMatrixInverse = unity_WorldToObject; 
	 	o.posWorld = mul(modelMatrix, v.vertex);
        o.normalDir = normalize( mul(float4(v.normal, 0.0), modelMatrixInverse).xyz); 

        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)); 
        o.worldRefl = reflect(-worldViewDir, worldNormal);

		return o;
	}
 
	 half4 calculateBaseColor(v2f input)  
    {
        // Sample texture color with tiling
        half4 baseColor = tex2D(_MainTex, input.posWorld.xz * _MainTexTiling.xy);

        // Return the base texture color directly without lighting calculations
        return half4(baseColor.rgb, 1.0);
    }


	half4 frag( v2f i ) : SV_Target
	{ 
 
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
 
        half4 baseColor = calculateBaseColor(i);
       
 
		half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));
		if( _isInnerAlphaBlendOrColor==0)
			baseColor.rgb += 1.0-edgeBlendFactors.x;
		if(  _isInnerAlphaBlendOrColor==1.0)
			baseColor.a  =  edgeBlendFactors.x;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
ENDCG

Subshader
{
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 500
	ColorMask RGB
	
	GrabPass { "_RefractionTex" }
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull Off
		
			CGPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF 
		
			ENDCG
	}
}


Fallback "Transparent/Diffuse"
}
