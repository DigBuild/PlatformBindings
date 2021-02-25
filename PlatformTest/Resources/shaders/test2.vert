#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
};

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec3 fragNormal;

void main() {
    gl_Position = matrix * vec4(pos.xyz, 1.0);
    fragColor = color;
	fragNormal = normal;
}
