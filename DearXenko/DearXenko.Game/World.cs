using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DearXenko {

    /*
     * We start out with a fixed size world, size defined at construction, depth fixed as 128 initially.
    */
    public class World {

        const int DEFAULT_DEPTH = 128 / Chunk.SIZE;

        public int width;
        public int height;
        public int depth = DEFAULT_DEPTH;
        public bool dirty;

        Chunk[] chunks;
        bool[] dirts;

        // width, height in chunks
        public World(int w, int h) {
            width = w;
            height = h;
            chunks = new Chunk[(width * height) * DEFAULT_DEPTH];
            dirts = new bool[(width * height) * DEFAULT_DEPTH];
            dirty = false;
            PlaceHolderData();
        }

        void PlaceHolderData() {
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    for (int z = 0; z < depth; ++z) {
                        PlaceHolderChunk(ref this[x, y, z]);  
                    }
                }
            }
        }

        void PlaceHolderChunk(ref Chunk c) {
            for (int x = 0; x < Chunk.SIZE; ++x) {
                for (int y = 0; y < Chunk.SIZE; ++y) {
                    for (int z = 0; z < Chunk.SIZE; ++z) {
                        c[x, y, z] = 1;
                    }
                }
            }
        }

        public bool IsChunkDirty(int x, int y, int z) {
            var idx = (x * width * height) + (y * height) + z;
            return dirts[idx];
        }
        
        public void SetChunkDirty(int x, int y, int z, bool v) {
            var idx = (x * width * height) + (y * height) + z;
            dirts[idx] = v;
        }

        /*
        public Chunk this[int x, int y, int z] {
            get {
                var idx = (x * width * height) + (y * height) + z;
                return chunks[idx];
            } set {
                var idx = (x * width * height) + (y * height) + z;
                chunks[idx] = value;
                dirts[idx] = true;
                dirty = true;
            }
        }
        */

        public ref Chunk this[int x, int y, int z] {
            get {
                var idx = (x * width * height) + (y * height) + z;
                return ref chunks[idx];
            }
        }

    }

}
