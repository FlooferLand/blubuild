shader_type spatial;

uniform vec3 color : source_color = vec3(1);
uniform sampler2D albedo : source_color;
uniform sampler2D alpha : source_color;
uniform sampler2D normal : source_color;
uniform int index = 0;

float random(vec2 uv) {
    return fract(sin(dot(uv.xy,
        vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment() {
	int i = index;

	// Reading the 4 tile map using the index
	vec2 uv = UV;
	uv.x = uv.x * 0.5;
	uv.x = uv.x + (float(i) * 0.5);
	uv.y = uv.y * 0.5;
	if (mod(float(i), 4.0) > 1.0) {
		uv.x -= 1.0;
		uv.y += 0.5;
	}

	// Random UV offset
	uv.x += random(uv) * 0.0015;
	uv.y += random(uv) * 0.0015;

	//float extra_darken = (float(i) / 4.0);
	ALBEDO = texture(albedo, uv).rgb * color;
	NORMAL = texture(normal, uv).rgb;
	ALPHA = clamp(texture(alpha, uv).r * 2.0, 0.0, 1.0);
}
