namespace TweaksLauncher;

public struct LogColor(byte red, byte green, byte blue)
{
    public byte R { get; set; } = red;
    public byte G { get; set; } = green;
    public byte B { get; set; } = blue;

    /// <summary>#66CDAA</summary>
    public static LogColor MediumAquamarine => new(0x66, 0xCD, 0xAA);

    /// <summary>#0000CD</summary>
    public static LogColor MediumBlue => new(0x00, 0x00, 0xCD);

    /// <summary>#BA55D3</summary>
    public static LogColor MediumOrchid => new(0xBA, 0x55, 0xD3);

    /// <summary>#9370DB</summary>
    public static LogColor MediumPurple => new(0x93, 0x70, 0xDB);

    /// <summary>#3CB371</summary>
    public static LogColor MediumSeaGreen => new(0x3C, 0xB3, 0x71);

    /// <summary>#7B68EE</summary>
    public static LogColor MediumSlateBlue => new(0x7B, 0x68, 0xEE);

    /// <summary>#00FA9A</summary>
    public static LogColor MediumSpringGreen => new(0x00, 0xFA, 0x9A);

    /// <summary>#48D1CC</summary>
    public static LogColor MediumTurquoise => new(0x48, 0xD1, 0xCC);

    /// <summary>#C71585</summary>
    public static LogColor MediumVioletRed => new(0xC7, 0x15, 0x85);

    /// <summary>#191970</summary>
    public static LogColor MidnightBlue => new(0x19, 0x19, 0x70);

    /// <summary>#F5FFFA</summary>
    public static LogColor MintCream => new(0xF5, 0xFF, 0xFA);

    /// <summary>#FFE4E1</summary>
    public static LogColor MistyRose => new(0xFF, 0xE4, 0xE1);

    /// <summary>#FFE4B5</summary>
    public static LogColor Moccasin => new(0xFF, 0xE4, 0xB5);

    /// <summary>#FFDEAD</summary>
    public static LogColor NavajoWhite => new(0xFF, 0xDE, 0xAD);

    /// <summary>#000080</summary>
    public static LogColor Navy => new(0x00, 0x00, 0x80);

    /// <summary>#FDF5E6</summary>
    public static LogColor OldLace => new(0xFD, 0xF5, 0xE6);

    /// <summary>#808000</summary>
    public static LogColor Olive => new(0x80, 0x80, 0x00);

    /// <summary>#800000</summary>
    public static LogColor Maroon => new(0x80, 0x00, 0x00);

    /// <summary>#6B8E23</summary>
    public static LogColor OliveDrab => new(0x6B, 0x8E, 0x23);

    /// <summary>#FF00FF</summary>
    public static LogColor Magenta => new(0xFF, 0x00, 0xFF);

    /// <summary>#32CD32</summary>
    public static LogColor LimeGreen => new(0x32, 0xCD, 0x32);

    /// <summary>#FFF0F5</summary>
    public static LogColor LavenderBlush => new(0xFF, 0xF0, 0xF5);

    /// <summary>#7CFC00</summary>
    public static LogColor LawnGreen => new(0x7C, 0xFC, 0x00);

    /// <summary>#FFFACD</summary>
    public static LogColor LemonChiffon => new(0xFF, 0xFA, 0xCD);

    /// <summary>#ADD8E6</summary>
    public static LogColor LightBlue => new(0xAD, 0xD8, 0xE6);

    /// <summary>#F08080</summary>
    public static LogColor LightCoral => new(0xF0, 0x80, 0x80);

    /// <summary>#E0FFFF</summary>
    public static LogColor LightCyan => new(0xE0, 0xFF, 0xFF);

    /// <summary>#FAFAD2</summary>
    public static LogColor LightGoldenrodYellow => new(0xFA, 0xFA, 0xD2);

    /// <summary>#D3D3D3</summary>
    public static LogColor LightGray => new(0xD3, 0xD3, 0xD3);

    /// <summary>#90EE90</summary>
    public static LogColor LightGreen => new(0x90, 0xEE, 0x90);

    /// <summary>#FFB6C1</summary>
    public static LogColor LightPink => new(0xFF, 0xB6, 0xC1);

    /// <summary>#FFA07A</summary>
    public static LogColor LightSalmon => new(0xFF, 0xA0, 0x7A);

    /// <summary>#20B2AA</summary>
    public static LogColor LightSeaGreen => new(0x20, 0xB2, 0xAA);

    /// <summary>#87CEFA</summary>
    public static LogColor LightSkyBlue => new(0x87, 0xCE, 0xFA);

