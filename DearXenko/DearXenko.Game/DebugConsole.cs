using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenko.Core;
using Xenko.Engine;
using Xenko.Input;

using ImGuiNET;
using Xenko.Games;

namespace DearXenko {

    public class DebugConsole : GameSystem {

        private List<string> lines;

        public DebugConsole(IServiceRegistry reg) : base(reg) {
            Services.AddService<DebugConsole>(this);
            lines = new List<string>();
        }

        public void WriteLine(string str) {
            lines.Add(str);
        }

        public override void Update(GameTime gameTime) {
            ImGui.Begin("Debug Console");
            foreach (string line in lines) {
                ImGui.Text(line);
            }
            ImGui.End();
        }

    }

}
