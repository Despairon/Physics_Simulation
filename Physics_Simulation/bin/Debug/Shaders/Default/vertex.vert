#version 450 core

layout (location = 0) in vec3 _vertex;
layout (location = 1) in vec2 _texcoord;
layout (location = 2) in vec3 _normal;

out vec2 texcoord;
out vec3 normal;

out vec3 color;

uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position =  projection * view * vec4(_vertex, 1.0);  
	
	color = _vertex;
	
	texcoord = _texcoord;
	normal   = _normal;
}  