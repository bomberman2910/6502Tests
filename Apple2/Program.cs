using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib6502;
using static SDL2.SDL;

namespace Apple2;

internal class Program
{
    public const string RomLocation = "";
    private const string OsdCharRom = "342-0265-A.bin";
    public const string SystemCharRom = "charmap.rom";
    private const string D0Rom = "341011d0.bin";
    private const string D8Rom = "341012d8.bin";
    private const string E0Rom = "341013e0.bin";
    private const string E8Rom = "341014e8.bin";
    private const string F0Rom = "341015f0.bin";
    private const string F8Rom = "341020f8.bin";
    
    private const int WindowWidth = 1280;
    private const int WindowHeight = 960;
    private const int FrameTicks = 17;

    private static IntPtr renderer;
    private static IntPtr window;
    private static IntPtr texture;
    private static bool running = true;
    private static readonly bool[] KeyboardState = new bool[512];
    private static bool isCpuRunning;
    private static uint nextFrame;
    private static byte cursorFrameCount;
    private static bool isKeyPressHandled;
    private static byte[][] charset;
    private static byte[] nonShiftedScancodes;
    private static byte[] shiftedScancodes;

    private static PixelDisplay display;

    private static Bus mainBus;
    private static Cpu6502 cpu;

    private static Queue<char> inputBuffer = new();

