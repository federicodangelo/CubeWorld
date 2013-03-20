Shader "CW/Normal" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }

	Pass {
		Tags { "LightMode" = "Vertex" }
		ColorMaterial AmbientAndDiffuse

		//Lighting On
		SetTexture [_MainTex] {
			Combine texture * primary double, texture * primary 
		} 
	}	
}
}
