#include "native_handle.h"

#include "utils.h"

extern "C"
{
	DLLEXPORT void dbp_native_handle_destroy(digbuild::platform::Destructible* handle)
	{
		delete handle;
	}
}
