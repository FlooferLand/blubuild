shader_type canvas_item;

uniform float cutoff: hint_range(0.0, 1.0) = 0.0;
uniform float pull: hint_range(0.0, 1.0) = 0.0;
uniform float blend: hint_range(0.0, 1.0) = 0.1;
uniform int blend_frames: hint_range(0, 5) = 0;
uniform bool invert = false;
uniform sampler2D mask_texture;
uniform sampler2D display_texture;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture;

void fragment() {
	vec2 screenUv = SCREEN_UV;
	vec2 uv = UV;
	screenUv.y = screenUv.y + pull;
	uv.y = uv.y + pull;
	
	vec4 sample_color = texture(mask_texture, uv);
	float luminance = dot(sample_color.rgb, vec3(0.2126, 0.7152, 0.0722));
	bool filter = luminance > cutoff;
	if (invert) filter = !filter;
	
	
	if (filter) {
		COLOR = texture(display_texture, uv);
	} else {
		float diff = abs(cutoff - luminance);
		float blend2 = blend;
		if (cutoff == 1.0 || cutoff == 0.0) blend2 = 0.0;
		float alpha = pow(clamp(1.0 - diff, 0.0, 1.0), float(5 - blend_frames)) * blend2;
		
		vec4 screen_texture = texture(SCREEN_TEXTURE, screenUv);
		vec4 tex = texture(display_texture, screenUv);
		COLOR = mix(screen_texture, tex, alpha);
	}
}
