shader_type spatial;
render_mode unshaded;

uniform sampler2D albedo_texture;
uniform vec4 base_color = vec4(1.0, 1.0, 1.0, 1.0);

uniform float ray_length = 1.0;
uniform float ray_shift = 0.0;

void fragment() {
	vec2 uv = UV;
	uv.y -= ray_shift;
	vec4 color = texture(albedo_texture, uv);

	float len_fade = 0.1;
	float len_visible = (1.0 - len_fade) * ray_length;

	if (UV.y >= len_visible + len_fade) {
		color.a = 0.0;
	} else if (UV.y > len_visible) {
		float yt = (len_visible + len_fade - UV.y) / len_fade;
		color.a *= yt;
		color.a *= smoothstep(0, 1, yt);
	}

	ALBEDO = color.rgb;
	ALPHA = color.a;
}
