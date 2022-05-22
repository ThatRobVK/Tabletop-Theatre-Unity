Shader "HighlightPlus/Geometry/SeeThroughOccluder" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}