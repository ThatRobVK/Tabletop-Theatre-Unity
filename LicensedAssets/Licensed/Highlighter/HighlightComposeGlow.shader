Shader "HighlightPlus/Geometry/ComposeGlow" {
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    [HideInInspector] _Cull ("Cull Mode", Int) = 2
    [HideInInspector] _ZTest ("ZTest Mode", Int) = 0
	[HideInInspector] _Flip("Flip", Vector) = (0, 1, 0)
	_Debug("Debug Color", Color) = (0,0,0,0)
	[HideInInspector] _StereoRendering("Stereo Rendering Correction", Float) = 1
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}