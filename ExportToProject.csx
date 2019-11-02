using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;

int progress = 0;
string projFolder = GetFolder(FilePath) + "Export_Project" + Path.DirectorySeparatorChar;
var context = new DecompileContext(Data, true);
TextureWorker worker = new TextureWorker();
ThreadLocal<DecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<DecompileContext>(() => new DecompileContext(Data, false));

if (Directory.Exists(projFolder))
{
    ScriptError("A project export already exists. Please remove it.", "Error");
    return;
}

Directory.CreateDirectory(projFolder);

// --------------- Start exporting ---------------

// Export sprites
UpdateProgressBar(null, "Exporting sprites...", progress++, 8);
await ExportSprites();

// Export backgrounds
UpdateProgressBar(null, "Exporting backgrounds...", progress++, 8);
await ExportBackground();

// Export objects
UpdateProgressBar(null, "Exporting objects...", progress++, 8);
await ExportGameObjects();

// Export rooms
UpdateProgressBar(null, "Exporting rooms...", progress++, 8);
await ExportRooms();

// Export sounds
UpdateProgressBar(null, "Exporting sounds...", progress++, 8);
await ExportSounds();

// Export scripts
UpdateProgressBar(null, "Exporting scripts...", progress++, 8);
await ExportScripts();

// Export fonts
UpdateProgressBar(null, "Exporting fonts...", progress++, 8);
await ExportFonts();

// Generate project file
UpdateProgressBar(null, "Generating project file...", progress++, 8);
ExportProjectFile();

// --------------- Export completed ---------------
worker.Cleanup();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + projFolder);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}
string BoolToString(bool value)
{
    // In the GMX file, -1 is true and 0 is false.
    return value ? "-1" : "0";
}

