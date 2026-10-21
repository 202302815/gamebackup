Shader "UniTopia/PlayerHiddenOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0.85,0.2,1)
        _OutlinePixels ("Outline Width", Float) = 1
        _UVRect ("Sprite UV Bounds", Vector) = (0,0,1,1)

        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Stencil
        {
            Ref 64
            ReadMask 64
            WriteMask 0
            Comp Equal
            Pass Keep
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            float4 _MainTex_TexelSize;
            float4 _UVRect;
            float _OutlinePixels;
            fixed4 _OutlineColor;

            float SpriteAlpha(float2 uv)
            {
                if (uv.x < _UVRect.x || uv.y < _UVRect.y ||
                    uv.x > _UVRect.z || uv.y > _UVRect.w)
                    return 0;

                return SampleSpriteTexture(uv).a;
            }

            fixed4 OutlineFragment(v2f input) : SV_Target
            {
                float2 offset =
                    _MainTex_TexelSize.xy * _OutlinePixels;

                float center = step(
                    0.1, SpriteAlpha(input.texcoord));

                float surrounding = 1;

                surrounding = min(surrounding, step(0.1,
                    SpriteAlpha(input.texcoord + float2(offset.x, 0))));

                surrounding = min(surrounding, step(0.1,
                    SpriteAlpha(input.texcoord - float2(offset.x, 0))));

                surrounding = min(surrounding, step(0.1,
                    SpriteAlpha(input.texcoord + float2(0, offset.y))));

                surrounding = min(surrounding, step(0.1,
                    SpriteAlpha(input.texcoord - float2(0, offset.y))));

                // Draw only the inside edge of the sprite.
                float edge = center * (1 - surrounding);

                fixed4 color = _OutlineColor;
                color.a *= edge * input.color.a;
                color.rgb *= color.a;

                return color;
            }

            ENDCG
        }
    }
}
