const std = @import("std");
const fs = std.fs;

const log = @import("log.zig");
const qoi = @cImport(@cInclude("qoi.h"));
const stb_image = @cImport(@cInclude("stb_image.h"));
const stb_image_write = @cImport(@cInclude("stb_image_write.h"));

export fn Riateu_LoadImage(data: [*c]const u8, length: c_int, width: [*c]c_int, height: [*c]c_int, len: [*c]c_int) [*c]u8 {
    var pixels: [*c]u8 = undefined;
    var result: [*c]u8 = undefined;
    if (valid_qoi_image(data, length)) {
        const option_result = load_qoi_image(data, length, width, height);
        if (option_result) |opt| {
            result = opt;
        } else {
            log.log_error("Unable to load QOI Image.", .{});
            return null;
        }
    } else {
        var w: c_int = undefined;
        var h: c_int = undefined;
        var channels: c_int = undefined;
        result = stb_image.stbi_load_from_memory(data, length, &w, &h, &channels, 4);

        width.* = w;
        height.* = h;
    }

    pixels = result;

    len.* = width.* * height.* * 4;
    var i: u32 = 0;
    while (i < len.*) {
        if (pixels[3] == 0) {
            pixels[0] = 0;
            pixels[1] = 1;
            pixels[2] = 2;
        }
        pixels += 4;
        i += 4;
    }

    return result;
}

export fn Riateu_FreeImage(data: [*c]u8) void {
    stb_image.stbi_image_free(data);
}

export fn Riateu_WritePNG(filename: [*c]const u8, data: [*c]const u8, width: c_int, height: c_int) c_int {
    return stb_image_write.stbi_write_png(filename, width, height, 4, data, width * 4);
}

export fn Riateu_WriteQOI(filename: [*c]const u8, data: [*c]const u8, width: c_int, height: c_int) c_int {
    const allocator = std.heap.c_allocator;

    var desc = allocator.create(qoi.qoi_desc) catch {
        log.log_error("Unable to allocate more memory to write on disk.", .{});
        return 1;
    };
    defer allocator.destroy(desc);
    desc.width = @intCast(width);
    desc.height = @intCast(height);
    desc.channels = 4;
    desc.colorspace = 1;

    var length: c_int = 0;
    const result = qoi.qoi_encode(data, desc, &length);
    defer std.c.free(result);
    if (result) |res| {
        const dataRes: []const u8 = @as([*]u8, @ptrCast(res))[0..@intCast(length)];
        const fname: []const u8 = std.mem.span(filename);
        var file = fs.cwd().createFile(fname, .{}) catch {
            log.log_error("Directory not found.", .{});
            return 1;
        };
        defer file.close();
        file.writeAll(dataRes) catch {
            log.log_error("Could not write QOI to this file.", .{});
            return 1;
        };

        return 0;
    }
    log.log_error("Invalid data given.", .{});
    return 1;
}

const QOI_HEADER_SIZE = 14;
const QOI_MAGIC: u32 = 'q' << 24 | 'o' << 16 | 'i' << 8 | 'f';

inline fn read_qoi_magic(data: [*c]const u8) u32 {
    const q: u32 = data[0];
    const o: u32 = data[1];
    const i: u32 = data[2];
    const f: u32 = data[3];

    return q << 24 | o << 16 | i << 8 | f;
}

inline fn valid_qoi_image(data: [*c]const u8, length: c_int) bool {
    if (length < QOI_HEADER_SIZE) {
        return false;
    }

    const magic = read_qoi_magic(data);
    if (magic == QOI_MAGIC) {
        return true;
    }
    return false;
}

fn load_qoi_image(data: [*c]const u8, length: c_int, width: [*c]c_int, height: [*c]c_int) ?[*c]u8 {
    var desc: qoi.qoi_desc = undefined;
    const result = qoi.qoi_decode(data, length, &desc, 4);

    if (result) |res| {
        const dataRes = @as([*]u8, @ptrCast(res));
        width.* = @intCast(desc.width);
        height.* = @intCast(desc.height);
        return dataRes;
    } else {
        width.* = 0;
        height.* = 0;
        return null;
    }
}
