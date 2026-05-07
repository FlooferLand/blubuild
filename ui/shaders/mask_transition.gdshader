shader_type canvas_item;

uniform float cutoff: hint_range(0.0, 1.0) = 0.0;
uniform float pull: hint_range(0.0, 1.0) = 0.0;
uniform float blend: hint_range(0.0, 1.0) = 0.1;
uniform int blend_frames: hint_range(0, 5) = 0;
uniform bool invert = false;
uniform sampler2D mask_texture;
uniform sampler2D display_texture;

void fragment() {
	vec2 uv = UV;
	uv.y = uv.y + pull;
	
	vec4 sample_color = texture(mask_texture, uv);
	float luminance = dot(sample_color.rgb, vec3(0.2126, 0.7152, 0.0722));
	bool filter = luminance > cutoff;
	if (invert) filter = !filter;
	
	if (filter) {
		COLOR = texture(display_texture, uv);
	} else {
		float diff = cutoff - luminance;
		float blend2 = blend;
		if (cutoff == 1.0 || cutoff == 0.0) blend2 = 0.0;
		COLOR = vec4(1.0, 1.0, 1.0, pow(1.0 - diff, float(5 - blend_frames)) * blend2) * texture(display_texture, uv);
	}
}
