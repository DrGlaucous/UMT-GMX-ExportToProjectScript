using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Reflection;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;

string GameName = Data.GeneralInfo.Name.ToString().Replace(@"""",""); //Name == "Project" -> Project
int progress = 0;
string projFolder = GetFolder(FilePath) + GameName + ".gmx" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();
ThreadLocal<DecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<DecompileContext>(() => new DecompileContext(Data, false));
string gmxDeclaration = "This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!";
string eol = "\n"; // Linux: "\n", Windows: "\r\n"

if (Directory.Exists(projFolder))
{
    ScriptError("A project export already exists. Please remove it.", "Error");
    return;
}

Directory.CreateDirectory(projFolder);

// --------------- Start exporting ---------------

//total of all the resources in the file
var resourceNum = Data.Sprites.Count + 
    Data.Backgrounds.Count + 
    Data.GameObjects.Count + 
    Data.Rooms.Count + 
    Data.Sounds.Count + 
    Data.Scripts.Count + 
    Data.Fonts.Count + 
    Data.Paths.Count + 
    Data.Timelines.Count;

// Export sprites
await ExportSprites();

// Export backgrounds
await ExportBackground();

// Export objects
await ExportGameObjects();

// Export rooms
await ExportRooms();

// Export sounds
await ExportSounds();

// Export scripts
await ExportScripts();

// Export fonts
await ExportFonts();

// Export paths
await ExportPaths();

// Export timelines
await ExportTimelines();

// Generate project file
GenerateProjectFile();

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
    UpdateProgressBar(null, $"Exporting sprite: {sprite.Name.Content}", progress++, resourceNum);

    // Save the sprite GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("sprite",
            new XElement("type", "0"),
            new XElement("xorig", sprite.OriginX.ToString()),
            new XElement("yorigin", sprite.OriginY.ToString()),
            new XElement("colkind", sprite.BBoxMode.ToString()),
            new XElement("coltolerance", "0"),
            new XElement("sepmasks", sprite.SepMasks.ToString("D")),
            new XElement("bboxmode", sprite.BBoxMode.ToString()),
            new XElement("bbox_left", sprite.MarginLeft.ToString()),
            new XElement("bbox_right", sprite.MarginRight.ToString()),
            new XElement("bbox_top", sprite.MarginTop.ToString()),
            new XElement("bbox_bottom", sprite.MarginBottom.ToString()),
            new XElement("HTile", "0"),
            new XElement("VTile", "0"),
            new XElement("TextureGroups",
                new XElement("TextureGroup0", "0")
            ),
            new XElement("For3D", "0"),
            new XElement("width", sprite.Width.ToString()),
            new XElement("height", sprite.Height.ToString()),
            new XElement("frames")
        )
    );

    for (int i = 0; i < sprite.Textures.Count; i++)
    {
        if (sprite.Textures[i]?.Texture != null)
        {
            gmx.Element("sprite").Element("frames").Add(
                new XElement(
                    "frame",
                    new XAttribute("index", i.ToString()),
                    "images\\" + sprite.Name.Content + "_" + i + ".png"
                )
            );
        }
    }

    File.WriteAllText(projFolder + "/sprites/" + sprite.Name.Content + ".sprite.gmx", gmx.ToString() + eol);

    // Save sprite images
    for (int i = 0; i < sprite.Textures.Count; i++)
    {
        if (sprite.Textures[i]?.Texture != null)
        {
            worker.ExportAsPNG(sprite.Textures[i].Texture, projFolder + "/sprites/images/" + sprite.Name.Content + "_" + i + ".png", null, true);
        }
    }
}

// --------------- Export Background ---------------
async Task ExportBackground()
{
    Directory.CreateDirectory(projFolder + "/background/images");
    await Task.Run(() => Parallel.ForEach(Data.Backgrounds, ExportBackground));
}
void ExportBackground(UndertaleBackground background)
{
    UpdateProgressBar(null, $"Exporting background: {background.Name.Content}", progress++, resourceNum);

    // Save the backgound GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("background",
            new XElement("istileset", "-1"),
            new XElement("tilewidth", background.Texture == null ? "0" : background.Texture.BoundingWidth.ToString()),
            new XElement("tileheight", background.Texture == null ? "0" : background.Texture.BoundingHeight.ToString()),
            new XElement("tilexoff", "0"),
            new XElement("tileyoff", "0"),
            new XElement("tilehsep", "0"),
            new XElement("tilevsep", "0"),
            new XElement("HTile", "-1"),
            new XElement("VTile", "-1"),
            new XElement("TextureGroups",
                new XElement("TextureGroup0", "0")
            ),
            new XElement("For3D", "0"),
            new XElement("width", background.Texture == null ? "0" : background.Texture.BoundingWidth.ToString()),
            new XElement("height",background.Texture == null ? "0" :  background.Texture.BoundingHeight.ToString()),
            new XElement("data", "images\\" + background.Name.Content + ".png")
        )
    );

    File.WriteAllText(projFolder + "/background/" + background.Name.Content + ".background.gmx", gmx.ToString() + eol);

    // Save background images
    if (background.Texture != null)
        worker.ExportAsPNG(background.Texture, projFolder + "/background/images/" + background.Name.Content + ".png");
}
// --------------- Export Object ---------------
async Task ExportGameObjects()
{
    Directory.CreateDirectory(projFolder + "/objects");
    await Task.Run(() => Parallel.ForEach(Data.GameObjects, ExportGameObject));
}
void ExportGameObject(UndertaleGameObject gameObject)
{
    UpdateProgressBar(null, $"Exporting object: {gameObject.Name.Content}", progress++, resourceNum);

    // Save the object GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("object",
            new XElement("spriteName", gameObject.Sprite is null ? "<undefined>" : gameObject.Sprite.Name.Content),
            new XElement("solid", BoolToString(gameObject.Solid)),
            new XElement("visible", BoolToString(gameObject.Visible)),
            new XElement("depth", gameObject.Depth.ToString()),
            new XElement("persistent", BoolToString(gameObject.Persistent)),
            new XElement("parentName", gameObject.ParentId is null ? "<undefined>" : gameObject.ParentId.Name.Content),
            new XElement("maskName", gameObject.TextureMaskId is null ? "<undefined>" : gameObject.TextureMaskId.Name.Content),
            new XElement("events"),
			
			//Physics
			new XElement("PhysicsObject", BoolToString(gameObject.UsesPhysics)),
			new XElement("PhysicsObjectSensor", BoolToString(gameObject.IsSensor)),
			new XElement("PhysicsObjectShape", (uint)gameObject.CollisionShape),
			new XElement("PhysicsObjectDensity", gameObject.Density),
			new XElement("PhysicsObjectRestitution", gameObject.Restitution),
			new XElement("PhysicsObjectGroup", gameObject.Group),
			new XElement("PhysicsObjectLinearDamping", gameObject.LinearDamping),
			new XElement("PhysicsObjectAngularDamping", gameObject.AngularDamping),
			new XElement("PhysicsObjectFriction", gameObject.Friction),
			new XElement("PhysicsObjectAwake", BoolToString(gameObject.Awake)),
			new XElement("PhysicsObjectKinematic", BoolToString(gameObject.Kinematic)),
			new XElement("PhysicsShapePoints")
        )
    );


	
	// Loop through PhysicsShapePoints List
	for (int _point = 0; _point < gameObject.PhysicsVertices.Count; _point++)
	{
		var _x = gameObject.PhysicsVertices[_point].X;
		var _y = gameObject.PhysicsVertices[_point].Y;
		
		var physicsPointsNode = gmx.Element("object").Element("PhysicsShapePoints");
		physicsPointsNode.Add(new XElement("points",_x.ToString() + "," + _y.ToString()));
	}

    // Traversing the event type list
    for (int i = 0; i < gameObject.Events.Count; i++)
    {
        // Determine if an event is empty
        if (gameObject.Events[i].Count > 0)
        {
            // Traversing event list
            foreach (var j in gameObject.Events[i])//for every event
            {
                var eventsNode = gmx.Element("object").Element("events");

                var eventNode = new XElement("event",
                        new XAttribute("eventtype", i.ToString())
                );

                if (j.EventSubtype == 4)
                {
                    // To get the actual name of the collision object when the event type is a collision event
                    eventNode.Add(new XAttribute("ename", Data.GameObjects[(int)j.EventSubtype].Name.Content));
                }
                else
                {
                    // Get the sub-event number directly
                    eventNode.Add(new XAttribute("enumb", j.EventSubtype.ToString()));
                }

                // Save action
                var actionNode = new XElement("action");

                // Traversing the action list
                foreach (var k in j.Actions)
                {
                    actionNode.Add(
                        new XElement("libid", "1"),//k.LibID.ToString()),//forcing static values on all of these (because of the manner by which they are exported by the program)
                        new XElement("id", "603"),//k.ID.ToString()),//see UndertaleGameObject.cs: this value should always be 603, but it isn't
                        new XElement("kind", "7"),//k.Kind.ToString()),
                        new XElement("userelative", BoolToString(k.UseRelative)),
                        new XElement("isquestion", BoolToString(k.IsQuestion)),
                        new XElement("useapplyto", BoolToString(k.UseApplyTo)),
                        new XElement("exetype", k.ExeType.ToString()),
                        new XElement("functionname", k.ActionName.Content),
                        new XElement("codestring", ""),
                        new XElement("whoName", "self"),
                        new XElement("relative", BoolToString(k.Relative)),
                        new XElement("isnot", BoolToString(k.IsNot)),
                        new XElement("arguments",
                            new XElement("argument",
                                new XElement("kind", "1"),
                                new XElement("string", k.CodeId != null ? Decompiler.Decompile(k.CodeId, DECOMPILE_CONTEXT.Value) : "")
                            )
                        )
                    );
                }
                eventNode.Add(actionNode);
                eventsNode.Add(eventNode);

            }
        }
    }

    File.WriteAllText(projFolder + "/objects/" + gameObject.Name.Content + ".object.gmx", gmx.ToString() + eol);
}

// --------------- Export Room ---------------
async Task ExportRooms()
{
    Directory.CreateDirectory(projFolder + "/rooms");
    await Task.Run(() => Parallel.ForEach(Data.Rooms, ExportRoom));
}
void ExportRoom(UndertaleRoom room)
{
    UpdateProgressBar(null, $"Exporting room: {room.Name.Content}", progress++, resourceNum);

    // Save the room GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("room",
            new XElement("caption", room.Caption.Content),
            new XElement("width", room.Width.ToString()),
            new XElement("height", room.Height.ToString()),
            new XElement("vsnap", "32"),
            new XElement("hsnap", "32"),
            new XElement("isometric", "0"),
            new XElement("speed", room.Speed.ToString()),
            new XElement("persistent", BoolToString(room.Persistent)),
            new XElement("colour", (room.BackgroundColor - 0xFF000000).ToString()),//ALTER: this still counts the "alpha" channel of the BackgroundColor, which with the newer modtool, has been set to FF (0xFF000000). In Big_Endian format, this reads as 4278190080. GameMaker Studio does not like this
            new XElement("showcolour", BoolToString(room.DrawBackgroundColor)),
            new XElement("code", room.CreationCodeId != null ? Decompiler.Decompile(room.CreationCodeId, DECOMPILE_CONTEXT.Value) : ""),
            new XElement("enableViews", BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews))),
            new XElement("clearViewBackground", BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ShowColor))),
            new XElement("clearDisplayBuffer", BoolToString(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ClearDisplayBuffer))),
			new XElement("makerSettings",
				new XElement("isSet", 0),
				new XElement("w", 0),
				new XElement("h", 0),
				new XElement("showGrid", 0),
				new XElement("showObjects", 0),
				new XElement("showTiles", 0),
				new XElement("showBackgrounds", 0),
				new XElement("showForegrounds", 0),
				new XElement("showViews", 0),
				new XElement("deleteUnderlyingObj", 0),
				new XElement("deleteUnderlyingTiles", 0),
				new XElement("page", 0),
				new XElement("xoffset", 0),
				new XElement("yoffset", 0)
			)
        )
    );

    // Room backgrounds
    var backgroundsNode = new XElement("backgrounds");
    foreach (var i in room.Backgrounds)
    {
        var backgroundNode = new XElement("background",
            new XAttribute("visible", BoolToString(i.Enabled)),
            new XAttribute("foreground", BoolToString(i.Foreground)),
            new XAttribute("name", i.BackgroundDefinition is null ? "" : i.BackgroundDefinition.Name.Content),
            new XAttribute("x", i.X.ToString()),
            new XAttribute("y", i.Y.ToString()),
            new XAttribute("htiled", Convert.ToBoolean(i.TileX) ? "-1" : "0"),//i.TileX.ToString()),//(these values are boolean in the IDE, they specify if the map should loop the background h/v)
            new XAttribute("vtiled", Convert.ToBoolean(i.TileY) ? "-1" : "0"),//i.TileY.ToString()),//ALTER
            new XAttribute("hspeed", i.SpeedX.ToString()),
            new XAttribute("vspeed", i.SpeedY.ToString()),
            new XAttribute("stretch", "0")
        );
        backgroundsNode.Add(backgroundNode);
    }
    gmx.Element("room").Add(backgroundsNode);

    // Room views
    var viewsNode = new XElement("views");
    foreach (var i in room.Views)
    {
        var viewNode = new XElement("view",
            new XAttribute("visible", BoolToString(i.Enabled)),
            new XAttribute("objName", i.ObjectId is null ? "<undefined>" : i.ObjectId.Name.Content),
            new XAttribute("xview", i.ViewX.ToString()),
            new XAttribute("yview", i.ViewY.ToString()),
            new XAttribute("wview", i.ViewWidth.ToString()),
			new XAttribute("hview", i.ViewHeight.ToString()),
            new XAttribute("xport", i.PortX.ToString()),
            new XAttribute("yport", i.PortY.ToString()),
            new XAttribute("wport", i.PortWidth.ToString()),
            new XAttribute("hport", i.PortHeight.ToString()),
            new XAttribute("hborder", i.BorderX.ToString()),
            new XAttribute("vborder", i.BorderY.ToString()),
            new XAttribute("hspeed", i.SpeedX.ToString()),
            new XAttribute("vspeed", i.SpeedY.ToString())
        );
        viewsNode.Add(viewNode);
    }
    gmx.Element("room").Add(viewsNode);

    // Room instances
    var instancesNode = new XElement("instances");
    foreach (var i in room.GameObjects)
    {
        var instanceNode = new XElement("instance",
            new XAttribute("objName", i.ObjectDefinition.Name.Content),
            new XAttribute("x", i.X.ToString()),
            new XAttribute("y", i.Y.ToString()),
            new XAttribute("name", "inst_" + i.InstanceID.ToString("X")),
            new XAttribute("locked", "0"),
            new XAttribute("code", i.CreationCode != null ? Decompiler.Decompile(i.CreationCode, DECOMPILE_CONTEXT.Value) : ""),
            new XAttribute("scaleX", i.ScaleX.ToString()),
            new XAttribute("scaleY", i.ScaleY.ToString()),
            new XAttribute("colour", i.Color.ToString()),
            new XAttribute("rotation", i.Rotation.ToString())
        );
        instancesNode.Add(instanceNode);
    }
    gmx.Element("room").Add(instancesNode);

    // Room tiles
    var tilesNode = new XElement("tiles");
    foreach (var i in room.Tiles)
    {
        var tileNode = new XElement("tile",
            new XAttribute("bgName", i.BackgroundDefinition is null ? "" : i.BackgroundDefinition.Name.Content),
            new XAttribute("x", i.X.ToString()),
            new XAttribute("y", i.Y.ToString()),
            new XAttribute("w", i.Width.ToString()),
            new XAttribute("h", i.Height.ToString()),
            new XAttribute("xo", i.SourceX.ToString()),
            new XAttribute("yo", i.SourceY.ToString()),
            new XAttribute("id", i.InstanceID.ToString()),
            new XAttribute("name", "inst_" + i.InstanceID.ToString("X")),
            new XAttribute("depth", i.TileDepth.ToString()),
            new XAttribute("locked", "0"),
            new XAttribute("colour", i.Color.ToString()),
            new XAttribute("scaleX", i.ScaleX.ToString()),
            new XAttribute("scaleY", i.ScaleY.ToString())
        );
        tilesNode.Add(tileNode);
    }
    gmx.Element("room").Add(tilesNode);

	//Room Physics
	
	gmx.Element("room").Add(
		new XElement("PhysicsWorld", room.World ? -1 : 0),//ALTER: (convert from true/false to -1 or 0)
        new XElement("PhysicsWorldTop", room.Top),
		new XElement("PhysicsWorldLeft", room.Left),
		new XElement("PhysicsWorldRight", room.Right),
		new XElement("PhysicsWorldBottom", room.Bottom),
		new XElement("PhysicsWorldGravityX", room.GravityX),
		new XElement("PhysicsWorldGravityY", room.GravityY),
		new XElement("PhysicsWorldPixToMeters", room.MetersPerPixel)
	);

    File.WriteAllText(projFolder + "/rooms/" + room.Name.Content + ".room.gmx", gmx.ToString() + eol);
}

// --------------- Export Sound ---------------
async Task ExportSounds()
{
    Directory.CreateDirectory(projFolder + "/sound/audio");
    await Task.Run(() => Parallel.ForEach(Data.Sounds, ExportSound));
}
void ExportSound(UndertaleSound sound)
{
    UpdateProgressBar(null, $"Exporting sound: {sound.Name.Content}", progress++, resourceNum);

    // Save the sound GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("sound",
            new XElement("kind", Path.GetExtension(sound.File.Content) == ".ogg" ? "3" : "0"),
            new XElement("extension", Path.GetExtension(sound.File.Content)),
            new XElement("origname", "sound\\audio\\" + sound.File.Content),
            new XElement("effects", sound.Effects.ToString()),
			new XElement("volume",
				new XElement("volume", sound.Volume.ToString())
			),
            new XElement("pan", "0"),
			new XElement("bitRates",
				new XElement("bitRate", "192")
			),
            new XElement("sampleRates",
                new XElement("sampleRate", "44100")
            ),
            new XElement("types",
                new XElement("type", "1")
            ),
            new XElement("bitDepths",
                new XElement("bitDepth", "16")
            ),
            new XElement("preload", "-1"),
            new XElement("data", Path.GetFileName(sound.File.Content)),
            new XElement("compressed", Path.GetExtension(sound.File.Content) == ".ogg" ? "1" : "0"),
            new XElement("streamed", Path.GetExtension(sound.File.Content) == ".ogg" ? "1" : "0"),
            new XElement("uncompressOnLoad", "0"),
            new XElement("audioGroup", "0")
        )
    );

    File.WriteAllText(projFolder + "/sound/" + sound.Name.Content + ".sound.gmx", gmx.ToString() + eol);

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
    UpdateProgressBar(null, $"Exporting script: {script.Name.Content}", progress++, resourceNum);

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
    UpdateProgressBar(null, $"Exporting font: {font.Name.Content}", progress++, resourceNum);

    // Save the font GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("font",
            new XElement("name", font.Name.Content),
            new XElement("size", font.EmSize.ToString()),
            new XElement("bold", BoolToString(font.Bold)),
            new XElement("renderhq", "-1"),
            new XElement("italic", BoolToString(font.Italic)),
            new XElement("charset", font.Charset.ToString()),
            new XElement("aa", font.AntiAliasing.ToString()),
            new XElement("includeTTF", "0"),
            new XElement("TTFName", ""),
            new XElement("texgroups",
                new XElement("texgroup", "0")
            ),
            new XElement("ranges",
                new XElement("range0", font.RangeStart.ToString() + "," + font.RangeEnd.ToString())
            ),
            new XElement("glyphs"),
            new XElement("kerningPairs"),
            new XElement("image", font.Name.Content + ".png")
        )
    );

    var glyphsNode = gmx.Element("font").Element("glyphs");
    foreach (var i in font.Glyphs)
    {
        var glyphNode = new XElement("glyph");
        glyphNode.Add(new XAttribute("character", i.Character.ToString()));
        glyphNode.Add(new XAttribute("x", i.SourceX.ToString()));
        glyphNode.Add(new XAttribute("y", i.SourceY.ToString()));
        glyphNode.Add(new XAttribute("w", i.SourceWidth.ToString()));
        glyphNode.Add(new XAttribute("h", i.SourceHeight.ToString()));
        glyphNode.Add(new XAttribute("shift", i.Shift.ToString()));
        glyphNode.Add(new XAttribute("offset", i.Offset.ToString()));
        glyphsNode.Add(glyphNode);
    }

    File.WriteAllText(projFolder + "/fonts/" + font.Name.Content + ".font.gmx", gmx.ToString() + eol);

    // Save font textures
    worker.ExportAsPNG(font.Texture, projFolder + "/fonts/" + font.Name.Content + ".png");
}

// --------------- Export Paths ---------------
async Task ExportPaths()
{
    Directory.CreateDirectory(projFolder + "/paths");
    await Task.Run(() => Parallel.ForEach(Data.Paths, ExportPath));
}
void ExportPath(UndertalePath path)
{
    UpdateProgressBar(null, $"Exporting path: {path.Name.Content}", progress++, resourceNum);

    // Save the path GMX
    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("path",
            new XElement("kind", "0"),
            new XElement("closed", BoolToString(path.IsClosed)),
            new XElement("precision", path.Precision.ToString()),
            new XElement("backroom", "-1"),
            new XElement("hsnap", "16"),
            new XElement("vsnap", "16"),
            new XElement("points")
        )
    );
    foreach (var i in path.Points)
    {
        var pointsNode = gmx.Element("path").Element("points");
        pointsNode.Add(
            new XElement("point", $"{i.X.ToString()},{i.Y.ToString()},{i.Speed.ToString()}")
        );
    }

    File.WriteAllText(projFolder + "/paths/" + path.Name.Content + ".path.gmx", gmx.ToString() + eol);
}

// --------------- Export Timelines ---------------
async Task ExportTimelines()
{
    Directory.CreateDirectory(projFolder + "/timelines");
    await Task.Run(() => Parallel.ForEach(Data.Timelines, ExportTimeline));
}

void ExportTimeline(UndertaleTimeline timeline)
{
    UpdateProgressBar(null, $"Exporting timeline: {timeline.Name.Content}", progress++, resourceNum);

    // Save the timeline GMX
    var gmx = new XDocument(//this does the saving in XML format
        new XComment(gmxDeclaration),//XML comment
        new XElement("timeline")//I think I understand now
    );
    foreach (var i in timeline.Moments)
    {
        var entryNode = new XElement("entry");
        entryNode.Add(new XElement("step", i.Step));//I don't know where "Item1" comes into play... (Item1 and Item2 replaced with Step and Event, respectively)
        entryNode.Add(new XElement("event"));
        foreach (var j in i.Event)//same goes for Item2... and the modTool doesn't like it either
        {
            entryNode.Element("event").Add(
                new XElement("action",
                    new XElement("libid", j.LibID.ToString()),
                    new XElement("id", j.ID.ToString()),
                    new XElement("kind", j.Kind.ToString()),
                    new XElement("userelative", BoolToString(j.UseRelative)),
                    new XElement("isquestion", BoolToString(j.IsQuestion)),
                    new XElement("useapplyto", BoolToString(j.UseApplyTo)),
                    new XElement("exetype", j.ExeType.ToString()),
                    new XElement("functionname", j.ActionName.Content),
                    new XElement("codestring", ""),
                    new XElement("whoName", "self"),
                    new XElement("relative", BoolToString(j.Relative)),
                    new XElement("isnot", BoolToString(j.IsNot)),
                    new XElement("arguments",
                        new XElement("argument",
                            new XElement("kind", "1"),
                            new XElement("string", j.CodeId != null ? Decompiler.Decompile(j.CodeId, DECOMPILE_CONTEXT.Value) : "")
                        )
                    )
                )
            );
        }
        gmx.Element("timeline").Add(entryNode);
    }

    File.WriteAllText(projFolder + "/timelines/" + timeline.Name.Content + ".timeline.gmx", gmx.ToString() + eol);
}


// --------------- Generate project file ---------------
void GenerateProjectFile()
{
    UpdateProgressBar(null, $"Generating project file...", progress++, resourceNum);

    var gmx = new XDocument(
        new XComment(gmxDeclaration),
        new XElement("assets")
    );

    // Write all resource indexes to project.gmx
    WriteIndexes<UndertaleSound>(gmx.Element("assets"), "sounds", "sound", Data.Sounds, "sound", "sound\\");
    WriteIndexes<UndertaleSprite>(gmx.Element("assets"), "sprites", "sprites", Data.Sprites, "sprite", "sprites\\");
    WriteIndexes<UndertaleBackground>(gmx.Element("assets"), "backgrounds", "background", Data.Backgrounds, "background", "background\\");
    WriteIndexes<UndertaleScript>(gmx.Element("assets"), "scripts", "scripts", Data.Scripts, "script", "scripts\\", ".gml");
    WriteIndexes<UndertaleFont>(gmx.Element("assets"), "fonts", "fonts", Data.Fonts, "font", "fonts\\");
    WriteIndexes<UndertaleGameObject>(gmx.Element("assets"), "objects", "objects", Data.GameObjects, "object", "objects\\");
    WriteIndexes<UndertaleRoom>(gmx.Element("assets"), "rooms", "rooms", Data.Rooms, "room", "rooms\\");
    WriteIndexes<UndertalePath>(gmx.Element("assets"), "paths", "paths", Data.Paths, "path", "paths\\");
    WriteIndexes<UndertaleTimeline>(gmx.Element("assets"), "timelines", "timelines", Data.Timelines, "timeline", "timelines\\");

    File.WriteAllText(projFolder + GameName + ".project.gmx", gmx.ToString() + eol);
}

void WriteIndexes<T>(XElement rootNode, string elementName, string attributeName, IList<T> dataList, string oneName, string resourcePath, string fileExtension = "")
{
    var resourcesNode = new XElement(elementName,
        new XAttribute("name", attributeName)
    );
    foreach (UndertaleNamedResource i in dataList)
    {
        var resourceNode = new XElement(oneName, resourcePath + i.Name.Content + fileExtension);
        resourcesNode.Add(resourceNode);
    }
    rootNode.Add(resourcesNode);
}
