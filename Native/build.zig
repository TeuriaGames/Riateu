const std = @import("std");

// Although this function looks imperative, note that its job is to
// declaratively construct a build graph that will be executed by an external
// runner.
pub fn build(b: *std.Build) void {
    // Standard target options allows the person running `zig build` to choose
    // what target to build for. Here we do not override the defaults, which
    // means any target is allowed, and the default is native. Other options
    // for restricting supported target set are available.
    const target = b.standardTargetOptions(.{});

    // Standard optimization options allow the person running `zig build` to select
    // between Debug, ReleaseSafe, ReleaseFast, and ReleaseSmall. Here we do not
    // set a preferred release mode, allowing the user to decide how to optimize.
    const optimize = b.standardOptimizeOption(.{});

    const lib = b.addSharedLibrary(.{
        .name = "RiateuNative",
        // In this case the main source file is merely a path, however, in more
        // complicated build scripts, this could be a generated file.
        .root_source_file = b.path("src/root.zig"),
        .target = target,
        .optimize = optimize,
        .strip = true,
    });
    const options = b.addOptions();

    const sourceFiles = &[_][]const u8{"./lib/src/zig_includes.c"};
    const sourceFlags = &[_][]const u8{ "-g", "-O3" };

    lib.addCSourceFiles(.{ .files = sourceFiles, .flags = sourceFlags });
    lib.addIncludePath(b.path("lib/include"));
    lib.addIncludePath(b.path("lib/SDL3/include"));
    if (target.result.isMinGW()) {
        lib.addObjectFile(b.path("../runtimes/x64/SDL3.dll"));
        lib.addObjectFile(b.path("../runtimes/x64/spirv-cross-c-shared.dll"));
        lib.addObjectFile(b.path("../runtimes/x64/SDL3_gpu_shadercross.dll"));
        addInstallPath(b, lib, "../../runtimes/x64");
        options.addOption(bool, "has_shadercross", true);
    } else if (target.result.isDarwin()) {
        lib.addObjectFile(b.path("../runtimes/osx/libSDL2-2.0.0.dylib"));
        addInstallPath(b, lib, "../../runtimes/osx");
        options.addOption(bool, "has_shadercross", false);
    } else {
        lib.addObjectFile(b.path("../runtimes/lib64/libSDL3.so"));
        addInstallPath(b, lib, "../../runtimes/lib64");
        options.addOption(bool, "has_shadercross", false);
    }
    lib.linkLibC();
    lib.root_module.addOptions("build_options", options);

    // This declares intent for the library to be installed into the standard
    // location when the user invokes the "install" step (the default step when
    // running `zig build`).
    b.installArtifact(lib);

    // Creates a step for unit testing. This only builds the test executable
    // but does not run it.
    const lib_unit_tests = b.addTest(.{
        .root_source_file = b.path("src/root.zig"),
        .target = target,
        .optimize = optimize,
    });

    lib_unit_tests.addIncludePath(b.path("lib/include"));
    lib_unit_tests.addCSourceFiles(.{ .files = sourceFiles, .flags = sourceFlags });
    lib_unit_tests.linkSystemLibrary("SDL2");
    lib_unit_tests.linkLibC();

    const run_lib_unit_tests = b.addRunArtifact(lib_unit_tests);

    // Similar to creating the run step earlier, this exposes a `test` step to
    // the `zig build --help` menu, providing a way for the user to request
    // running the unit tests.
    const test_step = b.step("test", "Run unit tests");
    test_step.dependOn(&run_lib_unit_tests.step);
}

fn addInstallPath(b: *std.Build, compile: *std.Build.Step.Compile, path: []const u8) void {
    const output_step = b.addInstallArtifact(compile, .{ .dest_dir = .{ .override = .{ .custom = path } } });
    b.getInstallStep().dependOn(&output_step.step);
}