    /// <summary>
    ///     Setup all of the SDL resources we'll need to display a window.
    /// </summary>
    private static void Setup()
    {
        #region initializing SDL
        // Initializes SDL.
        if (SDL_Init(SDL_INIT_VIDEO) < 0) Console.WriteLine($"There was an issue initializing SDL. {SDL_GetError()}");

        // Create a new window given a title, size, and passes it a flag indicating it should be shown.
        window = SDL_CreateWindow(
            "Apple2Sharp",
            SDL_WINDOWPOS_UNDEFINED,
            SDL_WINDOWPOS_UNDEFINED,
            WindowWidth,
            WindowHeight,
            SDL_WindowFlags.SDL_WINDOW_SHOWN);

        if (window == IntPtr.Zero) Console.WriteLine($"There was an issue creating the window. {SDL_GetError()}");

        // Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
        renderer = SDL_CreateRenderer(
            window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (renderer == IntPtr.Zero) Console.WriteLine($"There was an issue creating the renderer. {SDL_GetError()}");

        texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, WindowWidth, WindowHeight);

        if (texture == IntPtr.Zero) Console.WriteLine($"There was an issue creating the texture. {SDL_GetError()}");
        #endregion
        
        charset = new byte[256][];
        using (var charsetRom = File.OpenRead(Path.Combine(RomLocation, OsdCharRom)))
        {
            for (var i = 0; i < 256; i++)
            {
                var character = new byte[8];
                charsetRom.ReadExactly(character, 0, 8);
                charset[i] = character;
            }
        }

        #region filling scancodes

        nonShiftedScancodes = new byte[512];
        shiftedScancodes = new byte[512];
        {
            // letters
            var i = 0;
            while (i < 30)
            {
                nonShiftedScancodes[i] = (byte)(i + 61);
                i++;
            }

            // numbers
            while (i < 39)
            {
                nonShiftedScancodes[i] = (byte)(i + 19);
                i++;
            }
        }
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_0] = (byte)'0';
        // special and control characters
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_RETURN] = 0x0D;
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_ESCAPE] = 0x1B;
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_BACKSPACE] = (byte)'_';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_EQUALS] = (byte)'=';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_MINUS] = (byte)'-';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_SLASH] = (byte)'/';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_COMMA] = (byte)',';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_SPACE] = (byte)' ';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_PERIOD] = (byte)'.';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_SEMICOLON] = (byte)';';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_APOSTROPHE] = (byte)'\'';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_LEFTBRACKET] = (byte)'[';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET] = (byte)']';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_BACKSLASH] = (byte)'\\';
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_DELETE] = 0x7F;
        // arrow keys
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_LEFT] = 0x08;
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_UP] = 0x0B;
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_RIGHT] = 0x15;
        nonShiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_DOWN] = 0x0A;

        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_1] = (byte)'!';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_2] = (byte)'@';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_3] = (byte)'#';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_4] = (byte)'$';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_5] = (byte)'%';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_6] = (byte)'^';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_7] = (byte)'&';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_8] = (byte)'*';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_9] = (byte)'(';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_0] = (byte)')';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_MINUS] = (byte)'_';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_EQUALS] = (byte)'+';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_LEFTBRACKET] = (byte)'{';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET] = (byte)'}';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_SEMICOLON] = (byte)':';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_APOSTROPHE] = (byte)'"';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_BACKSLASH] = (byte)'|';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_COMMA] = (byte)'<';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_PERIOD] = (byte)'>';
        shiftedScancodes[(int)SDL_Scancode.SDL_SCANCODE_SLASH] = (byte)'?';

        #endregion

        // Initialize Apple ][
        display = new PixelDisplay();
        mainBus = new Bus();
        cpu = new Cpu6502(mainBus);

        // base RAM with Zeropage and Stack
        mainBus.Devices.Add(new RandomAccessMemory(0x0400, 0x0000));
        // low graphics RAM
        mainBus.Devices.Add(new LowResGraphicsRam(display));
        // RAM used by the graphics output (contains 0x1400 bytes free RAM starting at 0x0C00
        // free RAM
        mainBus.Devices.Add(new RandomAccessMemory(0x1400, 0x0C00));
        mainBus.Devices.Add(new RandomAccessMemory(0x4000, 0x2000)); // TODO Video RAM
        // free RAM
        mainBus.Devices.Add(new RandomAccessMemory(0x6000, 0x6000));

        // control devices
        mainBus.Devices.Add(new KeyboardStrobeRegister());
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xC001));
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xC006));
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xC007));
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xC010));
        mainBus.Devices.Add(new ReadOnlyMemory(0x000F, 0xC011));
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xC030));
        mainBus.Devices.Add(new GraphicsRegisters(display));
        mainBus.Devices.Add(new RandomAccessMemory(0x0008, 0xC058));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0002, 0xC061));
        mainBus.Devices.Add(new RandomAccessMemory(0x0001, 0xCFFF));

        // slot ROMs
        mainBus.Devices.Add(new ReadOnlyMemory(0x0F00, 0xC100));

        // Apple ][ ROMs
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xD000, Path.Combine(RomLocation, D0Rom)));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xD800, Path.Combine(RomLocation, D8Rom)));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xE000, Path.Combine(RomLocation, E0Rom)));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xE800, Path.Combine(RomLocation, E8Rom)));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xF000, Path.Combine(RomLocation, F0Rom)));
        mainBus.Devices.Add(new ReadOnlyMemory(0x0800, 0xF800, Path.Combine(RomLocation, F8Rom)));

        nextFrame = SDL_GetTicks() + FrameTicks;
    }

    /// <summary>
    ///     Checks to see if there are any events to be processed.
    /// </summary>
    private static void PollEvents()
    {
        // Check to see if there are any events and continue to do so until the queue is empty.
        while (SDL_PollEvent(out var e) == 1)
        {
            switch (e.type)
            {
                case SDL_EventType.SDL_QUIT:
                    running = false;
                    break;
                case SDL_EventType.SDL_KEYDOWN:
                    KeyboardState[(int)e.key.keysym.scancode] = true;
                    Console.WriteLine($"Pressed {e.key.keysym.scancode}");
                    isKeyPressHandled = false;
                    break;
                case SDL_EventType.SDL_KEYUP:
                    KeyboardState[(int)e.key.keysym.scancode] = false;
                    Console.WriteLine($"Released {e.key.keysym.scancode}");
                    break;
            }
        }
    }

    /// <summary>
    ///     Renders to the window.
    /// </summary>
    private static void Render()
    {
        DrawStringToFramebuffer(" PC  AC XR YR SP    SR    Instruction:", 0, 0, 0x00FFFFFF);
        var currentInstruction = new[]
        {
            mainBus.GetData(cpu.ProgramCounter),
            mainBus.GetData((ushort)(cpu.ProgramCounter + 1)),
            mainBus.GetData((ushort)(cpu.ProgramCounter + 2))
        };
        DrawStringToFramebuffer(
            $"{cpu.ProgramCounter:X4} {cpu.A:X2} {cpu.X:X2} {cpu.Y:X2} {cpu.StackPointer:X2} {cpu.StatusRegister:B8} {string.Join(' ', currentInstruction.Select(x => $"{x:X2}"))}",
            0, 16, 0x00FFFFFF);
        DrawStringToFramebuffer($"                          {DisAsm6502.Disassemble(currentInstruction, 0)}          ",
            0, 32,
            0x00FFFFFF);

        if (display.IsInTextMode)
        {
            display.RenderTextToFrameBuffer();
        }
        else
        {
            if (!display.IsGraphicsHighResolution)
                display.RenderLowResBufferToFrameBuffer();
        }

        var zoomedBuffer = Enumerable.Repeat<uint>(0, WindowWidth * WindowHeight).ToArray();
        for (var y = 0; y < WindowHeight; y += 2)
        for (var x = 0; x < WindowWidth; x += 2)
        {
            zoomedBuffer[y * WindowWidth + x] = display.FrameBuffer[y / 2 * (WindowWidth / 2) + x / 2];
            zoomedBuffer[y * WindowWidth + x + 1] = display.FrameBuffer[y / 2 * (WindowWidth / 2) + x / 2];
            zoomedBuffer[(y + 1) * WindowWidth + x] = display.FrameBuffer[y / 2 * (WindowWidth / 2) + x / 2];
            zoomedBuffer[(y + 1) * WindowWidth + x + 1] = display.FrameBuffer[y / 2 * (WindowWidth / 2) + x / 2];
        }

        unsafe
        {
            fixed (uint* buffer = zoomedBuffer)
            {
                var renderBuffer = new IntPtr(buffer);
                var sdlUpdateTexture =
                    SDL_UpdateTexture(texture, (IntPtr)null, renderBuffer, WindowWidth * sizeof(uint));
                if (sdlUpdateTexture != 0)
                    Console.WriteLine($"Error while updating texture: {SDL_GetError()}");
            }
        }

        var sdlRenderClear = SDL_RenderClear(renderer);
        if (sdlRenderClear != 0)
            Console.WriteLine($"Error while clearing renderer: {SDL_GetError()}");
        var sdlRenderCopy = SDL_RenderCopy(renderer, texture, (IntPtr)null, (IntPtr)null);
        if (sdlRenderCopy != 0)
            Console.WriteLine($"Error while copying texture: {SDL_GetError()}");
        SDL_RenderPresent(renderer);
    }

    private static void DrawStringToFramebuffer(string text, uint x, uint y, uint foreground)
    {
        for (var i = 0; i < text.Length; i++)
            display.DrawCharacterToFrameBuffer8X16(charset[text[i] + 128], (uint)(x + 8 * i),
                y, foreground);
    }

    /// <summary>
    ///     Clean up the resources that were created.
    /// </summary>
    private static void CleanUp()
    {
        SDL_DestroyTexture(texture);
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }

    private static void Main(string[] args)
    {
        Setup();

        while (running)
        {
            PollEvents();
            UpdateSystemState();
            Render();
        }

        CleanUp();
    }

    private static ulong inputCounter = 0;
    
    private static void UpdateSystemState()
    {
        if (!isKeyPressHandled)
        {
            if (KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_F5])
            {
                cpu.Reset();
                isKeyPressHandled = true;
            }
            else if (KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_F10])
            {
                if (!isCpuRunning)
                    cpu.Step();
                isKeyPressHandled = true;
            }
            else if (KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_F11])
            {
                isCpuRunning = !isCpuRunning;
                isKeyPressHandled = true;
            }
            else if (KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_F2])
            {
                if (inputBuffer.Count == 0)
                    ("10 REM PLOT APPLE\r20 REM\r30 GR\r40 COLOR= 4\r50 PLOT 20,10\r60 VLIN 11,14 AT 21\r70 COLOR= 12\r" +
                     "80 HLIN 17,19 AT 13\r90 HLIN 24,26 AT 13\r100 HLIN 16,20 AT 14\r110 HLIN 23,27 AT 14\r120 HLIN 15,27 AT 15" +
                     "130 COLOR= 13\r140 HLIN 15,26 AT 16\r150 HLIN 15,25 AT 17\r160 HLIN 14,25 AT 18\r170 COLOR= 9\r" +
                     "180 HLIN 14,25 AT 19\r190 HLIN 14,25 AT 20\r200 HLIN 14,26 AT 21\r210 COLOR= 1\r220 HLIN 14,26 AT 22" +
                     "230 HLIN 14,27 AT 23\r240 HLIN 14,27 AT 24\r250 COLOR= 3\r260 15,26 AT 25\r270 HLIN 16,25 AT 26\r" +
                     "280 HLIN 16,25 AT 27\r290 COLOR= 6\r300 HLIN 17,24 AT 28\r310 HLIN 17,24 AT 29\r320 HLIN 18,19 AT 30\r" +
                     "330 HLIN 22,23 AT 30\r").ToCharArray().ToList().ForEach(x => inputBuffer.Enqueue(x));
                isKeyPressHandled = true;
            }
            else
            {
                var pressedKey = 0;
                while (!KeyboardState[pressedKey] && pressedKey < 128) pressedKey++;

                if (pressedKey == 128)
                {
                    isKeyPressHandled = true;
                }
                else if (KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_LSHIFT] ||
                         KeyboardState[(int)SDL_Scancode.SDL_SCANCODE_RSHIFT])
                {
                    if (shiftedScancodes[pressedKey] > 0)
                        isKeyPressHandled = PressKey(shiftedScancodes[pressedKey]);
                }
                else
                {
                    if (nonShiftedScancodes[pressedKey] > 0)
                        isKeyPressHandled = PressKey(nonShiftedScancodes[pressedKey]);
                }
            }
        }
        else
        {
            if (inputBuffer.Count > 0 && inputCounter % 4 == 0)
                isKeyPressHandled = PressKey((byte)inputBuffer.Dequeue());
            inputCounter++;
        }

        if (isCpuRunning)
            for (var i = 0; i < 16666; i++)
                cpu.Exec();

        var now = SDL_GetTicks();
        if (nextFrame <= now)
            SDL_Delay(0);
        else
            SDL_Delay(nextFrame - now);

        nextFrame += FrameTicks;
        cursorFrameCount += 1;
        if (cursorFrameCount == 15)
        {
            cursorFrameCount = 0;
            display.CharacterGenerator.IsBlinkInversed = !display.CharacterGenerator.IsBlinkInversed;
        }
    }

    private static bool PressKey(byte character)
    {
        mainBus.SetData((byte)(character + 0x80), 0xC000);
        return true;
    }
}