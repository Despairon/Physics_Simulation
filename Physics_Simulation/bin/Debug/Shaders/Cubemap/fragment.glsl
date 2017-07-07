#version 140

varying vec3 color_seed;

void main()
{    
    gl_FragColor = vec4(color_seed.x, color_seed.y, color_seed.z, 1);
}