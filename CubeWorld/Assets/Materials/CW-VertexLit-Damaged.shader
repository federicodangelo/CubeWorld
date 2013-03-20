Shader "CW/Normal-Damaged" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DamageTex ("Damage (RGBA)", 2D) = "black" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }

	Pass {
		Tags { "LightMode" = "Vertex" }
		ColorMaterial AmbientAndDiffuse
		
		BindChannels {
		   Bind "Vertex", vertex
		   Bind "texcoord", texcoord0
		   Bind "texcoord1", texcoord1
		   Bind "Color", color
		} 		
		
		//Lighting On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
		
		// Blend in the alpha texture
        SetTexture [_DamageTex] {
            combine previous +- texture
        }
	}
}

}
