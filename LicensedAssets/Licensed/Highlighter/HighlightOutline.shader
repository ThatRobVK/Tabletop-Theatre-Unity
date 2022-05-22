Shader "HighlightPlus/Geometry/Outline" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    _OutlineWidth ("Outline Offset", Float) = 0.01
    _OutlineDirection("Outline Direction", Vector) = (0,0,0)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _Cull ("Cull Mode", Int) = 2
    _ConstantWidth ("Constant Width", Float) = 1
	_OutlineZTest("ZTest", Int) = 4
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}