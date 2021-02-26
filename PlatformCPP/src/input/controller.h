#pragma once
#include <bitset>
#include <memory>
#include <vector>

namespace digbuild::platform::input
{
	class Controller : public std::enable_shared_from_this<Controller>
	{
	public:
		Controller() = default;
		virtual ~Controller() = default;
		Controller(const Controller& other) = delete;
		Controller(Controller&& other) noexcept = delete;
		Controller& operator=(const Controller& other) = delete;
		Controller& operator=(Controller&& other) noexcept = delete;

		[[nodiscard]] virtual std::string getGUID() = 0;
		
		[[nodiscard]] virtual std::vector<bool> getButtonStates() = 0;
		[[nodiscard]] virtual std::vector<float> getJoysticks() = 0;
		[[nodiscard]] virtual std::vector<std::bitset<4>> getHatStates() = 0;
	};
}
