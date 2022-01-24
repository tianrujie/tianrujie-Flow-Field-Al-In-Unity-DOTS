Shader "Unlit/Cell_Renderer_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("_Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing //没这句 shader上不显示instance的toggle 相当于没开
            #include "UnityCG.cginc"

            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            UNITY_INSTANCING_BUFFER_START(InstanceProperties)
                UNITY_DEFINE_INSTANCED_PROP(float4,_Color)
            UNITY_INSTANCING_BUFFER_END(InstanceProperties)
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //设置当前实例id 没这句 所有顶点都是第一个位置的顶点...
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);//设置当前实例id 没这句 底下这些数都是数组里第一个值
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //return col;
                return UNITY_ACCESS_INSTANCED_PROP(InstanceProperties, _Color).rgba * col;
            }
            ENDCG
        }
    }
}
