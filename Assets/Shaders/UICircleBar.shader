// Good guide: https://github.com/Centribo/Unity-Shader-Basics-Tutorial

Shader "Unlit/UICircleBar"
{
    Properties
    {
        _MainColor("Example color", Color) = (.25, .5, .5, 1)
        _FillAmount ("Fill Amount", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        LOD 100

        Pass {
			CGPROGRAM
				#pragma vertex vertexFunction
				#pragma fragment fragmentFunction
                #pragma target 3.0

				#include "UnityCG.cginc"

                uniform float _FillAmount;
                uniform float4 _MainColor;

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vertexFunction (appdata IN) {
					v2f OUT;
                    OUT.position = UnityObjectToClipPos(IN.vertex);
                    OUT.uv = IN.uv;
					return OUT;
				}

				fixed4 fragmentFunction (v2f IN) : SV_TARGET {
                    fixed4 color = _MainColor;

                    if (IN.uv.y < _FillAmount) {
                        // If the UV x coordinate is less than the fill amount, return the color
                        return color;
                    } else {
                        // Otherwise, return transparent
                        return fixed4(0, 0, 0, 0);
                    }
				}
			ENDCG
		}
    }
}
