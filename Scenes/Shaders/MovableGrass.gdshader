shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color,filter_linear_mipmap,repeat_enable;
uniform float point_size : hint_range(0,128);
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_metallic : hint_default_white,filter_linear_mipmap,repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear_mipmap,repeat_enable;
uniform float specular;
uniform float metallic;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

uniform vec3 character_position;
uniform float character_radius;
uniform sampler2D character_distance_falloff_curve : hint_default_black;
uniform float character_push_strength = 1.0;
uniform float height_strength_adjust = 1.0;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
	
	
	float displacement_by_height = smoothstep(0.0, 1.0, VERTEX.y * height_strength_adjust);
	
	vec3 direction_to_character = character_position - MODEL_MATRIX[3].xyz;
	direction_to_character.y = 0.0;
	float distance_to_character = length(direction_to_character);
	float falloff = 1.0 - smoothstep(0.0, 1.0, distance_to_character / character_radius);
	direction_to_character = (inverse(MODEL_MATRIX) * vec4(direction_to_character, 0.0)).xyz;
	direction_to_character = normalize(direction_to_character);
	
	float falloff_curve = texture(character_distance_falloff_curve, vec2(falloff)).x;
	
	VERTEX += normalize(-direction_to_character) * falloff_curve * character_push_strength * displacement_by_height;
}

void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
	float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;
	SPECULAR = specular;
}

