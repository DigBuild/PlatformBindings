#pragma once
#include "vk_context.h"
#include "vk_shader.h"
#include "../../render/uniform_binding.h"

namespace digbuild::platform::desktop::vulkan
{
	class UniformBinding final : public render::UniformBinding
	{
	public:
		UniformBinding(
			std::shared_ptr<VulkanContext> context,
			std::shared_ptr<Shader> shader,
			uint32_t binding,
			uint32_t stages,
			const std::shared_ptr<render::UniformBuffer>& uniformBuffer
		);

		~UniformBinding() override;

		void tick();
		
		void update(
			const std::shared_ptr<render::UniformBuffer> uniformBuffer
		) override
		{
			update(uniformBuffer, true);
		}

		[[nodiscard]] vk::DescriptorSet& get()
		{
			return *m_descriptorSets[m_readIndex];
		}

		[[nodiscard]] std::shared_ptr<Shader>& getShader()
		{
			return m_shader;
		}

		[[nodiscard]] uint32_t getBinding() const
		{
			return m_binding;
		}

		[[nodiscard]] uint32_t getBindingSize() const
		{
			return m_bindingSize;
		}

		void updateNext();

	private:
		void update(
			std::shared_ptr<render::UniformBuffer> uniformBuffer,
			bool registerUser
		);
		
		std::shared_ptr<VulkanContext> m_context;
		std::shared_ptr<Shader> m_shader;
		uint32_t m_binding;
		uint32_t m_bindingSize;

		std::vector<std::shared_ptr<UniformBuffer>> m_buffers;

		vk::UniqueDescriptorPool m_descriptorPool;
		std::vector<vk::UniqueDescriptorSet> m_descriptorSets;
		
		uint32_t m_readIndex = 0;
		uint32_t m_leftoverWrites = 0;
	};
}
