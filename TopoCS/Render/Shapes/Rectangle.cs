using SharpDX;

namespace TopoCS.Shapes
{
    class Rectangle : Shape
    {
        public Rectangle() : base(12, 4)
        {
        }

        public Rectangle(Rectangle rectangle, D3DX d3dx) : base(rectangle, d3dx)
        {
        }

        override protected void CreateIndices(ref uint[] indices)
        {
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 3;
            indices[4] = 2;
            indices[5] = 1;

            indices[6] = 0;
            indices[7] = 2;
            indices[8] = 1;

            indices[9] = 3;
            indices[10] = 1;
            indices[11] = 2;
        }
        override protected void CreateVertices(ref VertexType[] vertices)
        {
            vertices[0].position = new Vector3(-0.5f, 0.5f, 0.0f); //tl
            vertices[1].position = new Vector3(0.5f, 0.5f, 0.0f); //tp
            vertices[2].position = new Vector3(-0.5f, -0.5f, 0.0f); //bl
            vertices[3].position = new Vector3(0.5f, -0.5f, 0.0f); //br
        }
    }
}
