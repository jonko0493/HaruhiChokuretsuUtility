﻿using HaruhiChokuretsuLib.NDS.Overlay;
using HaruhiChokuretsuLib.Util;
using Mono.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace HaruhiChokuretsuCLI
{
    public class PatchOverlaysCommand : Command
    {
        private string _inputOverlaysDirectory, _outputOverlaysDirectory, _overlaySourceDir, _romInfoPath;
        private bool _showHelp;

        public PatchOverlaysCommand() : base("patch-overlays", "Patches the game's overlays")
        {
            Options = new()
            {
                "Patches the game's overlays given a Riivolution-style XML file and a rominfo.xml",
                "Usage: HaruhiChokuretsuCLI patch-overlays -i [inputOverlaysDirectory] -o [outputOverlaysDirectory] -p [overlayPatch] -r [romInfo]",
                "",
                { "i|input-overlays=", "Directory containing unpatched overlays", i => _inputOverlaysDirectory = i },
                { "o|output-overlays=", "Directory where patched overlays will be written", o => _outputOverlaysDirectory = o },
                { "s|source-dir=", "Directory where overlay source code lives", s => _overlaySourceDir = s },
                { "r|rom-info=", "rominfo.xml file containing the overlay table", r => _romInfoPath = r },
                { "h|help", "Shows this help screen", h => _showHelp = true },
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            Options.Parse(arguments);
            ConsoleLogger log = new();

            if (_showHelp || string.IsNullOrEmpty(_inputOverlaysDirectory) || string.IsNullOrEmpty(_outputOverlaysDirectory) || string.IsNullOrEmpty(_overlaySourceDir) || string.IsNullOrEmpty(_romInfoPath))
            {
                int returnValue = 0;
                if (string.IsNullOrEmpty(_inputOverlaysDirectory))
                {
                    CommandSet.Out.WriteLine("Input overlays directory not provided, please supply -i or --input-overlays");
                    returnValue = 1;
                }
                if (string.IsNullOrEmpty(_outputOverlaysDirectory))
                {
                    CommandSet.Out.WriteLine("Output overlays directory not provided, please supply -o or --output-overlays");
                    returnValue = 1;
                }
                if (string.IsNullOrEmpty(_overlaySourceDir))
                {
                    CommandSet.Out.WriteLine("Overlay source directory not provided, please supply -s or --source-dir");
                    returnValue = 1;
                }
                if (string.IsNullOrEmpty(_romInfoPath))
                {
                    CommandSet.Out.WriteLine("rominfo.xml not provided, please supply -r or --rom-info");
                    returnValue = 1;
                }
                Options.WriteOptionDescriptions(CommandSet.Out);
                return returnValue;
            }

            if (!Directory.Exists(_outputOverlaysDirectory))
            {
                Directory.CreateDirectory(_outputOverlaysDirectory);
            }

            List<Overlay> overlays = new();
            foreach (string file in Directory.GetFiles(_inputOverlaysDirectory))
            {
                overlays.Add(new(file, _romInfoPath, log));
            }

            foreach (Overlay overlay in overlays)
            {
                if (Directory.GetDirectories(_overlaySourceDir).Contains(Path.Combine(_overlaySourceDir, overlay.Name)))
                {
                    OverlayAsmHack.Insert(_overlaySourceDir, overlay, _romInfoPath,
                        (object sender, DataReceivedEventArgs e) => log.Log(e.Data),
                        (object sender, DataReceivedEventArgs e) => log.LogWarning(e.Data, lookForErrors: true));
                }
            }

            foreach (Overlay overlay in overlays)
            {
                overlay.Save(Path.Combine(_outputOverlaysDirectory, $"{overlay.Name}.bin"));
            }

            return 0;
        }
    }
}
