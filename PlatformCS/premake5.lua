project "DigBuild.Platform"
    kind "SharedLib"
    framework "net5.0"
    language "C#"
    csversion "9.0"
    enabledefaultcompileitems(true)
    allownullable(true)
    noframeworktag(true)
    clr "Unsafe"
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"

    dependson { "DigBuild.Platform.Native" }

    nuget {
        "AdvancedDLSupport:3.2.0",
        "System.Drawing.Common:5.0.1",
		"OpenAL.NETCore:1.0.3",
        "NVorbis:0.10.3"
    }
	
    filter "system:windows"
		copylib { "Lib/OpenAL/Windows/OpenAL.dll:OpenAL.dll" }
    filter "system:linux"
		copylib { "Lib/OpenAL/Linux/libopenal.so:libopenal.so" }
    filter "system:macosx"
		copylib { "Lib/OpenAL/MacOSX/libopenal.dylib:libopenal.dylib" }

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"