    /// <summary>#778899</summary>
    public static LogColor LightSlateGray => new(0x77, 0x88, 0x99);

    /// <summary>#B0C4DE</summary>
    public static LogColor LightSteelBlue => new(0xB0, 0xC4, 0xDE);

    /// <summary>#FFFFE0</summary>
    public static LogColor LightYellow => new(0xFF, 0xFF, 0xE0);

    /// <summary>#00FF00</summary>
    public static LogColor Lime => new(0x00, 0xFF, 0x00);

    /// <summary>#FAF0E6</summary>
    public static LogColor Linen => new(0xFA, 0xF0, 0xE6);

    /// <summary>#FFFF00</summary>
    public static LogColor Yellow => new(0xFF, 0xFF, 0x00);

    /// <summary>#FFA500</summary>
    public static LogColor Orange => new(0xFF, 0xA5, 0x00);

    /// <summary>#DA70D6</summary>
    public static LogColor Orchid => new(0xDA, 0x70, 0xD6);

    /// <summary>#C0C0C0</summary>
    public static LogColor Silver => new(0xC0, 0xC0, 0xC0);

    /// <summary>#87CEEB</summary>
    public static LogColor SkyBlue => new(0x87, 0xCE, 0xEB);

    /// <summary>#6A5ACD</summary>
    public static LogColor SlateBlue => new(0x6A, 0x5A, 0xCD);

    /// <summary>#708090</summary>
    public static LogColor SlateGray => new(0x70, 0x80, 0x90);

    /// <summary>#FFFAFA</summary>
    public static LogColor Snow => new(0xFF, 0xFA, 0xFA);

    /// <summary>#00FF7F</summary>
    public static LogColor SpringGreen => new(0x00, 0xFF, 0x7F);

    /// <summary>#4682B4</summary>
    public static LogColor SteelBlue => new(0x46, 0x82, 0xB4);

    /// <summary>#D2B48C</summary>
    public static LogColor Tan => new(0xD2, 0xB4, 0x8C);

    /// <summary>#008080</summary>
    public static LogColor Teal => new(0x00, 0x80, 0x80);

    /// <summary>#D8BFD8</summary>
    public static LogColor Thistle => new(0xD8, 0xBF, 0xD8);

    /// <summary>#FF6347</summary>
    public static LogColor Tomato => new(0xFF, 0x63, 0x47);

    /// <summary>#40E0D0</summary>
    public static LogColor Turquoise => new(0x40, 0xE0, 0xD0);

    /// <summary>#EE82EE</summary>
    public static LogColor Violet => new(0xEE, 0x82, 0xEE);

    /// <summary>#F5DEB3</summary>
    public static LogColor Wheat => new(0xF5, 0xDE, 0xB3);

    /// <summary>#FFFFFF</summary>
    public static LogColor White => new(0xFF, 0xFF, 0xFF);

    /// <summary>#F5F5F5</summary>
    public static LogColor WhiteSmoke => new(0xF5, 0xF5, 0xF5);

    /// <summary>#A0522D</summary>
    public static LogColor Sienna => new(0xA0, 0x52, 0x2D);

    /// <summary>#FF4500</summary>
    public static LogColor OrangeRed => new(0xFF, 0x45, 0x00);

    /// <summary>#FFF5EE</summary>
    public static LogColor SeaShell => new(0xFF, 0xF5, 0xEE);

    /// <summary>#F4A460</summary>
    public static LogColor SandyBrown => new(0xF4, 0xA4, 0x60);

    /// <summary>#EEE8AA</summary>
    public static LogColor PaleGoldenrod => new(0xEE, 0xE8, 0xAA);

    /// <summary>#98FB98</summary>
    public static LogColor PaleGreen => new(0x98, 0xFB, 0x98);

    /// <summary>#AFEEEE</summary>
    public static LogColor PaleTurquoise => new(0xAF, 0xEE, 0xEE);

    /// <summary>#DB7093</summary>
    public static LogColor PaleVioletRed => new(0xDB, 0x70, 0x93);

    /// <summary>#FFEFD5</summary>
    public static LogColor PapayaWhip => new(0xFF, 0xEF, 0xD5);

    /// <summary>#FFDAB9</summary>
    public static LogColor PeachPuff => new(0xFF, 0xDA, 0xB9);

    /// <summary>#CD853F</summary>
    public static LogColor Peru => new(0xCD, 0x85, 0x3F);

    /// <summary>#FFC0CB</summary>
    public static LogColor Pink => new(0xFF, 0xC0, 0xCB);

