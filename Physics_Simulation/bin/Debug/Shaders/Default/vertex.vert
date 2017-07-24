#version 140 core

in vec3 _vertex;
in vec2 _texcoord;
in vec3 _normal;
in vec3 _color;
/*
out vec2 texcoord;
out vec3 normal;
out vec3 color;
*/
out vec4 pos;

uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position =  projection * view * vec4(_vertex, 1.0);  
	
	pos = gl_Position;
	
	/*texcoord = _texcoord;
	normal   = _normal;
	color    = _color;*/
}  