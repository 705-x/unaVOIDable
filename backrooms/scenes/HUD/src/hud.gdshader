shader_type canvas_item;
uniform float value : hint_range(0.0, 1.0, 0.01);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec3 color  = texture(TEXTURE, UV).rgb;
	COLOR.rgb = mix(vec3(value), color, 0.3);	
	COLOR.a = 0.5;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
