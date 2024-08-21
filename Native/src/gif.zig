const std = @import("std");
const log = @import("log.zig");

const stb_image = @cImport(
    @cInclude("stb_image.h")
);

export fn Riateu_LoadGif(buffer: [*c]const u8, ln: c_int) [*c]ImageGif {
    const gif_n = ImageGif.init(buffer, @intCast(ln));
    if (gif_n) |gif| {
        return gif;
    }

    return null;
}

export fn Riateu_GetGifSize(gif: [*c]ImageGif, width: [*c]c_int, height: [*c]c_int, len: [*c]c_int) void {
    width.* = @intCast(gif.*.width);
    height.* = @intCast(gif.*.height);
    len.* = @intCast(gif.*.width * gif.*.height * gif.*.comp);
}

export fn Riateu_GetGifFrames(gif: [*c]ImageGif, frames: [*c]c_int) void {
    frames.* = @intCast(gif.*.frames);
}

export fn Riateu_GetGifChannels(gif: [*c]ImageGif, channels: [*c]c_int) void {
    channels.* = @intCast(gif.*.comp);
}

export fn Riateu_CopyGifFrames(gif: [*c]ImageGif, index: c_int) [*c]u8 {
    const w = gif.*.width;
    const h = gif.*.height;
    const comp = gif.*.comp;
    const len: usize = @intCast(w * h * comp);
    const real_index: u32 = @intCast(index);
    const buffer = std.heap.raw_c_allocator.alloc(u8, len) catch {
        log.log_error("Unable to allocate more memory", .{});
        return null;
    };

    @memcpy(buffer, gif.*.buffer[@intCast(w * h * comp * real_index)..len]);

    return buffer.ptr;
}

export fn Riateu_FreeGif(gif: [*c]ImageGif) void {
    gif.*.free();
}

const ImageGif = extern struct {
    buffer: [*]u8,
    len: u32,
    width: u32,
    height: u32,
    frames: u32,
    comp: u32,
    delays: [*]c_int,

    pub fn init(buffer: [*c]const u8, len: u32) ?*@This() {
        var delays: [*c]c_int = null;
        var width: c_int = 0;
        var height: c_int = 0;
        var frames : c_int = 0;
        var comp: c_int = 0;
        const uc = stb_image.stbi_load_gif_from_memory(
            buffer, 
            @intCast(len),
            &delays, 
            &width,
            &height,
            &frames,
            &comp,
            4);
        
        const gif_alloc = std.heap.raw_c_allocator.create(ImageGif) catch {
            log.log_error("Cannot allocate more memory", .{});
            return null;
        };

        gif_alloc.buffer = uc;
        gif_alloc.len = @as(u32, @intCast(width * height)) * 4;
        gif_alloc.delays = delays;
        gif_alloc.width = @intCast(width);
        gif_alloc.height = @intCast(height);
        gif_alloc.frames = @intCast(frames);
        gif_alloc.comp = @intCast(comp);
        
        return gif_alloc;
    }

    pub fn free(self: *@This()) void {
        std.heap.raw_c_allocator.destroy(self);
        std.c.free(self.delays);
    }
};