// --------------- Export Sprite ---------------
async Task ExportSprites()
{
    Directory.CreateDirectory(projFolder + "/sprites/images");
    await Task.Run(() => Parallel.ForEach(Data.Sprites, ExportSprite));
}
void ExportSprite(UndertaleSprite sprite)
{
    // Save the sprite GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/sprites/" + sprite.Name.Content + ".sprite.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("sprite");

    xmlWriter.WriteStartElement("type");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("xorig");
    xmlWriter.WriteString(sprite.OriginX.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("yorig");
    xmlWriter.WriteString(sprite.OriginY.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("colkind");
    xmlWriter.WriteString(sprite.BBoxMode.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("coltolerance");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("sepmasks");
    xmlWriter.WriteString(sprite.SepMasks.ToString("D"));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bboxmode");
    xmlWriter.WriteString(sprite.BBoxMode.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bbox_left");
    xmlWriter.WriteString(sprite.MarginLeft.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bbox_right");
    xmlWriter.WriteString(sprite.MarginRight.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bbox_top");
    xmlWriter.WriteString(sprite.MarginTop.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bbox_bottom");
    xmlWriter.WriteString(sprite.MarginBottom.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("HTile");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("VTile");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("TextureGroups");
    xmlWriter.WriteStartElement("TextureGroup0");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("For3D");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("width");
    xmlWriter.WriteString(sprite.Width.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("height");
    xmlWriter.WriteString(sprite.Height.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("frames");
    for (int i = 0; i < sprite.Textures.Count; i++)
    {
        if (sprite.Textures[i]?.Texture != null)
        {
            xmlWriter.WriteStartElement("frame");
            xmlWriter.WriteAttributeString("index", i.ToString());
            xmlWriter.WriteString("images\\" + sprite.Name.Content + "_" + i + ".png");
            xmlWriter.WriteEndElement();
        }
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();

    // Save sprite images
    for (int i = 0; i < sprite.Textures.Count; i++)
        if (sprite.Textures[i]?.Texture != null)
            worker.ExportAsPNG(sprite.Textures[i].Texture, projFolder + "/sprites/images/" + sprite.Name.Content + "_" + i + ".png");
}

// --------------- Export Background ---------------
async Task ExportBackground()
{
    Directory.CreateDirectory(projFolder + "/backgrounds/images");
    await Task.Run(() => Parallel.ForEach(Data.Backgrounds, ExportBackground));
}
void ExportBackground(UndertaleBackground background)
{
    // Save the backgound GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/backgrounds/" + background.Name.Content + ".background.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("background");

    xmlWriter.WriteStartElement("istileset");
    xmlWriter.WriteString("-1");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tilewidth");
    xmlWriter.WriteString(background.Texture.BoundingWidth.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tileheight");
    xmlWriter.WriteString(background.Texture.BoundingHeight.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tilexoff");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tileyoff");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tilehsep");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("tilevsep");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("HTile");
    xmlWriter.WriteString("-1");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("VTile");
    xmlWriter.WriteString("-1");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("TextureGroups");
    xmlWriter.WriteStartElement("TextureGroup0");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("For3D");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("width");
    xmlWriter.WriteString(background.Texture.BoundingWidth.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("height");
    xmlWriter.WriteString(background.Texture.BoundingHeight.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("data");
    xmlWriter.WriteString("images\\" + background.Name.Content + ".png");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();

    // Save background images
    worker.ExportAsPNG(background.Texture, projFolder + "/backgrounds/images/" + background.Name.Content + ".png");
}
// --------------- Export Object ---------------
async Task ExportGameObjects()
{
    Directory.CreateDirectory(projFolder + "/objects");
    await Task.Run(() => Parallel.ForEach(Data.GameObjects, ExportGameObject));
}
void ExportGameObject(UndertaleGameObject gameObject)
{
    // Save the object GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/objects/" + gameObject.Name.Content + ".object.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("object");

    xmlWriter.WriteStartElement("spriteName");
    xmlWriter.WriteString(gameObject.Sprite is null ? "<undefined>" : gameObject.Sprite.Name.Content);
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("solid");
    xmlWriter.WriteString(BoolToString(gameObject.Solid));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("visible");
    xmlWriter.WriteString(BoolToString(gameObject.Visible));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("depth");
    xmlWriter.WriteString(gameObject.Depth.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("persistent");
    xmlWriter.WriteString(BoolToString(gameObject.Persistent));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("parentName");
    xmlWriter.WriteString(gameObject.ParentId is null ? "<undefined>" : gameObject.ParentId.Name.Content);
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("maskName");
    xmlWriter.WriteString(gameObject.TextureMaskId is null ? "<undefined>" : gameObject.TextureMaskId.Name.Content);
    xmlWriter.WriteEndElement();

    // Save events
    xmlWriter.WriteStartElement("events");

    // Traversing the event type list
    for (int i = 0; i < gameObject.Events.Count; i++)
    {
        // Determine if an event is empty
        if (gameObject.Events[i].Count > 0)
        {
            // Traversing event list
            foreach (var j in gameObject.Events[i])
            {
                xmlWriter.WriteStartElement("event");

                xmlWriter.WriteAttributeString("eventtype", i.ToString());

                if (j.EventSubtype == 4)
                {
                    // To get the actual name of the collision object when the event type is a collision event
                    xmlWriter.WriteAttributeString("ename", Data.GameObjects[(int)j.EventSubtype].Name.Content);
                }
                else
                {
                    // Get the sub-event number directly
                    xmlWriter.WriteAttributeString("enumb", j.EventSubtype.ToString());
                }

                // Save action
                xmlWriter.WriteStartElement("action");

                // Traversing the action list
                foreach (var k in j.Actions)
                {
                    xmlWriter.WriteStartElement("libid");
                    xmlWriter.WriteString(k.LibID.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("id");
                    xmlWriter.WriteString(k.ID.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("kind");
                    xmlWriter.WriteString(k.Kind.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("userelative");
                    xmlWriter.WriteString(BoolToString(k.UseRelative));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("isquestion");
                    xmlWriter.WriteString(BoolToString(k.IsQuestion));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("useapplyto");
                    xmlWriter.WriteString(BoolToString(k.UseApplyTo));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("exetype");
                    xmlWriter.WriteString(k.ExeType.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("functionname");
                    xmlWriter.WriteString(k.ActionName.Content);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("codestring");
                    xmlWriter.WriteString("");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("whoName");
                    // All DND actions in data.win are converted to assembly code, automatically processing the performer
                    xmlWriter.WriteString("self");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("relative");
                    xmlWriter.WriteString(BoolToString(k.Relative));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("isnot");
                    xmlWriter.WriteString(BoolToString(k.IsNot));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("arguments");
                    xmlWriter.WriteStartElement("argument");

                    xmlWriter.WriteStartElement("kind");
                    xmlWriter.WriteString("1");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("string");
                    xmlWriter.WriteString(k.CodeId != null ? Decompiler.Decompile(k.CodeId, DECOMPILE_CONTEXT.Value) : "");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
                // TODO：Physics

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }
        }
    }

    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();
}

// --------------- Export Room ---------------
async Task ExportRooms()
{
    Directory.CreateDirectory(projFolder + "/rooms");
    await Task.Run(() => Parallel.ForEach(Data.Rooms, ExportRoom));
}
void ExportRoom(UndertaleRoom room)
{
    // Save the room GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/rooms/" + room.Name.Content + ".room.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("room");

    // Room settings
    xmlWriter.WriteStartElement("caption");
    xmlWriter.WriteString(room.Caption.Content);
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("width");
    xmlWriter.WriteString(room.Width.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("height");
    xmlWriter.WriteString(room.Height.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("vsnap");
    xmlWriter.WriteString("32");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("hsnap");
    xmlWriter.WriteString("32");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("isometric");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("speed");
    xmlWriter.WriteString(room.Speed.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("persistent");
    xmlWriter.WriteString(BoolToString(room.Persistent));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("colour");
    xmlWriter.WriteString(room.BackgroundColor.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("showcolour");
    xmlWriter.WriteString(BoolToString(room.DrawBackgroundColor));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("code");
    xmlWriter.WriteString(room.CreationCodeId is null ? "" : Decompiler.Decompile(room.CreationCodeId, context));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("enableViews");
    xmlWriter.WriteString(BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews)));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("clearViewBackground");
    xmlWriter.WriteString(BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ShowColor)));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("clearDisplayBuffer");
    xmlWriter.WriteString(BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ClearDisplayBuffer)));
    xmlWriter.WriteEndElement();

    // TODO：MakerSettings

    // Room backgrounds
    xmlWriter.WriteStartElement("backgrounds");
    foreach (var i in room.Backgrounds)
    {
        xmlWriter.WriteStartElement("background");

        xmlWriter.WriteAttributeString("visible", BoolToString(i.Enabled));
        xmlWriter.WriteAttributeString("foreground", BoolToString(i.Foreground));
        xmlWriter.WriteAttributeString("name", i.BackgroundDefinition is null ? "" : i.BackgroundDefinition.Name.Content);
        xmlWriter.WriteAttributeString("x", i.X.ToString());
        xmlWriter.WriteAttributeString("y", i.Y.ToString());
        xmlWriter.WriteAttributeString("htiled", i.TileX.ToString());
        xmlWriter.WriteAttributeString("vtiled", i.TileY.ToString());
        xmlWriter.WriteAttributeString("hspeed", i.SpeedX.ToString());
        xmlWriter.WriteAttributeString("vspeed", i.SpeedY.ToString());
        // TODO：Stretch
        xmlWriter.WriteAttributeString("stretch", "0");

        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    // Room views
    xmlWriter.WriteStartElement("views");
    foreach (var i in room.Views)
    {
        xmlWriter.WriteStartElement("view");

        xmlWriter.WriteAttributeString("visible", BoolToString(i.Enabled));
        xmlWriter.WriteAttributeString("objName", i.ObjectId is null ? "<undefined>" : i.ObjectId.Name.Content);
        xmlWriter.WriteAttributeString("xview", i.ViewX.ToString());
        xmlWriter.WriteAttributeString("yview", i.ViewY.ToString());
        xmlWriter.WriteAttributeString("wview", i.ViewWidth.ToString());
        xmlWriter.WriteAttributeString("hview", i.ViewHeight.ToString());
        xmlWriter.WriteAttributeString("xport", i.PortX.ToString());
        xmlWriter.WriteAttributeString("yport", i.PortY.ToString());
        xmlWriter.WriteAttributeString("wport", i.PortWidth.ToString());
        xmlWriter.WriteAttributeString("hport", i.PortHeight.ToString());
        xmlWriter.WriteAttributeString("hborder", i.BorderX.ToString());
        xmlWriter.WriteAttributeString("vborder", i.BorderY.ToString());
        xmlWriter.WriteAttributeString("hspeed", i.SpeedX.ToString());
        xmlWriter.WriteAttributeString("vspeed", i.SpeedY.ToString());

        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    // Room instances
    xmlWriter.WriteStartElement("instances");
    foreach (var i in room.GameObjects)
    {
        xmlWriter.WriteStartElement("instance");

        xmlWriter.WriteAttributeString("objName", i.ObjectDefinition.Name.Content);
        xmlWriter.WriteAttributeString("x", i.X.ToString());
        xmlWriter.WriteAttributeString("y", i.Y.ToString());
        xmlWriter.WriteAttributeString("name", "inst_" + i.InstanceID.ToString("X"));
        // TODO：Locked
        xmlWriter.WriteAttributeString("locked", "0");
        xmlWriter.WriteAttributeString("code", i.CreationCode != null ? Decompiler.Decompile(i.CreationCode, DECOMPILE_CONTEXT.Value) : "");
        xmlWriter.WriteAttributeString("scaleX", i.ScaleX.ToString());
        xmlWriter.WriteAttributeString("scaleY", i.ScaleY.ToString());
        xmlWriter.WriteAttributeString("colour", i.Color.ToString());
        xmlWriter.WriteAttributeString("rotation", i.Rotation.ToString());

        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    // Room tiles
    xmlWriter.WriteStartElement("tiles");
    foreach (var i in room.Tiles)
    {
        xmlWriter.WriteStartElement("tile");

        xmlWriter.WriteAttributeString("bgName", i.BackgroundDefinition.Name.Content);
        xmlWriter.WriteAttributeString("x", i.X.ToString());
        xmlWriter.WriteAttributeString("y", i.Y.ToString());
        xmlWriter.WriteAttributeString("w", i.Width.ToString());
        xmlWriter.WriteAttributeString("h", i.Height.ToString());
        xmlWriter.WriteAttributeString("xo", i.SourceX.ToString());
        xmlWriter.WriteAttributeString("yo", i.SourceY.ToString());
        xmlWriter.WriteAttributeString("id", i.InstanceID.ToString());
        xmlWriter.WriteAttributeString("name", "inst_" + i.InstanceID.ToString("X"));
        xmlWriter.WriteAttributeString("depth", i.TileDepth.ToString());
        // TODO：Locked
        xmlWriter.WriteAttributeString("locked", "0");
        xmlWriter.WriteAttributeString("colour", i.Color.ToString());
        xmlWriter.WriteAttributeString("scaleX", i.ScaleX.ToString());
        xmlWriter.WriteAttributeString("scaleY", i.ScaleY.ToString());

        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    // TODO：Room physics

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();
}

// --------------- Export Sound ---------------
async Task ExportSounds()
{
    Directory.CreateDirectory(projFolder + "/sound/audio");
    await Task.Run(() => Parallel.ForEach(Data.Sounds, ExportSound));
}
void ExportSound(UndertaleSound sound)
{
    // Save the sound GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/sound/" + sound.Name.Content + ".sound.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("sound");

    xmlWriter.WriteStartElement("kind");
    // Inferred by file extension
    xmlWriter.WriteString(Path.GetExtension(sound.File.Content) == ".ogg" ? "3" : "0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("extension");
    xmlWriter.WriteString(Path.GetExtension(sound.File.Content));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("origname");
    xmlWriter.WriteString("sound\\audio\\" + sound.File.Content);
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("effects");
    xmlWriter.WriteString(sound.Effects.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("volume");
    xmlWriter.WriteString(sound.Volume.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("pan");
    // TODO：Pan
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bitRates");
    // TODO：BitRates
    xmlWriter.WriteString("192");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("sampleRates");
    // TODO：SampleRates
    xmlWriter.WriteStartElement("sampleRate");
    xmlWriter.WriteString("44100");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("types");
    // TODO：Types
    xmlWriter.WriteStartElement("type");
    xmlWriter.WriteString("1");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bitDepths");
    // TODO：BitDepths
    xmlWriter.WriteStartElement("bitDepth");
    xmlWriter.WriteString("16");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("preload");
    // TODO：Preload
    xmlWriter.WriteString("-1");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("data");
    xmlWriter.WriteString(Path.GetFileName(sound.File.Content));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("compressed");
    // Inferred by file extension
    xmlWriter.WriteString(Path.GetExtension(sound.File.Content) == ".ogg" ? "1" : "0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("streamed");
    // Inferred by file extension
    xmlWriter.WriteString(Path.GetExtension(sound.File.Content) == ".ogg" ? "1" : "0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("uncompressOnLoad");
    // TODO：UncompressOnLoad
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("audioGroup");
    // TODO：AudioGroup
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();

    // Save sound files
    if (sound.AudioFile != null)
        File.WriteAllBytes(projFolder + "/sound/audio/" + sound.File.Content, sound.AudioFile.Data);
}

// --------------- Export Script ---------------
async Task ExportScripts()
{
    Directory.CreateDirectory(projFolder + "/scripts/");
    await Task.Run(() => Parallel.ForEach(Data.Scripts, ExportScript));
}
void ExportScript(UndertaleScript script)
{
    // Save GML files
    File.WriteAllText(projFolder + "/scripts/" + script.Name.Content + ".gml", (script.Code != null ? Decompiler.Decompile(script.Code, DECOMPILE_CONTEXT.Value) : ""));
}

// --------------- Export Font ---------------
async Task ExportFonts()
{
    Directory.CreateDirectory(projFolder + "/fonts/");
    await Task.Run(() => Parallel.ForEach(Data.Fonts, ExportFont));
}
void ExportFont(UndertaleFont font)
{
    // Save the font GMX
    var xmlWriter = XmlWriter.Create(projFolder + "/fonts/" + font.Name.Content + ".font.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("font");

    xmlWriter.WriteStartElement("name");
    xmlWriter.WriteString(font.Name.Content);
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("size");
    xmlWriter.WriteString(font.EmSize.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("bold");
    xmlWriter.WriteString(BoolToString(font.Bold));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("renderhq");
    xmlWriter.WriteString("-1");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("italic");
    xmlWriter.WriteString(BoolToString(font.Italic));
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("charset");
    xmlWriter.WriteString(font.Charset.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("aa");
    xmlWriter.WriteString(font.AntiAliasing.ToString());
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("includeTTF");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("TTFName");
    xmlWriter.WriteString("");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("texgroups");
    xmlWriter.WriteStartElement("texgroup");
    xmlWriter.WriteString("0");
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("ranges");
    xmlWriter.WriteStartElement("range0");
    xmlWriter.WriteString(font.RangeStart.ToString() + "," + font.RangeEnd.ToString());
    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("glyphs");
    foreach (var i in font.Glyphs)
    {
        xmlWriter.WriteStartElement("glyph");
        xmlWriter.WriteAttributeString("character", i.Character.ToString());
        xmlWriter.WriteAttributeString("x", i.SourceX.ToString());
        xmlWriter.WriteAttributeString("y", i.SourceY.ToString());
        xmlWriter.WriteAttributeString("w", i.SourceWidth.ToString());
        xmlWriter.WriteAttributeString("h", i.SourceHeight.ToString());
        xmlWriter.WriteAttributeString("shift", i.Shift.ToString());
        xmlWriter.WriteAttributeString("offset", i.Offset.ToString());
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("kerningPairs");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("image");
    xmlWriter.WriteString(font.Name.Content + ".png");
    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();

    // Save font textures
    worker.ExportAsPNG(font.Texture, projFolder + "/fonts/" + font.Name.Content + ".png");
}

// --------------- Generate project file ---------------
void ExportProjectFile()
{
    // Write all resource indexes to project.gmx
    var xmlWriter = XmlWriter.Create(projFolder + "Export_Project.project.gmx");
    xmlWriter.WriteStartDocument();

    xmlWriter.WriteStartElement("assets");

    xmlWriter.WriteStartElement("sounds");
    xmlWriter.WriteAttributeString("name", "sound");
    foreach (var i in Data.Sounds)
    {
        xmlWriter.WriteStartElement("sound");
        xmlWriter.WriteString("sound\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("sprites");
    xmlWriter.WriteAttributeString("name", "sprites");
    foreach (var i in Data.Sprites)
    {
        xmlWriter.WriteStartElement("sprite");
        xmlWriter.WriteString("sprites\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("backgrounds");
    xmlWriter.WriteAttributeString("name", "background");
    foreach (var i in Data.Backgrounds)
    {
        xmlWriter.WriteStartElement("background");
        xmlWriter.WriteString("backgrounds\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("scripts");
    xmlWriter.WriteAttributeString("name", "scripts");
    foreach (var i in Data.Scripts)
    {
        xmlWriter.WriteStartElement("script");
        xmlWriter.WriteString("scripts\\" + i.Name.Content + ".gml");
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("fonts");
    xmlWriter.WriteAttributeString("name", "fonts");
    foreach (var i in Data.Fonts)
    {
        xmlWriter.WriteStartElement("font");
        xmlWriter.WriteString("fonts\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("objects");
    xmlWriter.WriteAttributeString("name", "objects");
    foreach (var i in Data.GameObjects)
    {
        xmlWriter.WriteStartElement("object");
        xmlWriter.WriteString("objects\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteStartElement("rooms");
    xmlWriter.WriteAttributeString("name", "rooms");
    foreach (var i in Data.Rooms)
    {
        xmlWriter.WriteStartElement("room");
        xmlWriter.WriteString("rooms\\" + i.Name.Content);
        xmlWriter.WriteEndElement();
    }
    xmlWriter.WriteEndElement();

    xmlWriter.WriteEndElement();
    xmlWriter.WriteEndDocument();
    xmlWriter.Close();
}
