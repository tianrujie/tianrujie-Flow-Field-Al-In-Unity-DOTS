
Shader "TLStudio/Effect/FresnelRamp"
{
	Properties
	{		

		_FresnelColor("FresnelColor", Color) = (0,1,0,1)
		_FresnelColorLight("FresnelColorLight", Range(0,8)) = 1
		_Fresnel_Width("Fresnel_Width", Range( 0 , 2)) = 0.6
		_Fresnel_Pow("Fresnel_Pow", Range( 0 , 2)) = 1
		[Toggle]_FresnelInside("Inside?", Float) = 0
		_Blink("Blink",Range(0,2)) = 0
		

		[Header(DoubleFresnel)]
		[Toggle]_UseDoubleFresnel("UseDoubleFresnel", float) = 0
		
		_FresnelInColor("FresnelInColor", Color) = (1,0,0,1)
		_FresnelInColorLight("FresnelInColorLight", Range(0,10)) = 1
		_FresnelOutColor("FresnelOutColor", Color) = (0,1,0,1)
		_FresnelOutColorLight("FresnelOutColorLight", Range(0,10)) = 1
		_FresnelDoubleControl("FresnelDoubleControl", vector) = (0,0.5,0.5,1)

		

		[Header(UesTexture)]
		[Toggle] _UseTexture("UseTexture", Float) = 0.0
		[MaterialEnum(Blend, 10, Add, 1)] _Blend("BlendMode", Int) = 1
		[MaterialEnum(Dark, 1, Bright, 0)] _TextureColorBlend("TextureColorBlendMode", Int) = 0
		[MaterialEnum(off,0,on,1)] _ZWrite("ZWrite", float) = 0
		[HDR]_TextureColor("TextureColor", color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle] _ObjectUV("MainTexObjectUV", Float) = 0.0
		[Toggle]_MainCustom("Main_UseCustom",float) = 0

		//[Space(20)]
		_DissolveMask("DissolveTex", 2D) = "white" {}
		[Toggle]_DissolveUVCustom("Dissolve_UseCustom",float) = 0
		_DissolveIntensity("DissolveIntensity", Range(0,5)) = 1
		_DissolveControl("DissolveControl", Range(-1,1)) = 0
		[Toggle]_DissolveCustom("DissolveCustom",float) = 0
		_MoveSpeed("TexUVRoll",vector) = (0,0,0,0)
		_alphaCutoff("alphaCutoff", Range(0,1)) = 0


		[Header(Vertex Animation)]
		_WaveScale("WaveScale",Range(0,2)) = 0
		[Toggle]_WaveScaleCustom("WaveScaleCustom", float) = 0
		_WaveSpeed("WaveSpeed",Range(0,10)) = 0
		_Frequency("Wave Frequency", Range(0, 4)) = 1
		[Toggle] _WaveDirection("WaveDirection",float) = 0

		[Space(10)][Toggle] _UseExposureFX("UseExposureFX",float) = 1
		[HideInInspector]_Opacity ("Opacity", Range(0, 1)) = 1

		[Header(______NO___USE______)]
		_RampColor("RampColor", Color) = (0,0,0,0)
		_RampColorLight("RampColorLight", Range(0,8)) = 1
		//_RampTexture("RampTexture", 2D) = "white" {}
		//[Toggle]_UVChange1("UVChange1", Float) = 0
		//[Toggle]_UVChange2("UVChange2", Float) = 0
		
	}
	
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True"  "RenderType"="Transparent" }
		  
		Blend SrcAlpha [_Blend]
		Cull Back
		ZWrite [_ZWrite]
		Offset -1, -1

		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM

			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;	
				float4 custom1: TEXCOORD1;
				fixed3 normal : NORMAL;
				fixed4 color : COLOR;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;	
				float4 custom1 : TEXCOORD1;
				float3 worldViewDir : TEXCOORD2;
				float3 specular : TEXCOORD3;
				fixed3 worldNormal : NORMAL;
				fixed3 normal : NORMAL1;//高光计算用法线，在像素阶段转为世界空间
				fixed4 color : COLOR;
			};

			
			fixed4 _RampColor;
			half _RampColorLight;
			fixed4 _FresnelColor;
			half _FresnelColorLight;
			fixed _Fresnel_Width;
			fixed _Fresnel_Pow;
			fixed _FresnelInside;

			fixed4 _FresnelInColor;
			fixed4 _FresnelOutColor;
			half _FresnelInColorLight;
			half _FresnelOutColorLight;
			fixed4 _FresnelDoubleControl;
			fixed _UseDoubleFresnel;
			fixed _TextureColorBlend;

			
			sampler2D _MainTex; float4 _MainTex_ST;
			fixed _MainCustom;
			float4 _TextureColor;
			fixed _UseTexture;
			fixed _alphaCutoff;
			half4 _MoveSpeed;
			fixed _ObjectUV;
			fixed _UseExposureFX;
			half _ExposureFX;

			fixed _Blink;
			fixed _Opacity=1.0;

			half _WaveScale;
			fixed _WaveScaleCustom;
			half _WaveSpeed;
			half _Frequency;
			fixed _WaveDirection;

			sampler2D _DissolveMask;float4 _DissolveMask_ST;
			fixed _DissolveUVCustom;
			half _DissolveIntensity;
			fixed _DissolveControl;
			fixed _DissolveCustom;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				//顶点波浪动画
				float Direction = lerp(v.texcoord.x, v.texcoord.y, _WaveDirection);
				float wave = sin((Direction * _Frequency + _WaveSpeed * _Time.g)*6);
				v.vertex.xyz += (wave * lerp(_WaveScale, v.custom1.w, _WaveScaleCustom) * v.normal);
				o.vertex = UnityObjectToClipPos(v.vertex);

				
				o.uv.zw = TRANSFORM_TEX(v.texcoord,_DissolveMask) + lerp(frac(_Time * _MoveSpeed.zw), v.texcoord.zw, _DissolveUVCustom);

				//流光可以用模型的顶点位置作为uv避免uv接缝
				half2 UV      = TRANSFORM_TEX(v.texcoord, _MainTex) + lerp(frac(_Time * _MoveSpeed.xy), v.custom1.xy, _MainCustom);;
				half2 ObjectUV = (v.vertex.xy+(_Time * _MoveSpeed.xy))*_MainTex_ST.xy;
				o.uv.xy = lerp(UV, ObjectUV, _ObjectUV);

				o.normal = v.normal;
				o.worldNormal = normalize(mul(unity_ObjectToWorld,fixed4( v.normal , 0.0 )));
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldViewDir = UnityWorldSpaceViewDir(worldPos);
				o.worldViewDir.xyz = normalize(worldViewDir);

				o.custom1 = v.custom1;
				o.color = v.color;	
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed FresnelDot = 1.0 -  abs( dot(i.worldViewDir, i.worldNormal) );
				fixed FresnelSingle = smoothstep( ( 1.0 - _Fresnel_Width ) , 1 , FresnelDot ); 
				FresnelSingle = lerp(FresnelSingle, smoothstep( 1 , ( 1.0 - _Fresnel_Width ) , FresnelDot ), _FresnelInside); 
				float4 FresnelSingleColor = ( _FresnelColor * _FresnelColorLight * FresnelSingle ) * _Fresnel_Pow;

				//为了避免影响之前，双层fresnel单开控制
				fixed FresnelOut = smoothstep(_FresnelDoubleControl.z, _FresnelDoubleControl.w, FresnelDot);
				fixed FresnelIn  = abs(smoothstep(_FresnelDoubleControl.x, _FresnelDoubleControl.y, FresnelDot) - FresnelOut);
				fixed FresnelDouble = FresnelOut + FresnelIn;
				float4 FresnelDoubleColor = _FresnelInColor * float4(_FresnelInColorLight.xxx,1) * FresnelIn + _FresnelOutColor * float4(_FresnelOutColorLight.xxx,1) * FresnelOut;

				float4 FresnelColor = lerp(FresnelSingleColor, FresnelDoubleColor, _UseDoubleFresnel);
				fixed Fresnel = lerp(FresnelSingle, FresnelDouble, _UseDoubleFresnel);
				
				//ramp是历史遗留，后期可删除
				float4 Col =  ( ( _RampColor * _RampColorLight) + FresnelColor ) * i.color;

				
				fixed4 maintex = tex2D(_MainTex, i.uv.xy);
				float3 tex =  maintex.rgb*_TextureColor.rgb*_TextureColor.a;

				//是否使用贴图
				float3 colA = Col.rgb + lerp(0, tex, _UseTexture);
				float3 colB = Col.rgb * lerp(1, tex, _UseTexture);
				Col.rgb = lerp(colA, colB, _TextureColorBlend);

				//软溶解mask
				half2 DissolveUV = i.uv.zw ;
				fixed4 MaskDissolve = tex2D(_DissolveMask, DissolveUV);
				fixed SoftDissolve = saturate(saturate(MaskDissolve.r * _DissolveIntensity) - lerp(_DissolveControl,i.custom1.z, _DissolveCustom));

				//_RampColor.a是历史遗留，后期可删除
				fixed alpha = ( i.color.a * saturate( _RampColor.a + ( lerp(_FresnelColor.a, FresnelColor.a, _UseDoubleFresnel) * Fresnel ) ) );
				Col.a = lerp(alpha, alpha * maintex.a*_TextureColor.a*SoftDissolve, _UseTexture) * abs(cos(frac(_Time.y*_Blink)*6.283));
				Col.a = saturate(Col.a)*_Opacity;
				//用到主贴图时可以使用alpha裁剪
				if(_alphaCutoff > 0)
				{
				clip(Col.a - _alphaCutoff);
				}
				Col.rgb = lerp(Col.rgb, Col.rgb*(_ExposureFX+1.0), _UseExposureFX);//整体亮度
				return Col;
			}
			ENDCG
		}
	}
}
