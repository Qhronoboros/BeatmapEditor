// Documentation: https://docs.unity3d.com/2018.4/Documentation/Manual/class-CustomRenderTexture.html

Shader "CustomRenderTexture/WaveformShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("InputTex", 2D) = "white" {}
     }

     SubShader
     {
		Lighting Off
		Blend One Zero

        Pass
        {
            Name "WaveformShader"

            HLSLPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

			StructuredBuffer<half> buffer;

			int offset, skipn;
            int bufferSize;

            float4 _Color;
            sampler2D _MainTex;
            
            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float verticalCenteredPos = abs(IN.localTexcoord.y - 0.5);
                float pointHeight = buffer[(IN.localTexcoord.x * _CustomRenderTextureWidth) + offset] / 4.0;

                float stepped = 1.0 - step(pointHeight, verticalCenteredPos);

                return _Color * stepped.xxxx;
            }
            ENDHLSL
        }
    }
}
