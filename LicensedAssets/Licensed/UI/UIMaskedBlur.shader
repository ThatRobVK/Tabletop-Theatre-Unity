// Author: vinipc
// Forum post: https://forum.unity3d.com/threads/simple-optimized-blur-shader.185327/#post-3038561
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Based on cician's shader from: https://forum.unity3d.com/threads/simple-optimized-blur-shader.185327/#post-1267642

Shader "Custom/MaskedUIBlur"
{
	Properties
	{
		_Size("Blur", Range(0, 30)) = 1
		[HideInInspector] _MainTex("Tint Color (RGB)", 2D) = "white" {}
	}

	SubShader
	{
        UsePass "Standard/DEFERRED"
	}
}