    /// <summary>#DDA0DD</summary>
    public static LogColor Plum => new(0xDD, 0xA0, 0xDD);

    /// <summary>#B0E0E6</summary>
    public static LogColor PowderBlue => new(0xB0, 0xE0, 0xE6);

    /// <summary>#800080</summary>
    public static LogColor Purple => new(0x80, 0x00, 0x80);

    /// <summary>#663399</summary>
    public static LogColor RebeccaPurple => new(0x66, 0x33, 0x99);

    /// <summary>#FF0000</summary>
    public static LogColor Red => new(0xFF, 0x00, 0x00);

    /// <summary>#BC8F8F</summary>
    public static LogColor RosyBrown => new(0xBC, 0x8F, 0x8F);

    /// <summary>#4169E1</summary>
    public static LogColor RoyalBlue => new(0x41, 0x69, 0xE1);

    /// <summary>#8B4513</summary>
    public static LogColor SaddleBrown => new(0x8B, 0x45, 0x13);

    /// <summary>#FA8072</summary>
    public static LogColor Salmon => new(0xFA, 0x80, 0x72);

    /// <summary>#2E8B57</summary>
    public static LogColor SeaGreen => new(0x2E, 0x8B, 0x57);

    /// <summary>#F0E68C</summary>
    public static LogColor Khaki => new(0xF0, 0xE6, 0x8C);

    /// <summary>#E6E6FA</summary>
    public static LogColor Lavender => new(0xE6, 0xE6, 0xFA);

    /// <summary>#00FFFF</summary>
    public static LogColor Cyan => new(0x00, 0xFF, 0xFF);

    /// <summary>#8B008B</summary>
    public static LogColor DarkMagenta => new(0x8B, 0x00, 0x8B);

    /// <summary>#BDB76B</summary>
    public static LogColor DarkKhaki => new(0xBD, 0xB7, 0x6B);

    /// <summary>#006400</summary>
    public static LogColor DarkGreen => new(0x00, 0x64, 0x00);

    /// <summary>#A9A9A9</summary>
    public static LogColor DarkGray => new(0xA9, 0xA9, 0xA9);

    /// <summary>#B8860B</summary>
    public static LogColor DarkGoldenrod => new(0xB8, 0x86, 0x0B);

    /// <summary>#008B8B</summary>
    public static LogColor DarkCyan => new(0x00, 0x8B, 0x8B);

    /// <summary>#00008B</summary>
    public static LogColor DarkBlue => new(0x00, 0x00, 0x8B);

    /// <summary>#FFFFF0</summary>
    public static LogColor Ivory => new(0xFF, 0xFF, 0xF0);

    /// <summary>#DC143C</summary>
    public static LogColor Crimson => new(0xDC, 0x14, 0x3C);

    /// <summary>#FFF8DC</summary>
    public static LogColor Cornsilk => new(0xFF, 0xF8, 0xDC);

    /// <summary>#6495ED</summary>
    public static LogColor CornflowerBlue => new(0x64, 0x95, 0xED);

    /// <summary>#FF7F50</summary>
    public static LogColor Coral => new(0xFF, 0x7F, 0x50);

    /// <summary>#D2691E</summary>
    public static LogColor Chocolate => new(0xD2, 0x69, 0x1E);

    /// <summary>#556B2F</summary>
    public static LogColor DarkOliveGreen => new(0x55, 0x6B, 0x2F);

    /// <summary>#7FFF00</summary>
    public static LogColor Chartreuse => new(0x7F, 0xFF, 0x00);

    /// <summary>#DEB887</summary>
    public static LogColor BurlyWood => new(0xDE, 0xB8, 0x87);

    /// <summary>#A52A2A</summary>
    public static LogColor Brown => new(0xA5, 0x2A, 0x2A);

    /// <summary>#8A2BE2</summary>
    public static LogColor BlueViolet => new(0x8A, 0x2B, 0xE2);

    /// <summary>#0000FF</summary>
    public static LogColor Blue => new(0x00, 0x00, 0xFF);

    /// <summary>#FFEBCD</summary>
    public static LogColor BlanchedAlmond => new(0xFF, 0xEB, 0xCD);

    /// <summary>#000000</summary>
    public static LogColor Black => new(0x00, 0x00, 0x00);

    /// <summary>#FFE4C4</summary>
    public static LogColor Bisque => new(0xFF, 0xE4, 0xC4);

    /// <summary>#F5F5DC</summary>
    public static LogColor Beige => new(0xF5, 0xF5, 0xDC);

