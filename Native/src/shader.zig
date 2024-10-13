const shadercross = @cImport(@cInclude("SDL_gpu_shadercross.h"));

export fn Riateu_InitShaderCross() c_int {
    if (shadercross.SDL_ShaderCross_Init()) {
        return 0;
    }
    return 1;
}

export fn Riateu_DeinitShaderCross() void {
    shadercross.SDL_ShaderCross_Quit();
}

export fn Riateu_CompileSPIRVGraphics(device: ?*shadercross.SDL_GPUDevice, createInfo: [*c]const shadercross.SDL_GPUShaderCreateInfo) ?*shadercross.SDL_GPUShader {
    return shadercross.SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(device, createInfo);
}

export fn Riateu_CompileSPIRVCompute(device: ?*shadercross.SDL_GPUDevice, createInfo: [*c]const shadercross.SDL_GPUComputePipelineCreateInfo) ?*shadercross.SDL_GPUComputePipeline {
    return shadercross.SDL_ShaderCross_CompileComputePipelineFromSPIRV(device, createInfo);
}
