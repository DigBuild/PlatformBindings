#pragma once
#include "../input/global_input_context.h"

namespace digbuild::platform::desktop
{
	class GlobalInputContext final : public input::GlobalInputContext
	{
	public:
		[[nodiscard]] std::vector<std::shared_ptr<input::Controller>> getControllers() override;
		
		void update() override;

	private:
		void initialize();
		
		std::vector<std::shared_ptr<input::Controller>> m_controllers;
		bool m_initialized = false;
	};
}
