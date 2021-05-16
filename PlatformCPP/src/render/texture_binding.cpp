#include "texture_binding.h"

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C"
{
	DLLEXPORT void dbp_texture_binding_update(
		const native_handle instance,
		const native_handle sampler,
		const native_handle texture
	)
	{
		handle_cast<TextureBinding>(instance)->update(
			handle_share<TextureSampler>(sampler),
			handle_share<Texture>(texture)
		);
	}
}
