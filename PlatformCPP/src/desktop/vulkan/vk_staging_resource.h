#pragma once
#include <vector>
#include <vulkan.h>

namespace digbuild::platform::desktop::vulkan::util
{
	class ScalableStagingResource
	{
	public:
		ScalableStagingResource() = default;
		virtual ~ScalableStagingResource() = default;
		
		virtual void reserve(uint32_t stages) = 0;
	};
	
	template<typename T>
	class StagingResource final
	{
		using Handle = vk::UniqueHandle<T, VULKAN_HPP_DEFAULT_DISPATCHER_TYPE>;
	public:
		StagingResource() = default;
		explicit StagingResource(std::vector<Handle>&& handles) :
			m_handles(std::move(handles))
		{
		}
		~StagingResource() noexcept = default;
		StagingResource(const StagingResource& other) = delete;
		StagingResource(StagingResource&& other) noexcept :
			m_handles(std::move(other.m_handles))
		{
		}
		StagingResource& operator=(const StagingResource& other) = delete;
		StagingResource& operator=(StagingResource&& other) noexcept
		{
			m_handles = std::move(other.m_handles);
			return *this;
		}

		size_t size() const { return m_handles.size(); }
		T& operator[](size_t i) { return *m_handles[i]; }
		const T& operator[](size_t i) const { return *m_handles[i]; }
	private:
		std::vector<Handle> m_handles;
	};
}
