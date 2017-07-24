#version 140 core
/*
in vec2 texcoord;
in vec3 normal;
in vec3 color;
*/

in vec4 pos;

void main()
{    
    gl_FragColor = vec4(pos.x, pos.y, pos.z, 1.0f);
}