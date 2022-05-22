Shader "UI/Effect"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_GradientSpeedX("Gradient Speed X", Float) = -0.07
		_GradientSpeedY("Gradient Speed Y", Float) = 0.03
		_GradientNoiseScale("Gradient Noise Scale", Float) = 6
		_EffectTilingX("Effect Tiling X", Float) = 1.0
		_EffectTilingY("Effect Tiling Y", Float) = 0.5

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
        UsePass "Standard/DEFERRED"
	}
}