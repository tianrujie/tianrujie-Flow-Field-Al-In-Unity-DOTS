

Shader "TLStudio/Effect/Dissolve" 
{
    Properties 
	{
		[MaterialEnum(Blend,10,Add,1)] _Blend("Blend&Add", Int) = 1
		[MaterialEnum(off,0,on,1)] _ZWrite0("ZWrite", float) = 0
        _Color ("Color", Color) = (1,1,1,1)
		_ColorLight("ColorLight", Range(0,8)) = 1
        _MainTex ("MainTexture", 2D) = "white" {}
		[Toggle]_MainTexUVCustom("MainTexUVCustom", Float) = 0
		_MainTexUVRoll("MainTexUVRoll", Vector) = (0,0,0,0)
        _mask ("Mask", 2D) = "white" {}
		[Toggle]_UseCustom("DissolveCustom", Float) = 0
        _Dissolve ("Dissolve", Range(-0.1, 1)) = 0
		[Toggle]_SideWidthCustom("SideWidthCustom", Float) = 0
		_SideWidth("SideWidth", Range(0, 0.1)) = 0
        _SideColor ("SideColor", Color) = (1,1,1,1)
		_SideColorLight("SideColorLight", Range(0,8)) = 1
		[Header(Noise Texture Settings)]
		_Noise("Noise", 2D) = "white" {}
		_NoiseMulti("NoiseMulti", Range( 0 , 10)) = 0
		_NoiseUVRoll("NoiseUVRoll", Vector) = (0,0,0,0)
		[Space(10)][Toggle] _UseExposureFX("UseExposureFX",float) = 1
		
    }
    SubShader 
	{

        Tags {
            "Queue"="Transparent"
            "RenderType"="TransparentCutout"
        }

		Cull Off
		Blend SrcAlpha [_Blend]
		ZWrite [_ZWrite0]

        Pass 
		{         
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _mask; float4 _mask_ST;
			float4 _MainTexUVRoll;
            fixed _Dissolve;
            fixed4 _SideColor;
			fixed _SideWidth;
			fixed4 _Color;
			half _ColorLight;
			half _SideColorLight;
			half _ExposureFX;
			fixed _UseExposureFX;
			fixed _UseCustom;
			fixed _SideWidthCustom;

			sampler2D _Noise;float4 _Noise_ST;
			half4 _NoiseUVRoll;
			fixed _MainTexUVCustom;
			half _NoiseMulti;

            struct VertexInput 
			{
                float4 vertex : POSITION;
                fixed4 color :COLOR;
				float4 custom : TEXCOORD0;
				float4 custom2 : TEXCOORD1;
            };

            struct VertexOutput 
			{
                float4 pos : SV_POSITION;
				fixed4 color :COLOR;
				float4 custom : TEXCOORD0;
				float4 uv : TEXCOORD1;
				float4 custom2 : TEXCOORD2;
            };

            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv.xy = TRANSFORM_TEX(v.custom, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.custom, _mask);
				o.custom.xy = TRANSFORM_TEX(v.custom, _Noise);
				o.custom.zw = v.custom.zw;
				o.custom2 = v.custom2;
                return o;
            }

            half4 frag(VertexOutput i) : COLOR 
			{
				//Noise图的滚动和采样
				float2 NoiseUV = i.custom.xy + frac(_Time.y * _NoiseUVRoll.xy);
				fixed4 NoiseTex = tex2D( _Noise, NoiseUV );

				float2 MainTexUV = i.uv.xy + ( NoiseTex.r * _NoiseMulti * fixed2(-1,1) + _NoiseMulti * fixed2(0.2,-0.2)) + lerp(frac(_Time.y * _MainTexUVRoll.xy), i.custom2.xy, _MainTexUVCustom);
                fixed4 MainTexture = tex2D(_MainTex,MainTexUV);

				fixed4 Mask = tex2D(_mask,i.uv.zw);
				//溶解贴图的r乘以主贴图的A -Dissolve
				fixed dissolve = lerp(_Dissolve, i.custom.z, _UseCustom);
				fixed cut = Mask.r*MainTexture.a*0.99- dissolve;//乘个0.99是为了保证dissolve参数为1时，可以溶解完毕全部消失
				
                clip(cut-0.001);//减去个0.001是为了避免cut为0时模型会变成白色
				
				float sidewidth = lerp(_SideWidth, i.custom.w, _SideWidthCustom);
				fixed t = 1- smoothstep(0, sidewidth,cut);
				half4 finalColor = MainTexture * i.color * _ColorLight * _Color + lerp(0, _SideColor * _SideColorLight, t);
				finalColor.a = saturate(finalColor.a);
				finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb*(_ExposureFX+1.0), _UseExposureFX);
                return finalColor;
            }
            ENDCG
        }

		Pass 
		{
			Name "Caster"
			Tags { "LightMode" = "ShadowCaster" }
			
			
			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct a2v 
			{
				float4 vertex : POSITION;
				float4 custom : TEXCOORD0;
            };

			struct v2f { 
				V2F_SHADOW_CASTER;
				float4 custom : TEXCOORD0;
				float4 uv : TEXCOORD1;
			};
			
			sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _mask; float4 _mask_ST;
			
			v2f vert( a2v v )
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.uv.xy = TRANSFORM_TEX(v.custom, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.custom, _mask);
				o.custom = v.custom;
				return o;
			}
			
			half _Dissolve;
			half _UseCustom;
			
			float4 frag( v2f i ) : SV_Target
			{
				
				fixed4 MainTexture = tex2D(_MainTex,i.uv.xy);
				fixed4 Mask = tex2D(_mask,i.uv.zw);
				//溶解贴图的r乘以主贴图的A -Dissolve
				fixed dissolve = lerp(_Dissolve, i.custom.z, _UseCustom);
				fixed cut = Mask.r*MainTexture.a*0.99- dissolve;//乘个0.99是为了保证dissolve参数为1时，可以溶解完毕全部消失
				
                clip(cut-0.001);//减去个0.001是为了避免cut为0时模型会变成白色
				
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

		}
    }
    
}
