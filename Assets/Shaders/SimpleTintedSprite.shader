Shader "Custom/Simple Tinted Sprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags 
		{ 
			"Queue"="Transparent"
		}

		Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half4 color : COLOR;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				half4 color : TEXCOORD1;
			};

			sampler2D _MainTex;
			half4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);
				col.rgb = lerp(col.rgb, i.color.rgb, i.color.a);
				return col;
			}
			ENDCG
		}
	}
}
