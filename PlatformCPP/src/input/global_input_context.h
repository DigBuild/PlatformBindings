#pragma once
#include <memory>

#include "controller.h"

namespace digbuild::platform::input
{
	class GlobalInputContext
	{
	public:
		GlobalInputContext() = default;
		virtual ~GlobalInputContext() = default;
		GlobalInputContext(const GlobalInputContext& other) = delete;
		GlobalInputContext(GlobalInputContext&& other) noexcept = delete;
		GlobalInputContext& operator=(const GlobalInputContext& other) = delete;
		GlobalInputContext& operator=(GlobalInputContext&& other) noexcept = delete;
		
		[[nodiscard]] virtual std::vector<std::shared_ptr<Controller>> getControllers() = 0;
		
		virtual void update() = 0;
	};
}
