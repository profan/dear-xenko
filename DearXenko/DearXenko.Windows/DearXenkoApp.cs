using Xenko.Engine;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Core;
using Xenko.Games;
using System.Diagnostics;
using Xenko.UI.Events;
using Xenko.Input;

using ImGuiNET;

namespace DearXenko {
    class DearXenkoApp {

        class DearGame : Game {

            ImguiController imgui;
            DebugConsole console;

            public DearGame() {
                console = new DebugConsole(Services);
            }

            protected override void BeginRun() {
                base.BeginRun();
                imgui = new ImguiController(Services, GraphicsDeviceManager);
            }

            protected override void Update(GameTime gameTime) {
                imgui.Update(gameTime);
                base.Update(gameTime);
            }

            protected override void EndDraw(bool present) {
                imgui.Draw();
                base.EndDraw(present);
            }

        }

        static void Main(string[] args) {
            using (var game = new DearGame()) {
                game.Run();
            }
        }
    }
}
