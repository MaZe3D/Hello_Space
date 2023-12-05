#version 430 core

out vec4 FragColor;

layout(location = 0) uniform int timestamp;

void main()
{
    FragColor = vec4(1.,0.5,0.,1.) * sin(timestamp/100.);
}