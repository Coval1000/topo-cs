using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using System.Windows.Forms;
using SharpDX;

namespace TopoCS
{
    class Graph
    {
        public Terrain terrain;
        public Axle AxleX, AxleY, AxleZ;
        public Graph() { }
        public Graph(in Graph graph, Device device)
        {
            terrain = new Terrain(graph.terrain, device);
            AxleX = new Axle(graph.AxleX, device);
            AxleY = new Axle(graph.AxleY, device);
            AxleZ = new Axle(graph.AxleZ, device);
        }
        public bool Initialize(D3DX d3dx, string[] header, string[] data, string heightMapFilename, Plain.ColorMode colorMode = Plain.ColorMode.GreyScale)
        {
            terrain = new Terrain();
            if (!terrain.Initialize(d3dx, header, data, heightMapFilename))
            {
                MessageBox.Show("Could not initialize the terrain object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            terrain.RenderPlot = true;

            AxleX = new Axle();
            if (!AxleX.Initialize(d3dx))
            {
                MessageBox.Show("Could not initialize the x axle object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            AxleY = new Axle();
            if (!AxleY.Initialize(d3dx))
            {
                MessageBox.Show("Could not initialize the y axle object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            AxleZ = new Axle();
            if (!AxleZ.Initialize(d3dx))
            {
                MessageBox.Show("Could not initialize the z axle object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            AxleX.Direction = new Vector3((float)Math.PI, 0.0f, 0.0f);
            AxleX.SetMainDivision(5);
            AxleX.SetSupportDivision(4);
            AxleX.SetMainDivisionLenght(5);
            AxleX.SetSupportDivisionLenght(3);
            AxleX.Position = new Vector3(0, 0.0f, 2.0f);
            //AxleZ_.SetTextOffset(Vector3(100.0f, 100.0f, 100.0f));

            AxleY.SetValuesRange(terrain.Bottom, terrain.Top, "um");
            AxleY.Lenght = terrain.Top - terrain.Bottom;
            AxleY.Position = new Vector3(terrain.Bottom, 2.0f, 2.0f);
            AxleY.Direction = new Vector3(0.0f, 0.0f, (float)Math.PI / 2.0f);

            AxleZ.Direction = new Vector3(0.0f, (float)(-Math.PI) / 2.0f, 0.0f);
            AxleZ.Position = new Vector3(0, 0.0f, 2.0f);

            return true;
        }
        public void Dispose()
        {
            AxleX?.Dispose();
            AxleX = null;

            AxleY?.Dispose();
            AxleY = null;

            AxleZ?.Dispose();
            AxleZ = null;

            terrain?.Dispose();
            terrain = null;
        }
        public bool Render(D3DX d3dx, ColorShader colorShader, MonocolorShader monoShader, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Vector3 rotation)
        {
            bool result;
            result = terrain.Render(d3dx, colorShader, worldMatrix, viewMatrix, projectionMatrix);
            result &= AxleX.Render(d3dx, monoShader, worldMatrix, viewMatrix, projectionMatrix);
            result &= AxleX.RenderBanerLabel(d3dx, worldMatrix, viewMatrix, projectionMatrix, rotation);

            result &= AxleZ.Render(d3dx, monoShader, worldMatrix, viewMatrix, projectionMatrix);
            result &= AxleZ.RenderBanerLabel(d3dx, worldMatrix, viewMatrix, projectionMatrix, rotation);

            var worldMatrixPlaine = worldMatrix;
            var transformMatrix = Matrix.Scaling(terrain.Sensivity, 1.0f, 1.0f);
            worldMatrixPlaine = Matrix.Multiply(worldMatrixPlaine, transformMatrix);
            result &= AxleY.Render(d3dx, monoShader, worldMatrixPlaine, viewMatrix, projectionMatrix);
            result &= AxleY.RenderBanerLabel(d3dx, worldMatrix, viewMatrix, projectionMatrix, rotation);
            return result;
        }
    }
}
