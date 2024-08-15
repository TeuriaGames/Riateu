const stb_truetype = @cImport(
    @cInclude("stb_truetype.h")
);

const sdl2 = @cImport(
    @cInclude("SDL2/SDL.h")
);
const log = @import("./log.zig");

const RiateuFont = [*c]stb_truetype.stbtt_fontinfo;

export fn Riateu_FontInit(data: [*c]const u8) RiateuFont {
    if (stb_truetype.stbtt_GetNumberOfFonts(data) <= 0) 
    {
        log.log_error("Number of fonts were below 0.", .{});
        return null;
    }

    const info = sdl2.SDL_malloc(@sizeOf(stb_truetype.stbtt_fontinfo));

    if (info) |inf| {
        const info_strict = @as([*]stb_truetype.stbtt_fontinfo, @alignCast(@ptrCast(inf)));
        if (stb_truetype.stbtt_InitFont(info_strict, data, 0) == 0) {
            log.log_error("Unable to create font.", .{});
            sdl2.SDL_free(info);
            return null;
        }

        return info_strict;
    }

    log.log_error("Unable to allocate more memory.", .{});
    return null;
}

export fn Riateu_FontGetCharacter(
    font: RiateuFont, glyph: c_int, scale: f32,
    width: [*c]c_int, height: [*c]c_int, advance: [*c]f32,
    offsetX: [*c]f32, offsetY: [*c]f32, visible: [*c]c_int
) void {
    var adv: c_int = 0;
    var x0: c_int = 0;
    var y0: c_int = 0;
    var x1: c_int = 0;
    var y1: c_int = 0;
    var offX: c_int = 0;

    stb_truetype.stbtt_GetGlyphHMetrics(font, glyph, &adv, &offX);
    stb_truetype.stbtt_GetGlyphBitmapBox(font, glyph, scale, scale, &x0, &y0, &x1, &y1);

    width.* = (x1 - x0);
    height.* = (y1 - y0);
    advance.* = @as(f32, @floatFromInt(adv)) * scale;
    offsetX.* = @as(f32, @floatFromInt(offX)) * scale;
    offsetY.* = @floatFromInt(y0);
    if (width.* > 0 and height.* > 0 and stb_truetype.stbtt_IsGlyphEmpty(font, glyph) == 0) {
        visible.* = 1;
    } else {
        visible.* = 0;
    }
}

export fn Riateu_FontGetPixels(font: RiateuFont, dest: [*c]u8, glyph: c_int, width: c_int, height: c_int, scale: f32) void {
    stb_truetype.stbtt_MakeGlyphBitmap(font, dest, width, height, width, scale, scale, glyph);

    const len: c_int = width * height;
    var idx: usize = @as(usize, @intCast(len - 1)) * 4;
    var point: usize = @intCast(len - 1);
    while (point >= 0) 
    {
        dest[idx] = dest[point];
        dest[idx + 1] = dest[point];
        dest[idx + 2] = dest[point];
        dest[idx + 3] = dest[point];

        if (point > 0) 
        {
            point -= 1;
            idx -= 4;
            continue;
        }
        break;
    }
}

export fn Riateu_FontGetMetrics(font: RiateuFont, ascent: [*c]c_int, descent: [*c]c_int, line_gap: [*c]c_int) void {
    stb_truetype.stbtt_GetFontVMetrics(font, ascent, descent, line_gap);
}

export fn Riateu_FontFindGlyphIndex(font: RiateuFont, codepoint: c_int) c_int {
    return stb_truetype.stbtt_FindGlyphIndex(font, codepoint);
}

export fn Riateu_FontGetPixelScale(font: RiateuFont, scale: f32) f32 {
    return stb_truetype.stbtt_ScaleForMappingEmToPixels(font, scale);
}

export fn Riateu_FontGetKerning(font: RiateuFont, glyph1: c_int, glyph2: c_int, scale: f32) f32 {
    const kerning = stb_truetype.stbtt_GetGlyphKernAdvance(font, glyph1, glyph2);
    return @as(f32, @floatFromInt(kerning)) * scale;
}

export fn Riateu_FontFree(font: RiateuFont) void {
    sdl2.SDL_free(font);
}

test {
    const std = @import("std");
    const fs = std.fs;

    var arena = std.heap.ArenaAllocator.init(std.heap.c_allocator);
    defer arena.deinit();

    var allocator = arena.allocator();
    var file = try fs.cwd().openFile("PressStart2P-Regular.ttf", .{});
    const size = try file.getEndPos();
    const buffer = try allocator.alloc(u8, size);
    defer allocator.free(buffer);

    _ = try file.readAll(buffer);
    const font = Riateu_FontInit(buffer.ptr);
    defer Riateu_FontFree(font);

    const glyph = Riateu_FontFindGlyphIndex(font, 40);
    const scale = Riateu_FontGetPixelScale(font, 32);
    var width: c_int = 0;
    var height: c_int = 0;
    var advance: f32 = 0;
    var offsetX: f32 = 0;
    var offsetY: f32 = 0;
    var visible: bool = false;
    Riateu_FontGetCharacter(font, glyph, scale, &width, &height, &advance, &offsetX, &offsetY, &visible);

    const bytes = try allocator.alloc(u8, @intCast(width * height));
    defer allocator.free(bytes);

    Riateu_FontGetPixels(font, bytes.ptr, glyph, width, height, scale);
}