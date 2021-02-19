#pragma once
#include <vector>

namespace digbuild::platform::util
{
	class NativeBuffer final
	{
	public:
		NativeBuffer(const uint32_t initialCapacity)
		{
			reserve(std::max(initialCapacity, 1u));
		}
		~NativeBuffer() = default;
		NativeBuffer(const NativeBuffer& other) = delete;
		NativeBuffer(NativeBuffer&& other) noexcept = delete;
		NativeBuffer& operator=(const NativeBuffer& other) = delete;
		NativeBuffer& operator=(NativeBuffer&& other) noexcept = delete;

		void reserve(const uint32_t minCapacity)
		{
			m_data.resize(minCapacity);
		}

		[[nodiscard]] char* getPtr()
		{
			return m_data.data();
		}
		[[nodiscard]] uint32_t getCapacity() const
		{
			return static_cast<uint32_t>(m_data.size());
		}
	private:
		std::vector<char> m_data;
	};
}
