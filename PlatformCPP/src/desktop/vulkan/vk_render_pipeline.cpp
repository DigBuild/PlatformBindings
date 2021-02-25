#include "vk_render_pipeline.h"

namespace digbuild::platform::desktop::vulkan
{
	const uint32_t BINDING_VERTEX = 0;
	const uint32_t BINDING_INSTANCE = 1;

	const render::CullingMode DEFAULT_CULLING_MODE = render::CullingMode::BACK;
	const render::FrontFace DEFAULT_FRONT_FACE = render::FrontFace::CLOCKWISE;
	const render::DepthBias DEFAULT_DEPTH_BIAS = render::DepthBias{ false, 0.0f, 0.0f, 0.0f };
	const float DEFAULT_LINE_WIDTH = 1.0f;
	const render::DepthTest DEFAULT_DEPTH_TEST = render::DepthTest{ false, render::CompareOperation::NEVER, false };
	const render::StencilTest DEFAULT_STENCIL_TEST = render::StencilTest{ false, {}, {} };
	
	vk::Format toVulkan(const render::NumericType type)
	{
		switch (type)
		{
		case render::NumericType::BYTE:
			return vk::Format::eR8Sint;
		case render::NumericType::UBYTE:
			return vk::Format::eR8Uint;
		case render::NumericType::SHORT:
			return vk::Format::eR16Sint;
		case render::NumericType::USHORT:
			return vk::Format::eR16Uint;
		case render::NumericType::INT:
			return vk::Format::eR32Sint;
		case render::NumericType::UINT:
			return vk::Format::eR32Uint;
		case render::NumericType::LONG:
			return vk::Format::eR64Sint;
		case render::NumericType::ULONG:
			return vk::Format::eR64Uint;
		case render::NumericType::FLOAT:
			return vk::Format::eR32Sfloat;
		case render::NumericType::DOUBLE:
			return vk::Format::eR64Sfloat;
		case render::NumericType::FLOAT2:
			return vk::Format::eR32G32Sfloat;
		case render::NumericType::FLOAT3:
			return vk::Format::eR32G32B32Sfloat;
		case render::NumericType::FLOAT4:
			return vk::Format::eR32G32B32A32Sfloat;
		default:
			throw std::runtime_error("Unsupported type.");
		}
	}
	
	std::vector<vk::VertexInputAttributeDescription> toVulkan(
		const render::VertexFormatDescriptor& format,
		const uint32_t binding,
		const uint32_t locationOffset
	)
	{
		std::vector<vk::VertexInputAttributeDescription> descriptions;
		descriptions.reserve(format.elements.size());
		for (const auto& element : format.elements)
		{
			vk::VertexInputAttributeDescription description {
				element.location + locationOffset,
				binding,
				toVulkan(element.type),
				static_cast<uint32_t>(element.offset)
			};
			descriptions.push_back(description);
		}
		return descriptions;
	}

