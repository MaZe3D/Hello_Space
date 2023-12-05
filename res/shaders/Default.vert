#version 430 core

layout (location = 0) in vec2 aPosition; // vertex coordinates

void main()
{
    gl_Position = vec4(aPosition, 1., 1.); // coordinates
}