Shader "CW/Item" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Main Color", COLOR) = (1,1,1,1)
}

SubShader {
	Tags { "RenderType"="Opaque" }

	Pass {
		Tags { "LightMode" = "Vertex" }
		//Color [_Color]
		
		ColorMaterial AmbientAndDiffuse
		//Lighting On
	}	
}
}
