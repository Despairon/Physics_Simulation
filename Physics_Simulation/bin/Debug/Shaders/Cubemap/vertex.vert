#version 450 core

in  vec3 coords;
out vec3 color;

uniform mat4 view;

void main(void)
{
    gl_Position =  view * vec4(coords, 1.0);  
}  