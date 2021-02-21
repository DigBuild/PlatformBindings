#include "render_context.h"

#include <stdexcept>


#include "../util/native_handle.h"
#include "../util/utils.h"

namespace digbuild::platform::render
{
	uint32_t calculateSize(const NumericType numericType)
	{
		switch (numericType) {
		case NumericType::BYTE:
		case NumericType::UBYTE:
			return sizeof(uint8_t);
		case NumericType::SHORT:
		case NumericType::USHORT:
			return sizeof(uint16_t);
		case NumericType::INT:
		case NumericType::UINT:
			return sizeof(uint32_t);
		case NumericType::LONG:
		case NumericType::ULONG:
			return sizeof(uint64_t);
		case NumericType::FLOAT:
			return sizeof(float);
		case NumericType::DOUBLE:
			return sizeof(double);
		case NumericType::FLOAT2:
			return 2 * sizeof(float);
		case NumericType::FLOAT3:
			return 3 * sizeof(float);
		case NumericType::FLOAT4:
			return 4 * sizeof(float);
		case NumericType::FLOAT4X4:
			return 4 * 4 * sizeof(float);
		}
		throw std::runtime_error("Invalid type.");
	}
	
	struct FramebufferAttachmentDescriptorC
	{
		const FramebufferAttachmentType type;
		const TextureFormat format;

		[[nodiscard]] FramebufferAttachmentDescriptor toCpp() const
		{
			return FramebufferAttachmentDescriptor{
				type,
				format
			};
		}
	};
	struct FramebufferRenderStageDescriptorC
	{
		const uint32_t* colorAttachments;
		const uint32_t colorAttachmentCount;
		const uint32_t depthStencilAttachment;
		const uint32_t* dependencies;
		const uint32_t dependencyCount;

		[[nodiscard]] FramebufferRenderStageDescriptor toCpp() const
		{
			return FramebufferRenderStageDescriptor{
				std::vector<uint32_t>(colorAttachments, colorAttachments + colorAttachmentCount),
				depthStencilAttachment,
				std::vector<uint32_t>(dependencies, dependencies + dependencyCount)
			};
		}
	};

	struct ShaderUniformPropertyC
	{
		const NumericType type;

		[[nodiscard]] ShaderUniformProperty toCpp() const
		{
			return ShaderUniformProperty{
				type
			};
		}
	};
	struct ShaderBindingC
	{
		const uint32_t offset;
		const uint32_t propertyCount;

		[[nodiscard]] ShaderBinding toCpp(const ShaderUniformPropertyC* properties) const
		{
			std::vector<ShaderUniformProperty> propertyVector;
			propertyVector.reserve(propertyCount);
			uint32_t size = 0;
			for (auto i = 0u; i < propertyCount; ++i) {
				auto prop = properties[offset + i].toCpp();
				propertyVector.push_back(prop);
				size += calculateSize(prop.type);
			}
			return ShaderBinding{
				size,
				propertyVector
			};
		}
	};

	struct DepthBiasC
	{
		const bool enabled;
		const float constantFactor, clamp, slopeFactor;
		
		[[nodiscard]] DepthBias toCpp() const
		{
			return DepthBias{ enabled, constantFactor, clamp, slopeFactor };
		}
	};
	struct DepthTestC
	{
		const bool enabled;
		const CompareOperation comparison;
		const bool write;

		[[nodiscard]] DepthTest toCpp() const
		{
			return DepthTest{ enabled, comparison, write };
		}
	};
	struct StencilFaceOperationC
	{
		const StencilOperation stencilFailOperation;
		const StencilOperation depthFailOperation;
		const StencilOperation successOperation;
		const CompareOperation compareOperation;
		const uint32_t compareMask;
		const uint32_t writeMask;
		const uint32_t value;

		[[nodiscard]] StencilFaceOperation toCpp() const
		{
			return StencilFaceOperation{
				stencilFailOperation,
				depthFailOperation,
				successOperation,
				compareOperation,
				compareMask,
				writeMask,
				value
			};
		}
	};
	struct StencilTestC
	{
		const bool enabled;
		const StencilFaceOperationC front;
		const StencilFaceOperationC back;

		[[nodiscard]] StencilTest toCpp() const
		{
			return StencilTest{ enabled, front.toCpp(), back.toCpp() };
		}
	};
	struct VertexFormatElementC
	{
		const uint32_t location;
		const NumericType type;
		const uint32_t offset;

		[[nodiscard]] VertexFormatElement toCpp() const
		{
			return VertexFormatElement{ location, type, offset };
		}
	};
	struct BlendOptionsC
	{
		const bool enabled;
		const BlendFactor srcColor, dstColor;
		const BlendOperation colorOperation;
		const BlendFactor srcAlpha, dstAlpha;
		const BlendOperation alphaOperation;
		const ColorComponents components;

		[[nodiscard]] BlendOptions toCpp() const
		{
			return BlendOptions{
				enabled,
				srcColor, dstColor, colorOperation,
				srcAlpha, dstAlpha, alphaOperation,
				components
			};
		}
	};

