#version 140

attribute vec3 coords;
attribute vec3 color_seed_in;
varying vec3 color_seed;

uniform mat4 view;

void main(void)
{
    gl_Position =  view * vec4(coords, 1.0);  
    color_seed = color_seed_in;
}  