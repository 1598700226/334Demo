Shader "Custom/Clouds" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Brightness ("Brightness", Range (0.5, 5)) = 1.5
	_Normals("Normal Map tl", 2D) = "black" {}
    _NormalStrength ("Normal Strength", Range (0, 2)) = 1
}
 
SubShader {
	Tags {"Queue"="Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200
	Offset -1, -2
 
CGPROGRAM
#pragma surface surf Lambert finalcolor:nightcolor alpha
 
sampler2D _MainTex;

sampler2D _Normals;
half _NormalStrength;
half _Brightness; 
 
struct Input {
	float2 uv_MainTex;
	float2 uv_Normals;
	float3 viewDir;
};
 
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	float4 Sampled2D0=tex2D(_Normals,IN.uv_Normals.xy);
   	float4 UnpackNormal0=float4(UnpackNormal(Sampled2D0).xyz, 1.0);
   	half rim =  1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
	o.Albedo = c.rgb * _Brightness * (1+(rim*rim));
	UnpackNormal0.xy = UnpackNormal0.xy*_NormalStrength;
	o.Normal = UnpackNormal0;
	o.Alpha = c.a * 1-(rim*rim*rim*rim);
	o.Normal = normalize(o.Normal);
}
 
void nightcolor (Input IN, SurfaceOutput o, inout fixed4 color)
{
	color.r = saturate(o.Albedo.r - (o.Albedo.r - color.r) * 1.02);
	color.g = saturate(o.Albedo.g - (o.Albedo.g - color.g) * 1.028);
	color.b = saturate(o.Albedo.b - (o.Albedo.b - color.b) * 1.03);
}
 
ENDCG
}
 
Fallback "Transparent/VertexLit"
}