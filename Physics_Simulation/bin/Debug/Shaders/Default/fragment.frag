#version 450 core

in vec3 vertex;
in vec2 texcoord;
in vec3 normal;

out vec4 color;

const vec3 default_color          = vec3(1.0f,1.0f,0.8f);
const vec3 default_light_position = vec3(0,0,0);

void main()
{  
	vec3 unitNormal = normalize(normal);
	
	vec3 unitLightVector = normalize(vec3(0,1,0) - vertex);
	
	float brightness = max(dot(unitNormal, unitLightVector), 0.0);
	
    color = vec4(brightness * default_color, 1.0f);
}