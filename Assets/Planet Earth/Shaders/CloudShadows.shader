Shader "Custom/CloudShadows" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_ShadowsDarkness ("Shadows darkness", Range (0, 1)) = 0.8
}
 
SubShader {
	Tags {"Queue"="Transparent-10" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200
	Offset -1, -2
 
CGPROGRAM
#pragma surface surf Lambert finalcolor:nightcolor alpha
 
half _ShadowsDarkness;
sampler2D _MainTex;
 
struct Input {
	float3 viewDir;
	float2 uv_MainTex;
};
 
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb*0.01;
	half rim =  saturate(dot (normalize(IN.viewDir), o.Normal));
	o.Alpha = c.a*_ShadowsDarkness*rim*rim;
}
 
void nightcolor (Input IN, SurfaceOutput o, inout fixed4 color)
{
	color.r = saturate(o.Albedo.r - (o.Albedo.r - color.r) * 1.1);
	color.g = saturate(o.Albedo.g - (o.Albedo.g - color.g) * 1.05);
}
 
ENDCG
}
 
Fallback "Transparent/VertexLit"
}