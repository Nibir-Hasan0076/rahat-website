using System.Text;

var root = Path.Combine("wwwroot", "images");

void Write(string relative, string svg)
{
    var file = Path.Combine(root, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
    File.WriteAllText(file, svg.Trim() + "\n", Encoding.UTF8);
}

string backdrop(P p) => $"""
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="0.4" y2="1">
      <stop offset="0" stop-color="{p.BgA}"/>
      <stop offset="1" stop-color="{p.BgB}"/>
    </linearGradient>
    <linearGradient id="cloth" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="{p.ClothA}"/>
      <stop offset="1" stop-color="{p.ClothB}"/>
    </linearGradient>
  </defs>
  <rect width="800" height="1000" fill="url(#bg)"/>
  <circle cx="652" cy="196" r="168" fill="#ffffff" opacity=".26"/>
  <circle cx="132" cy="812" r="228" fill="#ffffff" opacity=".16"/>
  <ellipse cx="400" cy="892" rx="248" ry="30" fill="#6f6658" opacity=".13"/>
""";

string longShirt(P p) => $"""
  <path d="M318 232 L400 200 L482 232 L548 274 L636 486 L556 532 L516 448 L520 842 L280 842 L284 448 L244 532 L164 486 L252 274 Z"
        fill="url(#cloth)" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <path d="M330 236 L400 206 L470 236 L458 272 L400 244 L342 272 Z"
        fill="{p.Trim}" stroke="{p.Line}" stroke-width="2.5" stroke-linejoin="round"/>
  <rect x="389" y="256" width="22" height="566" rx="4" fill="{p.Trim}"/>
  {string.Join("", new[] { 306, 386, 466, 546, 626, 706 }.Select(y => $"<circle cx=\"400\" cy=\"{y}\" r=\"5.5\" fill=\"{p.Button}\"/>"))}
  <path d="M330 642 q28 58 8 188" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".55"/>
  <path d="M470 642 q-28 58 -8 188" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".55"/>
  <path d="M284 448 l32 24" stroke="{p.Line}" stroke-width="2" opacity=".5"/>
  <path d="M516 448 l-32 24" stroke="{p.Line}" stroke-width="2" opacity=".5"/>
""";

string polo(P p, bool stripes = false)
{
    var stripeOverlay = stripes
        ? """<path d="M322 262 C350 282 450 282 478 262 L556 306 L640 496 L558 542 L516 458 L520 756 L280 756 L284 458 L242 542 L160 496 L244 306 Z" fill="url(#stripe)"/>"""
        : string.Empty;

    return $"""
  <defs>
    <pattern id="stripe" width="34" height="12" patternUnits="userSpaceOnUse">
      <rect width="16" height="12" fill="#ffffff" opacity=".14"/>
    </pattern>
  </defs>
  <path d="M322 262 C350 282 450 282 478 262 L556 306 L640 496 L558 542 L516 458 L520 756 L280 756 L284 458 L242 542 L160 496 L244 306 Z"
        fill="url(#cloth)" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  {stripeOverlay}
  <path d="M322 262 L400 346 L478 262 L508 302 L400 412 L292 302 Z" fill="{p.Trim}" stroke="{p.Line}" stroke-width="2.5" stroke-linejoin="round"/>
  <rect x="386" y="326" width="28" height="112" rx="4" fill="{p.Placket}"/>
  <circle cx="400" cy="350" r="5" fill="{p.Button}"/>
  <circle cx="400" cy="388" r="5" fill="{p.Button}"/>
  <circle cx="400" cy="426" r="5" fill="{p.Button}"/>
  <path d="M284 458 l28 22 M516 458 l-28 22" stroke="{p.Line}" stroke-width="2" opacity=".55"/>
  <path d="M326 600 q26 40 6 148" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".4"/>
  <path d="M474 600 q-26 40 -6 148" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".4"/>
""";
}

string fullShirt(P p) => $"""
  <path d="M318 250 L400 216 L482 250 L552 296 L638 492 L556 538 L516 452 L520 762 L280 762 L284 452 L244 538 L162 492 L248 296 Z"
        fill="url(#cloth)" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <path d="M318 250 L400 330 L482 250 L462 236 L400 286 L338 236 Z" fill="{p.Trim}" stroke="{p.Line}" stroke-width="2.5" stroke-linejoin="round"/>
  <path d="M338 236 L400 286 L300 320 Z" fill="{p.Collar}" stroke="{p.Line}" stroke-width="2" stroke-linejoin="round"/>
  <path d="M462 236 L400 286 L500 320 Z" fill="{p.Collar}" stroke="{p.Line}" stroke-width="2" stroke-linejoin="round"/>
  <rect x="389" y="300" width="22" height="440" rx="3" fill="{p.Placket}"/>
  {string.Join("", new[] { 330, 408, 486, 564, 642, 716 }.Select(y => $"<circle cx=\"400\" cy=\"{y}\" r=\"5\" fill=\"{p.Button}\"/>"))}
  <rect x="298" y="382" width="74" height="78" rx="6" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".7"/>
  <path d="M284 452 l30 24 M516 452 l-30 24" stroke="{p.Line}" stroke-width="2" opacity=".5"/>
  <path d="M330 684 q26 20 6 74 M470 684 q-26 20 -6 74" fill="none" stroke="{p.Line}" stroke-width="2" opacity=".4"/>
""";

string pants(P p) => $"""
  <defs>
    <pattern id="denim" width="8" height="8" patternUnits="userSpaceOnUse" patternTransform="rotate(35)">
      <rect width="3" height="8" fill="#ffffff" opacity=".06"/>
    </pattern>
  </defs>
  <path d="M266 250 h268 v64 h-268 z" fill="{p.Trim}" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <path d="M266 314 L392 314 L376 816 L262 816 Z" fill="url(#cloth)" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <path d="M408 314 L534 314 L538 816 L424 816 Z" fill="url(#cloth)" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <path d="M392 314 Q400 382 408 314" fill="{p.Trim}" stroke="{p.Line}" stroke-width="3" stroke-linejoin="round"/>
  <rect x="266" y="250" width="268" height="64" fill="url(#denim)"/>
  <path d="M400 314 L400 392" stroke="{p.Line}" stroke-width="2.5" opacity=".8"/>
  <circle cx="400" cy="276" r="6" fill="{p.Button}"/>
  <path d="M322 340 L314 806 M478 340 L486 806" stroke="{p.Stripe}" stroke-width="2.5" opacity=".55" stroke-dasharray="10 7"/>
  <path d="M292 292 h20 M488 292 h20" stroke="{p.Line}" stroke-width="3" opacity=".7"/>
  <ellipse cx="400" cy="884" rx="210" ry="24" fill="#6f6658" opacity=".12"/>
""";

string product(string type, P p, bool stripes = false)
{
    var body = type switch
    {
        "longShirt" => longShirt(p),
        "polo" => polo(p, stripes),
        "shirt" => fullShirt(p),
        "pants" => pants(p),
        _ => throw new ArgumentException(type)
    };
    return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 1000" role="img">
{backdrop(p)}
{body}
</svg>
""";
}

string placeholder(string label) => $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 1000" role="img">
  <rect width="800" height="1000" fill="#EFEAE1"/>
  <rect x="36" y="36" width="728" height="928" rx="26" fill="none" stroke="#D8CFC0" stroke-width="3" stroke-dasharray="12 12"/>
  <g opacity=".5">
    <path d="M318 300 L400 266 L482 300 L552 346 L638 542 L556 588 L516 502 L520 760 L280 760 L284 502 L244 588 L162 542 L248 346 Z" fill="#DED5C6"/>
    <path d="M318 300 L400 380 L482 300 L462 286 L400 336 L338 286 Z" fill="#CFC4B2"/>
  </g>
  <text x="400" y="836" text-anchor="middle" font-family="Georgia, serif" font-size="30" fill="#A79B88">{label}</text>
</svg>
""";

var ivory = new P("#F2EDE4", "#E7DECD", "#CFC2AC", "#F8F4EB", "#EFE8DB", "#F8F4EB", "#A98D6A", "#FFFFFF", "#FAF6EE", "#EFE7D9");
var navy = new P("#E9EDF3", "#DCE2EB", "#1B2740", "#1F2C45", "#1B2740", "#243453", "#C8A96B", "#93A3BA", "#2B3B58", "#20304A");
var sage = new P("#EAEDE4", "#DEE4D7", "#55684F", "#6F8467", "#5E7357", "#6F8467", "#EFE9DA", "#C9D3C1", "#81947A", "#71856A");
var oxford = new P("#E9E6DF", "#DED9CF", "#77879B", "#DCE4EC", "#C6D2DF", "#C6D2DF", "#F4F1E9", "#9AA9BA", "#D5DFE9", "#C3CFDC");
var linen = new P("#F0E8DB", "#E6DCCB", "#9C8060", "#DCC7A7", "#CBB18E", "#CBB18E", "#7A5F42", "#EDE1CF", "#DECBAE", "#CDB794");
var chino = new P("#ECE6DC", "#E1D9CB", "#8A7152", "#A68B68", "#A68B68", "#A68B68", "#59452F", "#D6C3A6", "#B9A486", "#A78F6F");
var denim = new P("#E8EBEF", "#DDE2E8", "#3D5470", "#415B79", "#415B79", "#415B79", "#D7B979", "#C9A45F", "#4C6584", "#3F5673");

Write("products/panjabi-ivory.svg", product("longShirt", ivory));
Write("products/panjabi-navy.svg", product("longShirt", navy));
Write("products/polo-navy.svg", product("polo", navy));
Write("products/polo-sage.svg", product("polo", sage, true));
Write("products/shirt-oxford.svg", product("shirt", oxford));
Write("products/shirt-linen.svg", product("shirt", linen));
Write("products/pants-chino.svg", product("pants", chino));
Write("products/pants-denim.svg", product("pants", denim));

Write("categories/panjabi.svg", product("longShirt", ivory));
Write("categories/polo.svg", product("polo", sage, true));
Write("categories/shirt.svg", product("shirt", oxford));
Write("categories/pants.svg", product("pants", denim));

Write("placeholder-product.svg", placeholder("Product image"));
Write("placeholder-category.svg", placeholder("Category image"));

Console.WriteLine($"Artwork written to {Path.GetFullPath(root)}");

record P(
    string BgA,
    string BgB,
    string Line,
    string Trim,
    string Placket,
    string Collar,
    string Button,
    string Stripe,
    string ClothA,
    string ClothB);
