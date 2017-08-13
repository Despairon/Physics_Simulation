#version 450 core

in vec3 vertex;
in vec2 texcoord;
in vec3 normal;
in vec3 toLightVector;

out vec4 color;

const vec3 default_color          = vec3(1,1,1);
const vec3 default_light_position = vec3(0,0,0);

void main()
{  
	vec3 unitNormal = normalize(normal);
	
	vec3 unitLightVector = normalize(toLightVector);
	
	float brightness = max(dot(unitNormal, unitLightVector), 0.0);
	
    color = vec4(brightness * default_color, 1.0f);
}