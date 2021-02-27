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
		
		[[nodiscard]] bool isConnected() const override;

		[[nodiscard]] std::string getGUID() const override
		{
			return m_guid;
		}

		[[nodiscard]] std::vector<bool> getButtonStates() const override;
		[[nodiscard]] std::vector<float> getJoysticks() const override;
		[[nodiscard]] std::vector<std::bitset<4>> getHatStates() const override;

	private:
		uint32_t m_id;
		std::string m_guid;
	};
}
