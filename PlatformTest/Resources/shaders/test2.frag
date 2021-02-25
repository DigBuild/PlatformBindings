#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec3 fragNormal;

layout(location = 0) out vec4 outColor;

float calculateNormalShade(vec3 normal) {
    return min(0.7 - 0.2 * abs(normal.x) + 0.5 * normal.y, 1.0);
}

void main() {
    float shade = calculateNormalShade(fragNormal);
    outColor = vec4(fragColor.rgb * shade, fragColor.a);
}
