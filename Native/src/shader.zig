const std = @import("std");
const shadercross = @cImport(@cInclude("SDL_gpu_shadercross.h"));
const buildOptions = @import("build_options");

export fn Riateu_InitShaderCross() c_int {
    if (!buildOptions.has_shadercross) {
        return 1;
    }
    if (shadercross.SDL_ShaderCross_Init()) {
        return 0;
    }
    return 1;
}

export fn Riateu_DeinitShaderCross() void {
    if (!buildOptions.has_shadercross) {
        return;
    }
    shadercross.SDL_ShaderCross_Quit();
}

export fn Riateu_CompileSPIRVGraphics(device: ?*shadercross.SDL_GPUDevice, createInfo: [*c]const shadercross.SDL_GPUShaderCreateInfo) ?*shadercross.SDL_GPUShader {
    if (!buildOptions.has_shadercross) {
        return null;
    }
    return shadercross.SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(device, createInfo);
}

export fn Riateu_CompileSPIRVCompute(device: ?*shadercross.SDL_GPUDevice, createInfo: [*c]const shadercross.SDL_GPUComputePipelineCreateInfo) ?*shadercross.SDL_GPUComputePipeline {
    if (!buildOptions.has_shadercross) {
        return null;
    }
    return shadercross.SDL_ShaderCross_CompileComputePipelineFromSPIRV(device, createInfo);
}

export fn Riateu_GetShaderFormat() c_uint {
    if (!buildOptions.has_shadercross) {
        return 2;
    }

    return @as(c_uint, shadercross.SDL_ShaderCross_GetSPIRVShaderFormats());
}
