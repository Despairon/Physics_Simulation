#version 450 core

in vec2 texcoord;
in vec3 normal;

in vec3 color;

void main()
{    
    gl_FragColor = vec4(color, 1.0f);
}