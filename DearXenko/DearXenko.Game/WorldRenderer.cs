using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenko.Core;
using Xenko.Core.Annotations;
using Xenko.Rendering;
using Xenko.Graphics;
using Xenko.Games;
using Xenko.Core.Mathematics;
using Xenko.Core.Collections;
using Xenko.Engine;

namespace DearXenko {
    public class WorldRenderer : SyncScript {

        internal struct ChunkVertex {
            byte x, y, z;
            byte normal;
            byte uvX, uvY;
        }

        internal class ChunkRenderer {

        }

        internal struct ChunkData {
            public Mesh Mesh { get; set; }
        }

        // dependencies
        GraphicsDevice device;
        GraphicsDeviceManager deviceManager;
        GraphicsContext context;
        CommandList commandList;

        // our materialoo
        Material material;

        // storin dems dere gpu data
        Dictionary<Int3, ChunkData> chunkData;

        // world data
        World world;

        public override void Start() {

            world = new World(8, 8);
            chunkData = new Dictionary<Int3, ChunkData>();

            device = GraphicsDevice;
            Debug.Assert(device != null, "WorldRenderer: GraphicsDevice must be available!");

            context = Services.GetService<GraphicsContext>();
            Debug.Assert(context != null, "WorldRenderer: GraphicsContext must be available!");

            PrepareDeviceObjects();
            GenerateChunkData();

            // first gen
            world.dirty = true;

        }

        void PrepareDeviceObjects() {
            commandList = context.CommandList;
            // material = Content.Load<Material>("")
        }

        public void GenerateChunkData() {

            ChunkData PrepareChunkData() {

                var chunkVtxCount = 16;
                var chunkIdxCount = 16;

                var vertexBuffer = Xenko.Graphics.Buffer.New<VertexPositionNormalColor>(device, chunkVtxCount, BufferFlags.VertexBuffer, GraphicsResourceUsage.Dynamic);
                var indexBuffer = Xenko.Graphics.Buffer.New<ushort>(device, chunkIdxCount, BufferFlags.IndexBuffer, GraphicsResourceUsage.Dynamic);
                var is32Bits = false;

                var vertexBufferBinding = new VertexBufferBinding(vertexBuffer, VertexPositionNormalColor.Layout, chunkVtxCount);
                var indexBufferBinding = new IndexBufferBinding(indexBuffer, is32Bits, chunkIdxCount);

                var chunkData = new ChunkData() {
                    Mesh = new Mesh() {
                        Draw = new MeshDraw() {
                            PrimitiveType = PrimitiveType.TriangleList,
                            VertexBuffers = new[] { vertexBufferBinding },
                            IndexBuffer = indexBufferBinding,
                            DrawCount = (6 * 6) * (Chunk.SIZE * Chunk.SIZE * Chunk.SIZE)
                        }
                    }
                };

                return chunkData;

            }

            for (int x = 0; x < world.width; ++x) {
                for (int y = 0; y < world.height; ++y) {
                    for (int z = 0; z < world.depth; ++z) {

                        var key = new Int3(x, y, z);
                        if (chunkData.TryGetValue(key, out ChunkData data)) {
                            GenerateChunkMesh(key, ref world[x, y, z], ref data);
                        } else {
                            var newData = PrepareChunkData();
                            chunkData[key] = newData;
                            GenerateChunkMesh(key, ref world[x, y, z], ref newData);
                        }

                    }
                }
            }

        }

        FastList<VertexPositionNormalColor> verts;
        FastList<ushort> indices;

