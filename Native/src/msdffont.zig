const std = @import("std");
const c_font = @import("thirdparty.zig").c_font;

const sdl3 = @cImport(@cInclude("SDL3/SDL.h"));

const log = @import("log.zig");

const f = @import("font.zig");
const RiateuFont = f.RiateuFont;

export fn Riateu_GetMSDFFontGlyphBox(font: RiateuFont, glyphIndex: c_int, x0: [*c]c_int, y0: [*c]c_int, x1: [*c]c_int, y1: [*c]c_int) c_int {
    return c_font.stbtt_GetGlyphBox(font, glyphIndex, x0, y0, x1, y1);
}

export fn Riateu_GetMSDFFontPixels(font: RiateuFont, dest: [*c]u8, glyph: c_int, borderSize: c_uint, size: f32, range: f32) c_int {
    var allocCtx = c_font.msdf_AllocCtx{ .alloc = g_alloc, .free = g_free, .ctx = null };
    const genScale = f.Riateu_GetFontPixelScale(font, size);

    var msdfResult: c_font.msdf_Result = undefined;
    _ = c_font.msdf_genGlyph(&msdfResult, font, glyph, borderSize, genScale, range / size, &allocCtx);

    const scale = size;
    const transWidth = ((size * 0.7) + scale) / (scale * 2.0);
    const transAbs = transWidth - 0.5;
    const transStart = 0.5 - transAbs / 2.0;
    const transEnd = transStart + transAbs;

    const pixels: []u8 = std.mem.span(dest);

    const msdfWidth = @as(usize, @intCast(msdfResult.width));
    const msdfHeight = @as(usize, @intCast(msdfResult.height));

    for (0..msdfHeight) |y| {
        const yPos = msdfWidth * 3 * y;
        const pixelRow = pixels.ptr + (y * msdfWidth * 4);

        for (0..msdfWidth) |x| {
            const indexSdf = yPos + (x * 3);
            var r = msdfResult.rgb[indexSdf + 0];
            var g = msdfResult.rgb[indexSdf + 1];
            var b = msdfResult.rgb[indexSdf + 2];

            r = (r + scale) / (scale * 2.0);
            g = (g + scale) / (scale * 2.0);
            b = (b + scale) / (scale * 2.0);

            if (r > transStart) {
                if (r > transEnd) {
                    r = 1.0;
                } else {
                    r = 0.0 + (r - transStart) / transAbs;
                }
            } else {
                r = 0.0;
            }

            if (g > transStart) {
                if (g > transEnd) {
                    g = 1.0;
                } else {
                    g = 0.0 + (g - transStart) / transAbs;
                }
            } else {
                g = 0.0;
            }

            if (b > transStart) {
                if (b > transEnd) {
                    b = 1.0;
                } else {
                    b = 0.0 + (b - transStart) / transAbs;
                }
            } else {
                b = 0.0;
            }

            pixelRow[x * 4 + 0] = @intFromFloat(r * 255.0);
            pixelRow[x * 4 + 1] = @intFromFloat(g * 255.0);
            pixelRow[x * 4 + 2] = @intFromFloat(b * 255.0);
            pixelRow[x * 4 + 3] = 255;
        }
    }

    sdl3.SDL_free(msdfResult.rgb);
    return 0;
}

export fn Riateu_FreeMSDFFont(msdf: [*c]u8) void {
    sdl3.SDL_free(msdf);
}

fn g_alloc(size: usize, ctx: ?*anyopaque) callconv(.C) ?*anyopaque {
    if (ctx) |_| {}
    return sdl3.SDL_malloc(size);
}

fn g_free(mem: ?*anyopaque, ctx: ?*anyopaque) callconv(.C) void {
    if (ctx) |_| {}
    sdl3.SDL_free(mem);
}

inline fn g_min(a: f32, b: f32) f32 {
    if (a > b) {
        return b;
    }
    return a;
}

inline fn g_max(a: f32, b: f32) f32 {
    if (a > b) {
        return a;
    }
    return b;
}

inline fn g_clamp(val: f32, min: f32, max: f32) f32 {
    return g_min(g_max(val, min), max);
}

inline fn g_median(r: f32, g: f32, b: f32) f32 {
    return g_max(g_min(r, g), g_min(g_max(r, g), b));
}
