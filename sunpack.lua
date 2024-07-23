Package "Riateu"
    Version "2.0.0"
    Projects {
        "Riateu/Riateu.csproj"
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