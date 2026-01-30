shader_type canvas_item;

uniform float saturation : hint_range(0.0, 1.0, 0.01);
uniform sampler2D screen_texture : hint_screen_texture;

void fragment() {
	vec4 tex_color  = texture(screen_texture, SCREEN_UV, 0.0);
	COLOR.rgb = mix(vec3(dot(tex_color.rgb, vec3(0.299, 0.587, 0.114))), tex_color.rgb, saturation);
	COLOR.a = tex_color.a;
}
