"""生成 JHViewer 文件类型图标：同一文档模板，徽章颜色和文字区分 MD / JSON / XML。

运行：python3 generate_icons.py
输出：同目录下的 jhviewer-md.ico、jhviewer-json.ico、jhviewer-xml.ico。
"""

from __future__ import annotations

import io
import struct
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
SIZES = (16, 24, 32, 48, 64, 256)
HERE = Path(__file__).resolve().parent

# 同一版式，只换强调色和徽章文字。
VARIANTS = {
    "md": ((18, 128, 92), "MD", "M"),
    "json": ((224, 106, 18), "JSON", "{}"),
    "xml": ((42, 91, 215), "XML", "</>"),
}


def mix(a: tuple[int, int, int], b: tuple[int, int, int], t: float) -> tuple[int, int, int]:
    return tuple(int(a[i] * (1 - t) + b[i] * t) for i in range(3))  # type: ignore[return-value]


def draw_icon(size: int, accent: tuple[int, int, int], label: str) -> Image.Image:
    # 小尺寸用更高分辨率再缩小，文字仍按目标尺寸选择短标签。
    scale = 4 if size <= 48 else 2 if size <= 64 else 1
    canvas = size * scale
    image = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    def px(value: float) -> int:
        return int(round(value * canvas / 256))

    shadow = (15, 23, 42, 36)
    draw.rounded_rectangle(
        [px(36), px(30), px(232), px(244)],
        radius=px(28),
        fill=shadow,
    )

    page = [px(22), px(14), px(226), px(230)]
    radius = px(26)
    draw.rounded_rectangle(page, radius=radius, fill=(255, 255, 255, 255))

    # 左侧强调色条，三种类型一眼能分开，版式位置固定。
    rail_clip = Image.new("L", (canvas, canvas), 0)
    ImageDraw.Draw(rail_clip).rounded_rectangle(page, radius=radius, fill=255)
    rail = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
    ImageDraw.Draw(rail).rectangle(
        [page[0], page[1], page[0] + px(16), page[3]],
        fill=accent + (255,),
    )
    image.paste(rail, (0, 0), rail_clip)

    outline = ImageDraw.Draw(image)
    outline.rounded_rectangle(
        page,
        radius=radius,
        outline=(214, 219, 227, 255),
        width=max(1, px(3)),
    )

    fold = px(58)
    x1, y0 = page[2], page[1]
    fold_fill = mix((255, 255, 255), accent, 0.28) + (255,)
    fold_layer = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
    ImageDraw.Draw(fold_layer).polygon(
        [(x1 - fold, y0), (x1, y0), (x1, y0 + fold)],
        fill=fold_fill,
    )
    image.paste(fold_layer, (0, 0), rail_clip)
    ImageDraw.Draw(image).line(
        [(x1 - fold, y0 + max(1, px(2))), (x1 - max(1, px(2)), y0 + fold)],
        fill=accent + (210,),
        width=max(1, px(3)),
    )

    line = (203, 210, 220, 255)
    thickness = max(2, px(10))
    for index, width_ratio in enumerate((0.58, 0.44, 0.30)):
        y = px(88 + index * 30)
        x0 = page[0] + px(32)
        x2 = x0 + int((page[2] - x0 - px(18)) * width_ratio)
        ImageDraw.Draw(image).rounded_rectangle(
            [x0, y, x2, y + thickness],
            radius=thickness // 2,
            fill=line,
        )

    badge = [px(46), px(158), px(206), px(214)]
    ImageDraw.Draw(image).rounded_rectangle(badge, radius=px(14), fill=accent + (255,))

    font_size = px(36 if len(label) <= 2 else 30)
    font = ImageFont.truetype(FONT, max(8, font_size))
    text_draw = ImageDraw.Draw(image)
    bbox = text_draw.textbbox((0, 0), label, font=font)
    text_w = bbox[2] - bbox[0]
    text_h = bbox[3] - bbox[1]
    tx = (badge[0] + badge[2] - text_w) / 2 - bbox[0]
    ty = (badge[1] + badge[3] - text_h) / 2 - bbox[1]
    text_draw.text((tx, ty), label, font=font, fill=(255, 255, 255, 255))

    if scale != 1:
        image = image.resize((size, size), Image.Resampling.LANCZOS)
    return image


def save_ico(path: Path, images: list[Image.Image]) -> None:
    # Vista 起的 ICO 可以直接嵌入 PNG，每种尺寸用单独画好的图，而不是从一张图缩小。
    payloads: list[bytes] = []
    for image in images:
        buffer = io.BytesIO()
        image.save(buffer, format="PNG")
        payloads.append(buffer.getvalue())

    header = struct.pack("<HHH", 0, 1, len(images))
    entries = bytearray()
    offset = 6 + 16 * len(images)
    blobs = bytearray()
    for image, payload in zip(images, payloads):
        width = 0 if image.width >= 256 else image.width
        height = 0 if image.height >= 256 else image.height
        entries += struct.pack("<BBBBHHII", width, height, 0, 0, 1, 32, len(payload), offset)
        blobs += payload
        offset += len(payload)
    path.write_bytes(header + entries + blobs)


def assert_ico(path: Path) -> None:
    data = path.read_bytes()
    _reserved, kind, count = struct.unpack_from("<HHH", data, 0)
    if kind != 1 or count != len(SIZES):
        raise SystemExit(f"{path.name}: unexpected ICO header count={count}")
    seen: list[int] = []
    for index in range(count):
        width = data[6 + index * 16]
        seen.append(256 if width == 0 else width)
    if tuple(seen) != SIZES:
        raise SystemExit(f"{path.name}: sizes {seen} != {SIZES}")


def main() -> None:
    for key, (accent, full_label, short_label) in VARIANTS.items():
        images = []
        for size in SIZES:
            label = short_label if size <= 24 else full_label
            images.append(draw_icon(size, accent, label))
        destination = HERE / f"jhviewer-{key}.ico"
        save_ico(destination, images)
        assert_ico(destination)
        print(destination.name, destination.stat().st_size)


if __name__ == "__main__":
    main()
