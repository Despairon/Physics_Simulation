#version 450 core

layout (location = 0) in vec3 _vertex;
layout (location = 1) in vec2 _texcoord;
layout (location = 2) in vec3 _normal;

out vec3 vertex;
out vec2 texcoord;
out vec3 normal;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 transform;

void main(void)
{
	vec4 worldPosition = (transform * vec4(_vertex, 1.0));
    gl_Position =  projection * view * worldPosition;  
	
	vertex   = worldPosition.xyz;
	texcoord = _texcoord;
	normal   = (transform * vec4(_normal, 1.0)).xyz;
}  