using SharpDX;
using SharpDX.Direct3D11;
namespace TopoCS.Shapes
{
    class Triangle : Shape
    {

        public Triangle() : base(6, 3)
        {
        }

        public Triangle(Triangle triangle, Device device) : base(triangle, device)
        {
        }
        override protected void CreateIndices(ref uint[] indices)
        {
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 0;
        }
        override protected void CreateVertices(ref VertexType[] vertices)
        {

            vertices[0].position = new Vector3(0.0f, 0.5f, 0.0f); //t
            vertices[1].position = new Vector3(0.5f, -0.5f, 0.0f); //r
            vertices[2].position = new Vector3(-0.5f, -0.5f, 0.0f); //l

        }

    }
}
