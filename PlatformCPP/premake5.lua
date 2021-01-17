project "DigBuildPlatformCPP"
    kind "SharedLib"
    language "C++"
    cppdialect "C++17"
    architecture "x64"
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"

    files { "src/**.cpp", "src/**.h" }
    files { "premake5.lua" }

    includedirs {
        "vendor/glfw/include",
        "vendor/glad/include",
        "vendor/vulkan/include",
        os.getenv("VK_SDK_PATH") .. "/include" -- TODO: Change this to something that won't be relative
    }
    libdirs {
        "vendor/glfw/lib/lib-vc2019",
        os.getenv("VK_SDK_PATH") .. "/lib" -- TODO: Change this to something that won't be relative
    }
    links {
        "glfw3",
        "vulkan-1",
    }
    files {
        "vendor/glad/src/**.c"
    }
    
    filter "configurations:Debug"
        links {
            "shaderc_combinedd"
        }
    filter "configurations:Release"
        links {
            "shaderc_combined"
        }

    filter "action:vs2019"
        libdirs {
            "vendor/glfw/lib/lib-vc2019"
        }

    filter "action:vs2017"
        libdirs {
            "vendor/glfw/lib/lib-vc2017"
        }

    filter { "system:windows", "action:gmake or gmake2" }
        libdirs {
            "vendor/glfw/lib/lib-mingw-w64"
        }

    filter "system:macosx"
        libdirs {
            "vendor/glfw/lib/lib-macos"
        }

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"
