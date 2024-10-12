const sdl3 = @cImport(@cInclude("SDL3/SDL.h"));

pub fn log_error(fmt: []const u8, args: anytype) void {
    sdl3.SDL_LogError(sdl3.SDL_LOG_CATEGORY_ERROR, fmt.ptr, args);
}
