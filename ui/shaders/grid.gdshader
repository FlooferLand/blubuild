shader_type canvas_item;

uniform float opacity : hint_range(0.0, 1.0, 0.001) = 1.0;
uniform float tile = 1.0;

void vertex() {
	UV = UV * vec2(tile);
}

void fragment() {
	vec4 tex_color = texture(TEXTURE, UV);
    COLOR = tex_color * opacity;
}