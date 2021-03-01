project "DigBuild.Platform.Test"
    kind "ConsoleApp"
    framework "net5.0"
    language "C#"
    csversion "9.0"
    enabledefaultcompileitems(true)
    allownullable(true)
    noframeworktag(true)
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"
    -- resourcesdir "Resources"

    dependson {
		"DigBuild.Platform",
		"DigBuild.Platform.SourceGen"
	}
    links {
		"DigBuild.Platform"
	}
	analyzer {
		"DigBuild.Platform.SourceGen"
	}

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"
