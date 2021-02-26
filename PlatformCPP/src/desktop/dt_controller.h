#pragma once
#include "../input/controller.h"

namespace digbuild::platform::desktop
{
	class Controller final : public input::Controller
	{
	public:
		Controller(const uint32_t id, std::string guid)
			: m_id(id),
			  m_guid(std::move(guid))
		{
		}

		[[nodiscard]] std::string getGUID() override
		{
			return m_guid;
		}

		[[nodiscard]] std::vector<bool> getButtonStates() override;
		[[nodiscard]] std::vector<float> getJoysticks() override;
		[[nodiscard]] std::vector<std::bitset<4>> getHatStates() override;

	private:
		uint32_t m_id;
		std::string m_guid;
	};
}
