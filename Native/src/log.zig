const sdl2 = @cImport(
    @cInclude("SDL2/SDL.h")
);

pub fn log_error(fmt: []const u8, args: anytype) void {
    sdl2.SDL_LogError(sdl2.SDL_LOG_CATEGORY_ERROR, fmt.ptr, args); 
}