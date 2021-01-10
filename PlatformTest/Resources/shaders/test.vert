#version 450

layout(binding = 0) uniform UBO1 {
	mat4 projectionMatrix;
};
layout(binding = 1) uniform UBO2 {
	mat4 modelViewMatrix;
};

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec3 fragNormal;

void main() {
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos.xyz, 1.0);
    fragColor = color;
	fragNormal = normal;
}
