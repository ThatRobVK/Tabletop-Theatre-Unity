Shader "HighlightPlus/Geometry/Glow" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Glow ("Glow", Vector) = (1, 0.025, 0.75, 0.5)
    _Glow2 ("Glow2", Vector) = (0.01, 1, 0.5, 0)
    _GlowColor ("Glow Color", Color) = (1,1,1)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _GlowDirection("GlowDir", Vector) = (1,1,0)
    _Cull ("Cull Mode", Int) = 2
    _ConstantWidth ("Constant Width", Float) = 1
	_GlowZTest ("ZTest", Int) = 4
    _GlowStencilOp ("Stencil Operation", Int) = 0
}
    SubShader
    {
        UsePass "Standard/DEFERRED"

    }
}