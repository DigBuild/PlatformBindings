#pragma once
#include <memory>

namespace digbuild::platform::desktop::vulkan
{
	class VulkanContext final : public std::enable_shared_from_this<VulkanContext>
	{
	public:
		VulkanContext() = default;
		~VulkanContext() = default;
		VulkanContext(const VulkanContext& other) = delete;
		VulkanContext(VulkanContext&& other) noexcept = delete;
		VulkanContext& operator=(const VulkanContext& other) = delete;
		VulkanContext& operator=(VulkanContext&& other) noexcept = delete;
	};
}
