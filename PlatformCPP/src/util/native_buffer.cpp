#include "native_buffer.h"

#include "native_handle.h"
#include "utils.h"

using namespace digbuild::platform::util;
extern "C" {
	DLLEXPORT native_handle dbp_native_buffer_create(
		const uint32_t initialCapacity,
		void*& ptr, uint32_t& capacity
	)
	{
		auto buffer = std::make_shared<NativeBuffer>(initialCapacity);
		ptr = buffer->getPtr();
		capacity = buffer->getCapacity();
		return make_native_handle(std::move(buffer));
	}
	DLLEXPORT void dbp_native_buffer_reserve(
		const native_handle instance,
		const uint32_t minCapacity,
		void*& ptr, uint32_t& capacity
	)
	{
		const auto& buffer = handle_cast<NativeBuffer>(instance);
		buffer->reserve(minCapacity);
		ptr = buffer->getPtr();
		capacity = buffer->getCapacity();
	}
}

