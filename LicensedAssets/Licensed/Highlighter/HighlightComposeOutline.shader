Shader "HighlightPlus/Geometry/ComposeOutline" {
Properties {
    _MainTex ("Texture", Any) = "black" {}
	_Color("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
	_Cull("Cull Mode", Int) = 2
	_ZTest("ZTest Mode", Int) = 0
		_Flip("Flip", Vector) = (0, 1, 0)
	_Debug("Debug Color", Color) = (0,0,0,0)
}
SubShader
	{
        UsePass "Standard/DEFERRED"
   }
}