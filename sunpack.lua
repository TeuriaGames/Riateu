Package "Riateu"
    Version "2.0.0"
    Projects {
        "Riateu/Riateu.Vulkan.csproj",
        "Riateu/Riateu.D3D11.csproj",
        "Riateu/Riateu.Metal.csproj"
    }
    Dependencies {
        MoonWorks = {
            Repository "https://github.com/MoonsideGames",
            Branch "refresh2",
            Project "MoonWorks.csproj"
        }
    }
    ResolvePackages {
        MoonWorks = "MoonWorks/MoonWorks.sunpack.lua"
    }