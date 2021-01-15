#pragma once
#include <memory>

namespace digbuild::platform::util
{
	class Destructible
	{
	public:
		Destructible() = default;
		virtual ~Destructible() = default;
		Destructible(Destructible const&) = delete;
		Destructible(Destructible&&) = delete;
		void operator=(Destructible const&) = delete;
		void operator=(Destructible&&) = delete;
	};
	
	template<typename T>
	class SharedPtr final : public Destructible
	{
	public:
		explicit SharedPtr(std::shared_ptr<T> pointer) :
			pointer(pointer)
		{
		}
		const std::shared_ptr<T> pointer;
	};
	
	using native_handle = const void*;

	template<typename T>
	native_handle make_native_handle(std::shared_ptr<T>&& pointer)
	{
		return new SharedPtr<T>(std::move(pointer));
	}

	template<typename T>
	T* handle_cast(native_handle pointer)
	{
		return static_cast<const SharedPtr<T>*>(pointer)->pointer.get();
	}

	template<typename T>
	std::shared_ptr<T> handle_share(native_handle pointer)
	{
		return static_cast<const SharedPtr<T>*>(pointer)->pointer;
	}
}