	vk::PrimitiveTopology toVulkan(const render::Topology topology)
	{
		switch (topology)
		{
		case render::Topology::POINTS:
			return vk::PrimitiveTopology::ePointList;
		case render::Topology::LINES:
			return vk::PrimitiveTopology::eLineList;
		case render::Topology::LINE_STRIPS:
			return vk::PrimitiveTopology::eLineStrip;
		case render::Topology::TRIANGLES:
			return vk::PrimitiveTopology::eTriangleList;
		case render::Topology::TRIANGLE_STRIPS:
			return vk::PrimitiveTopology::eTriangleStrip;
		case render::Topology::TRIANGLE_FANS:
			return vk::PrimitiveTopology::eTriangleFan;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::PolygonMode toVulkan(const render::RasterMode rasterMode)
	{
		switch (rasterMode)
		{
		case render::RasterMode::FILL:
			return vk::PolygonMode::eFill;
		case render::RasterMode::LINE:
			return vk::PolygonMode::eLine;
		case render::RasterMode::POINT:
			return vk::PolygonMode::ePoint;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::CullModeFlags toVulkan(const render::CullingMode cullingMode)
	{
		switch (cullingMode)
		{
		case render::CullingMode::FRONT:
			return vk::CullModeFlagBits::eFront;
		case render::CullingMode::BACK:
			return vk::CullModeFlagBits::eBack;
		case render::CullingMode::FRONT_AND_BACK:
			return vk::CullModeFlagBits::eFrontAndBack;
		case render::CullingMode::NONE:
			return vk::CullModeFlagBits::eNone;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::FrontFace toVulkan(const render::FrontFace frontFace)
	{
		switch (frontFace)
		{
		case render::FrontFace::CLOCKWISE:
			return vk::FrontFace::eClockwise;
		case render::FrontFace::COUNTER_CLOCKWISE:
			return vk::FrontFace::eCounterClockwise;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::CompareOp toVulkan(const render::CompareOperation operation)
	{
		switch (operation)
		{
		case render::CompareOperation::NEVER:
			return vk::CompareOp::eNever;
		case render::CompareOperation::LESS:
			return vk::CompareOp::eLess;
		case render::CompareOperation::LESS_OR_EQUAL:
			return vk::CompareOp::eLessOrEqual;
		case render::CompareOperation::EQUAL:
			return vk::CompareOp::eEqual;
		case render::CompareOperation::NOT_EQUAL:
			return vk::CompareOp::eNotEqual;
		case render::CompareOperation::GREATER_OR_EQUAL:
			return vk::CompareOp::eGreaterOrEqual;
		case render::CompareOperation::GREATER:
			return vk::CompareOp::eGreater;
		case render::CompareOperation::ALWAYS:
			return vk::CompareOp::eAlways;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::StencilOp toVulkan(const render::StencilOperation operation)
	{
		switch (operation)
		{
		case render::StencilOperation::ZERO:
			return vk::StencilOp::eZero;
		case render::StencilOperation::KEEP:
			return vk::StencilOp::eKeep;
		case render::StencilOperation::REPLACE:
			return vk::StencilOp::eReplace;
		case render::StencilOperation::INVERT:
			return vk::StencilOp::eInvert;
		case render::StencilOperation::INCREMENT_AND_CLAMP:
			return vk::StencilOp::eIncrementAndClamp;
		case render::StencilOperation::DECREMENT_AND_CLAMP:
			return vk::StencilOp::eDecrementAndClamp;
		case render::StencilOperation::INCREMENT_AND_WRAP:
			return vk::StencilOp::eIncrementAndWrap;
		case render::StencilOperation::DECREMENT_AND_WRAP:
			return vk::StencilOp::eDecrementAndWrap;
		}
		throw std::runtime_error("Invalid type.");
	}
	
	vk::StencilOpState toVulkan(const render::StencilFaceOperation operation)
	{
		return vk::StencilOpState{
			toVulkan(operation.stencilFailOperation),
			toVulkan(operation.successOperation),
			toVulkan(operation.depthFailOperation),
			toVulkan(operation.compareOperation),
			operation.compareMask,
			operation.writeMask,
			operation.value
		};
	}

	vk::BlendFactor toVulkan(const render::BlendFactor factor)
	{
		switch (factor)
		{
		case render::BlendFactor::ZERO:
			return vk::BlendFactor::eZero;
		case render::BlendFactor::ONE:
			return vk::BlendFactor::eOne;
		case render::BlendFactor::SRC_COLOR:
			return vk::BlendFactor::eSrcColor;
		case render::BlendFactor::ONE_MINUS_SRC_COLOR:
			return vk::BlendFactor::eOneMinusSrcColor;
		case render::BlendFactor::SRC_ALPHA:
			return vk::BlendFactor::eSrcAlpha;
		case render::BlendFactor::ONE_MINUS_SRC_ALPHA:
			return vk::BlendFactor::eOneMinusSrcAlpha;
		case render::BlendFactor::DST_COLOR:
			return vk::BlendFactor::eDstColor;
		case render::BlendFactor::ONE_MINUS_DST_COLOR:
			return vk::BlendFactor::eOneMinusDstColor;
		case render::BlendFactor::DST_ALPHA:
			return vk::BlendFactor::eDstAlpha;
		case render::BlendFactor::ONE_MINUS_DST_ALPHA:
			return vk::BlendFactor::eOneMinusDstAlpha;
		case render::BlendFactor::CST_COLOR:
			return vk::BlendFactor::eConstantColor;
		case render::BlendFactor::ONE_MINUS_CST_COLOR:
			return vk::BlendFactor::eOneMinusConstantColor;
		case render::BlendFactor::CST_ALPHA:
			return vk::BlendFactor::eConstantAlpha;
		case render::BlendFactor::ONE_MINUS_CST_ALPHA:
			return vk::BlendFactor::eOneMinusConstantAlpha;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::BlendOp toVulkan(const render::BlendOperation operation)
	{
		switch (operation)
		{
		case render::BlendOperation::ADD:
			return vk::BlendOp::eAdd;
		case render::BlendOperation::SUBTRACT:
			return vk::BlendOp::eSubtract;
		case render::BlendOperation::REVERSE_SUBTRACT:
			return vk::BlendOp::eReverseSubtract;
		case render::BlendOperation::MIN:
			return vk::BlendOp::eMin;
		case render::BlendOperation::MAX:
			return vk::BlendOp::eMax;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::ColorComponentFlags toVulkan(const render::ColorComponents components)
	{
		vk::ColorComponentFlags flags = {};
		if (static_cast<uint8_t>(components & render::ColorComponents::RED))
			flags |= vk::ColorComponentFlagBits::eR;
		if (static_cast<uint8_t>(components & render::ColorComponents::GREEN))
			flags |= vk::ColorComponentFlagBits::eG;
		if (static_cast<uint8_t>(components & render::ColorComponents::BLUE))
			flags |= vk::ColorComponentFlagBits::eB;
		if (static_cast<uint8_t>(components & render::ColorComponents::ALPHA))
			flags |= vk::ColorComponentFlagBits::eA;
		return flags;
	}

	vk::PipelineColorBlendAttachmentState toVulkan(const render::BlendOptions options)
	{
		return vk::PipelineColorBlendAttachmentState{
			options.enabled,
			toVulkan(options.srcColor), toVulkan(options.dstColor), toVulkan(options.colorOperation),
			toVulkan(options.srcAlpha), toVulkan(options.dstAlpha), toVulkan(options.alphaOperation),
			toVulkan(options.components)
		};
	}

	std::vector<vk::DynamicState> toVulkan(const render::RenderState state)
	{
		std::vector<vk::DynamicState> dynamicStates;
		if (!state.depthBias.has_value())
			dynamicStates.push_back(vk::DynamicState::eDepthBias);
		if (!state.cullingMode.has_value())
			dynamicStates.push_back(vk::DynamicState::eCullModeEXT);
		if (!state.frontFace.has_value())
			dynamicStates.push_back(vk::DynamicState::eFrontFaceEXT);
		if (!state.lineWidth.has_value())
			dynamicStates.push_back(vk::DynamicState::eLineWidth);
		if (!state.depthTest.has_value())
			dynamicStates.insert(dynamicStates.end(), {
				vk::DynamicState::eDepthTestEnableEXT,
				vk::DynamicState::eDepthCompareOpEXT,
				vk::DynamicState::eDepthWriteEnableEXT
			});
		if (!state.stencilTest.has_value())
			dynamicStates.insert(dynamicStates.end(), {
				vk::DynamicState::eStencilTestEnableEXT,
				vk::DynamicState::eStencilOpEXT,
				vk::DynamicState::eStencilCompareMask,
				vk::DynamicState::eStencilReference,
				vk::DynamicState::eStencilWriteMask
			});
		return dynamicStates;
	}
	
	RenderPipeline::RenderPipeline(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<FramebufferFormat> format,
		const uint32_t stage,
		std::vector<std::shared_ptr<Shader>> shaders,
		const render::VertexFormatDescriptor& vertexFormat,
		const render::VertexFormatDescriptor& instanceFormat,
		const render::RenderState state,
		const std::vector<render::BlendOptions>& blendOptions
	) :
		m_context(std::move(context)),
		m_format(std::move(format)),
		m_shaders(std::move(shaders))
	{
		std::vector<vk::VertexInputBindingDescription> vertexBindings;
		std::vector<vk::VertexInputAttributeDescription> vertexAttributes;
		
		vertexBindings.emplace_back(BINDING_VERTEX, vertexFormat.size, vk::VertexInputRate::eVertex);
		auto perVertexAttributes = toVulkan(vertexFormat, BINDING_VERTEX, 0);
		vertexAttributes.insert(vertexAttributes.end(), perVertexAttributes.begin(), perVertexAttributes.end());
		
		if (instanceFormat.size > 0)
		{
			vertexBindings.emplace_back(BINDING_INSTANCE, instanceFormat.size, vk::VertexInputRate::eInstance);
			auto perInstanceAttributes = toVulkan(instanceFormat, BINDING_INSTANCE, static_cast<uint32_t>(vertexFormat.elements.size()));
			vertexAttributes.insert(vertexAttributes.end(), perInstanceAttributes.begin(), perInstanceAttributes.end());
		}

		vk::PipelineVertexInputStateCreateInfo vertexInputStateCreateInfo{ {}, vertexBindings, vertexAttributes };
		vk::PipelineInputAssemblyStateCreateInfo inputAssemblyStateCreateInfo{ {}, toVulkan(state.topology), false };

		const auto depthBias = state.depthBias.value_or(DEFAULT_DEPTH_BIAS);
		vk::PipelineRasterizationStateCreateInfo rasterizationStateCreateInfo{
			{}, false, state.discardRaster,
			toVulkan(state.rasterMode),
			toVulkan(state.cullingMode.value_or(DEFAULT_CULLING_MODE)),
			toVulkan(state.frontFace.value_or(DEFAULT_FRONT_FACE)),
			depthBias.enabled, depthBias.constantFactor, depthBias.clamp, depthBias.slopeFactor,
			state.lineWidth.value_or(DEFAULT_LINE_WIDTH)
		};

		vk::PipelineTessellationStateCreateInfo tessellationStateCreateInfo{};
		vk::Viewport viewport{ 0, 0, 0, 0, 0, 1 };
		vk::Rect2D scissor{ { 0, 0 }, { 0, 0 } };
		vk::PipelineViewportStateCreateInfo viewportStateCreateInfo{
			{},
			1, &viewport,
			1, &scissor
		};
		vk::PipelineMultisampleStateCreateInfo multisampleStateCreateInfo{
			{}, vk::SampleCountFlagBits::e1,
			false, 1.0f,
			nullptr, false, false
		};

		const auto depthTest = state.depthTest.value_or(DEFAULT_DEPTH_TEST);
		const auto stencilTest = state.stencilTest.value_or(DEFAULT_STENCIL_TEST);
		vk::PipelineDepthStencilStateCreateInfo depthStencilStateCreateInfo{
			{},
			depthTest.enabled, depthTest.write, toVulkan(depthTest.comparison), false,
			stencilTest.enabled, toVulkan(stencilTest.front), toVulkan(stencilTest.back),
			0.0f, 1.0f
		};

		std::vector<vk::PipelineColorBlendAttachmentState> blendAttachments;
		for (auto i = 0u; i < m_format->getAttachmentCount(); ++i)
		{
			if (m_format->getAttachments()[i].type == render::FramebufferAttachmentType::COLOR)
				blendAttachments.push_back(toVulkan(blendOptions[i]));
		}
		vk::PipelineColorBlendStateCreateInfo colorBlendStateCreateInfo{
			{}, false, vk::LogicOp::eCopy,
			blendAttachments,
			{ 0.0f, 0.0f, 0.0f, 0.0f }
		};

		auto dynamicStates = std::vector(toVulkan(state));
		dynamicStates.insert(dynamicStates.end(), {
			vk::DynamicState::eViewport,
			vk::DynamicState::eScissor
		});
		vk::PipelineDynamicStateCreateInfo dynamicStateCreateInfo{ {}, dynamicStates };
		
		std::vector<vk::DescriptorSetLayout> descriptorSetLayouts;
		uint32_t descriptorOffset = 0;
		for (const auto& shader : m_shaders)
		{
			auto& layouts = shader->getDescriptorSetLayouts();
			descriptorSetLayouts.insert(descriptorSetLayouts.end(), layouts.begin(), layouts.end());
			
			m_shaderLayoutOffsets.emplace(shader.get(), descriptorOffset);
			descriptorOffset += static_cast<uint32_t>(layouts.size());
		}
		m_layout = m_context->m_device->createPipelineLayoutUnique({ {}, descriptorSetLayouts });

		// TODO: Other layout / uniform stuff

		std::vector<vk::PipelineShaderStageCreateInfo> pipelineShaderStageCreateInfos;
		for (const auto& shader : m_shaders)
			pipelineShaderStageCreateInfos.push_back({ {}, shader->getStage(), shader->getModule(), "main", {} });

		m_pipeline = m_context->m_device->createGraphicsPipelineUnique(*m_context->m_pipelineCache, {
			{},
			pipelineShaderStageCreateInfos,
			&vertexInputStateCreateInfo,
			&inputAssemblyStateCreateInfo,
			&tessellationStateCreateInfo,
			&viewportStateCreateInfo,
			&rasterizationStateCreateInfo,
			&multisampleStateCreateInfo,
			&depthStencilStateCreateInfo,
			&colorBlendStateCreateInfo,
			&dynamicStateCreateInfo,
			m_layout.get(),
			m_format->getPass(),
			stage,
			nullptr,
			-1
		}).value;
	}
}
