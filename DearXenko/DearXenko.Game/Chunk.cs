using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DearXenko {

    public unsafe struct Chunk {
        public const int SIZE = 16;
        fixed byte blocks[SIZE * SIZE * SIZE];

        public byte this[int x, int y, int z] {
            get {
                fixed (byte* b = blocks) {
                    if (x >= SIZE || x < 0 || y >= SIZE || y < 0 || z >= SIZE || z < 0) return 0;
                    var idx = (x * SIZE * SIZE) + (y * SIZE) + z;
                    return b[idx];
                }
            } set {
                fixed (byte* b = blocks) {
                    var idx = (x * SIZE * SIZE) + (y * SIZE) + z;
                    b[idx] = value;
                }
            }
        }
    }

}
