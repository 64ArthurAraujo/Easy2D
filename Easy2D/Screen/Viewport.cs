﻿using OpenTK.Mathematics;
using Easy2D.OpenGL;

namespace Easy2D
{
    public static class Viewport
    {
        public static Vector4i CurrentViewport { get; private set; }

        public static Rectangle Area => 
            new Rectangle(CurrentViewport.X, CurrentViewport.Y, CurrentViewport.Z, CurrentViewport.W);

        public static int X => CurrentViewport.X;
        public static int Y => CurrentViewport.Y;

        public static void SetViewport(int x, int y, int width, int height) => 
            SetViewport(new Vector4i(x, y, width, height));

        public static void SetViewport(Vector4i dimensions)
        {
            if (CurrentViewport != dimensions)
            {
                CurrentViewport = dimensions;
                GLController.Instance.Viewport(dimensions.X, dimensions.Y, (uint)dimensions.Z, (uint)dimensions.W);
                GLController.Instance.Scissor(dimensions.X, dimensions.Y, (uint)dimensions.Z, (uint)dimensions.W);
            }
        }
    }
}