        void GenerateChunkMesh(Int3 pos, ref Chunk c, ref ChunkData data) {

            if (verts == null) verts = new FastList<VertexPositionNormalColor>();
            if (indices == null) indices = new FastList<ushort>();

            void CheckChunkMeshData() {

            }

            void GenerateBox(ref Chunk chunk, int x, int z, int y) {

                if ((Block.Type)chunk[x, y, z] == Block.Type.Air) return;

                {
                    // bottom face
                    if ((Block.Type)chunk[x, y - 1, z] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, -1.0f, 0.0f),
                            Color = Color.Red
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, -1.0f, 0.0f),
                            Color = Color.Red
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, -1.0f, 0.0f),
                            Color = Color.Red
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, -1.0f, 0.0f),
                            Color = Color.Red
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 2)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 1)); // bottom right

                    }

                }

                {
                    // top face
                    if ((Block.Type)chunk[x, y + 1, z] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Green
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Green
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Green
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Green
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 2)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 4)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 1)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 4)); // bottom right

                    }

                }

                {
                    // left face
                    if ((Block.Type)chunk[x - 1, y, z] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Blue
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Blue
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Blue
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Blue
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 2)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 4)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 1)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 4)); // bottom right

                    }

                }

                {
                    // right face
                    if ((Block.Type)chunk[x + 1, y, z] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.HotPink
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.HotPink
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.HotPink
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.HotPink
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 2)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 1)); // bottom right

                    }

                }

                {
                    // front face
                    if ((Block.Type)chunk[x, y, z + 1] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.White
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.White
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.White
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 1.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.White
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 2)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 4)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 1)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 4)); // bottom right

                    }

                }

                {
                    // back face

                    if ((Block.Type)chunk[x, y, z - 1] == Block.Type.Air) {

                        // bottom left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Orange
                        });

                        // top left
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(0.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Orange
                        });

                        // top right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, -1.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Orange
                        });

                        // bottom right
                        verts.Add(new VertexPositionNormalColor() {
                            Position = new Vector3(1.0f + x, 0.0f + y, 0.0f + z),
                            Normal = new Vector3(0.0f, 1.0f, 0.0f),
                            Color = Color.Orange
                        });

                        // first tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 3)); // top left
                        indices.Add((ushort)(verts.Count - 2)); // top right

                        // second tri
                        indices.Add((ushort)(verts.Count - 4)); // bottom left
                        indices.Add((ushort)(verts.Count - 2)); // top right
                        indices.Add((ushort)(verts.Count - 1)); // bottom right

                    }
                }

            }

            for (int x = 0; x < Chunk.SIZE; ++x) {
                for (int y = 0; y < Chunk.SIZE; ++y) {
                    for (int z = 0; z < Chunk.SIZE; ++z) {
                        // GenerateBox(ref c, (pos.X * Chunk.SIZE) + x, (pos.Y * Chunk.SIZE) + y, (pos.Z * Chunk.SIZE) + z);
                        GenerateBox(ref c, x, y, z);
                    }
                }
            }

            // recreate with proper size now, if necessary
            var neededVtxSize = verts.Count * VertexPositionNormalColor.Layout.CalculateSize();
            var neededIdxSize = indices.Count * sizeof(ushort);

            if (neededVtxSize > data.Mesh.Draw.VertexBuffers[0].Buffer.SizeInBytes) {
                var vertexBuffer = Xenko.Graphics.Buffer.New<VertexPositionNormalColor>(device, verts.Count, BufferFlags.VertexBuffer, GraphicsResourceUsage.Dynamic);
                data.Mesh.Draw.VertexBuffers[0] = new VertexBufferBinding(vertexBuffer, VertexPositionNormalColor.Layout, verts.Count);
            }

            if (neededIdxSize > data.Mesh.Draw.IndexBuffer.Buffer.SizeInBytes) {
                var indexBuffer = Xenko.Graphics.Buffer.New<ushort>(device, indices.Count, BufferFlags.IndexBuffer, GraphicsResourceUsage.Dynamic);
                data.Mesh.Draw.IndexBuffer = new IndexBufferBinding(indexBuffer, false, indices.Count);
            }

            data.Mesh.Draw.DrawCount = indices.Count;

            unsafe {
                fixed (VertexPositionNormalColor* vertsPtr = verts.Items) {
                    fixed (ushort* indicesPtr = indices.Items) {

                        data.Mesh.Draw.VertexBuffers[0].Buffer.SetData(commandList, new DataPointer(vertsPtr, neededVtxSize));
                        data.Mesh.Draw.IndexBuffer.Buffer.SetData(commandList, new DataPointer(indicesPtr, neededIdxSize));

                        var newChunk = new Entity(position: new Vector3(pos.X * Chunk.SIZE, pos.Z * Chunk.SIZE, pos.Y * Chunk.SIZE));
                        var chunkModel = new Model { data.Mesh };
                        newChunk.GetOrCreate<ModelComponent>().Model = chunkModel;
                        // chunkModel.Materials.Add(material);
                        Entity.AddChild(newChunk);

                    }
                }
            }

            verts.Clear();
            indices.Clear();
            // TODO: check sanity

        }

        public override void Update() {

            if (world.dirty) {

                for (int x = 0; x < world.width; ++x) {
                    for (int y = 0; y < world.height; ++y) {
                        for (int z = 0; z < world.depth; ++z) {

                            if (world.IsChunkDirty(x, y, z)) {
                                var key = new Int3(x, y, z);
                                var data = chunkData[key];
                                GenerateChunkMesh(key, ref world[x, y, z], ref data);
                                world.SetChunkDirty(x, y, z, false);
                            }

                        }
                    }
                }

                world.dirty = false;

            }

        }

    }
}
