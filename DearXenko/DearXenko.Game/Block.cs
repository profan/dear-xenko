using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DearXenko {
    public class Block {
        public enum Type : byte {
            Air = 0,
            Stone,
            Dirt,
            Grass,
            Water
        }
    }
}