    /// <summary>#F0FFFF</summary>
    public static LogColor Azure => new(0xF0, 0xFF, 0xFF);

    /// <summary>#7FFFD4</summary>
    public static LogColor Aquamarine => new(0x7F, 0xFF, 0xD4);

    /// <summary>#00FFFF</summary>
    public static LogColor Aqua => new(0x00, 0xFF, 0xFF);

    /// <summary>#FAEBD7</summary>
    public static LogColor AntiqueWhite => new(0xFA, 0xEB, 0xD7);

    /// <summary>#F0F8FF</summary>
    public static LogColor AliceBlue => new(0xF0, 0xF8, 0xFF);

    /// <summary>#5F9EA0</summary>
    public static LogColor CadetBlue => new(0x5F, 0x9E, 0xA0);

    /// <summary>#FF8C00</summary>
    public static LogColor DarkOrange => new(0xFF, 0x8C, 0x00);

    /// <summary>#9ACD32</summary>
    public static LogColor YellowGreen => new(0x9A, 0xCD, 0x32);

    /// <summary>#8B0000</summary>
    public static LogColor DarkRed => new(0x8B, 0x00, 0x00);

    /// <summary>#4B0082</summary>
    public static LogColor Indigo => new(0x4B, 0x00, 0x82);

    /// <summary>#CD5C5C</summary>
    public static LogColor IndianRed => new(0xCD, 0x5C, 0x5C);

    /// <summary>#9932CC</summary>
    public static LogColor DarkOrchid => new(0x99, 0x32, 0xCC);

    /// <summary>#F0FFF0</summary>
    public static LogColor Honeydew => new(0xF0, 0xFF, 0xF0);

    /// <summary>#ADFF2F</summary>
    public static LogColor GreenYellow => new(0xAD, 0xFF, 0x2F);

    /// <summary>#008000</summary>
    public static LogColor Green => new(0x00, 0x80, 0x00);

    /// <summary>#808080</summary>
    public static LogColor Gray => new(0x80, 0x80, 0x80);

    /// <summary>#DAA520</summary>
    public static LogColor Goldenrod => new(0xDA, 0xA5, 0x20);

    /// <summary>#FFD700</summary>
    public static LogColor Gold => new(0xFF, 0xD7, 0x00);

    /// <summary>#F8F8FF</summary>
    public static LogColor GhostWhite => new(0xF8, 0xF8, 0xFF);

    /// <summary>#DCDCDC</summary>
    public static LogColor Gainsboro => new(0xDC, 0xDC, 0xDC);

    /// <summary>#FF00FF</summary>
    public static LogColor Fuchsia => new(0xFF, 0x00, 0xFF);

    /// <summary>#228B22</summary>
    public static LogColor ForestGreen => new(0x22, 0x8B, 0x22);

    /// <summary>#FF69B4</summary>
    public static LogColor HotPink => new(0xFF, 0x69, 0xB4);

    /// <summary>#B22222</summary>
    public static LogColor Firebrick => new(0xB2, 0x22, 0x22);

    /// <summary>#FFFAF0</summary>
    public static LogColor FloralWhite => new(0xFF, 0xFA, 0xF0);

    /// <summary>#1E90FF</summary>
    public static LogColor DodgerBlue => new(0x1E, 0x90, 0xFF);

    /// <summary>#696969</summary>
    public static LogColor DimGray => new(0x69, 0x69, 0x69);

    /// <summary>#00BFFF</summary>
    public static LogColor DeepSkyBlue => new(0x00, 0xBF, 0xFF);

    /// <summary>#FF1493</summary>
    public static LogColor DeepPink => new(0xFF, 0x14, 0x93);

    /// <summary>#9400D3</summary>
    public static LogColor DarkViolet => new(0x94, 0x00, 0xD3);

    /// <summary>#00CED1</summary>
    public static LogColor DarkTurquoise => new(0x00, 0xCE, 0xD1);

    /// <summary>#2F4F4F</summary>
    public static LogColor DarkSlateGray => new(0x2F, 0x4F, 0x4F);

    /// <summary>#483D8B</summary>
    public static LogColor DarkSlateBlue => new(0x48, 0x3D, 0x8B);

    /// <summary>#8FBC8B</summary>
    public static LogColor DarkSeaGreen => new(0x8F, 0xBC, 0x8B);

    /// <summary>#E9967A</summary>
    public static LogColor DarkSalmon => new(0xE9, 0x96, 0x7A);
}