	uint32_t calculateSize(const std::vector<VertexFormatElement>& elements)
	{
		uint32_t size = 0;
		for (const auto& element : elements)
			size += calculateSize(element.type);
		return size;
	}

	VertexFormatDescriptor createFormatDescriptor(
		const VertexFormatElementC* elements, const uint32_t count,
		const VertexFormatDescriptorRate rate
	)
	{
		std::vector<VertexFormatElement> elementVector;
		elementVector.reserve(count);
		for (auto i = 0u; i < count; ++i)
			elementVector.push_back(elements[i].toCpp());
		
		return VertexFormatDescriptor{
			elementVector,
			calculateSize(elementVector),
			rate
		};
	}
}

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C" {
	DLLEXPORT native_handle dbp_render_context_create_framebuffer(
		RenderContext* instance,
		const native_handle format,
		const uint32_t width,
		const uint32_t height
	)
	{
		return make_native_handle(
			instance->createFramebuffer(
				handle_share<FramebufferFormat>(format),
				width, height
			)
		);
	}
	
	DLLEXPORT native_handle dbp_render_context_create_shader(
		RenderContext* instance,
		const ShaderType type,
		const uint8_t* data, const uint32_t dataLength,
		const ShaderBindingC* bindings, const uint32_t bindingCount,
		const ShaderUniformPropertyC* properties
	)
	{
		std::vector<ShaderBinding> bindingVector;
		bindingVector.reserve(bindingCount);
		for (auto i = 0u; i < bindingCount; ++i)
			bindingVector.push_back(bindings[i].toCpp(properties));
		
		return make_native_handle(
			instance->createShader(
				type,
				std::vector<uint8_t>(data, data + dataLength),
				bindingVector
			)
		);
	}

	DLLEXPORT native_handle dbp_render_context_create_render_pipeline(
		RenderContext* instance,
		const native_handle format,
		const uint32_t renderStage,
		const VertexFormatElementC* vertexFormat,
		const uint32_t vertexFormatLength,
		const VertexFormatElementC* instanceFormat,
		const uint32_t instanceFormatLength,
		const BlendOptionsC* blendOptions,
		const native_handle vertexShader,
		const native_handle fragmentShader,

		const Topology topology,
		const RasterMode rasterMode,
		const bool discardRaster,
		const bool hasLineWidth,
		const float lineWidth,
		const bool hasDepthBias,
		const DepthBiasC depthBias,
		const bool hasDepthTest,
		const DepthTestC depthTest,
		const bool hasStencilTest,
		const StencilTestC stencilTest,
		const bool hasCullingMode,
		const CullingMode cullingMode,
		const bool hasFrontFace,
		const FrontFace frontFace
	)
	{
		auto fmt = handle_share<FramebufferFormat>(format);
		if (!fmt)
			fmt = instance->getSurfaceFormat();
		
		std::vector<BlendOptions> blendOptionVector;
		blendOptionVector.reserve(fmt->getAttachmentCount());
		for (auto i = 0u; i < fmt->getAttachmentCount(); ++i)
			blendOptionVector.push_back(blendOptions[i].toCpp());
		
		return make_native_handle(
			instance->createPipeline(
				fmt,
				renderStage,
				std::vector{
					handle_share<Shader>(vertexShader),
					handle_share<Shader>(fragmentShader)
				},
				createFormatDescriptor(vertexFormat, vertexFormatLength, VertexFormatDescriptorRate::VERTEX),
				createFormatDescriptor(instanceFormat, instanceFormatLength, VertexFormatDescriptorRate::INSTANCE),
				{
					topology, rasterMode, discardRaster,
					hasLineWidth ? std::make_optional(lineWidth) : std::optional<float>{},
					hasDepthBias ? std::make_optional(depthBias.toCpp()) : std::optional<DepthBias>{},
					hasDepthTest ? std::make_optional(depthTest.toCpp()) : std::optional<DepthTest>{},
					hasStencilTest ? std::make_optional(stencilTest.toCpp()) : std::optional<StencilTest>{},
					hasCullingMode ? std::make_optional(cullingMode) : std::optional<CullingMode>{},
					hasFrontFace ? std::make_optional(frontFace) : std::optional<FrontFace>{}
				},
				blendOptionVector
			)
		);
	}

	DLLEXPORT native_handle dbp_render_context_create_vertex_buffer(
		RenderContext* instance,
		const uint8_t* data,
		const uint32_t vertexCount,
		const uint32_t vertexSize,
		const bool writable
	)
	{
		return make_native_handle(
			instance->createVertexBuffer(
				std::vector<uint8_t>(data, data + (vertexCount * vertexSize)),
				vertexSize,
				writable
			)
		);
	}
	
	DLLEXPORT native_handle dbp_render_context_create_command_buffer(RenderContext* instance)
	{
		return make_native_handle(
			instance->createCommandBuffer()
		);
	}
	
	DLLEXPORT void dbp_render_context_enqueue(
		RenderContext* instance,
		const native_handle renderTarget,
		const native_handle commandBuffer
	)
	{
		instance->enqueue(
			handle_share<IRenderTarget>(renderTarget),
			handle_share<CommandBuffer>(commandBuffer)
		);
	}
}
