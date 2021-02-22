#version 450

layout(binding = 0) uniform UBO {
    mat4 matrix;
};

layout(location = 0) in vec3 pos;
layout(location = 1) in vec4 color;

layout(location = 2) in vec3 offset;
layout(location = 3) in float size;

layout(location = 0) out vec4 fragColor;

void main() {
    gl_Position = matrix * vec4(pos * size, 1.0) + vec4(offset, 0.0);
    fragColor = color;
}
