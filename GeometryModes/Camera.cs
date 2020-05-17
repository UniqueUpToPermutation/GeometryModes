﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;

using System.IO;
using PrimType = OpenTK.Graphics.OpenGL.PrimitiveType;
using OpenTK.Input;
using Mat = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vec = MathNet.Numerics.LinearAlgebra.Vector<double>;


namespace GeometryModes
{
    class Camera
    {
        public Vector3 center = Vector3.Zero;
        public Vector3 direction = Vector3.UnitZ;
        public Vector3 up = Vector3.UnitY;
        public Quaternion rotation = Quaternion.Identity;
        public float distanceFromCenter = 1.0f;
        public float rotationSpeed = 1.0f;
        public float moveSpeed = 1.0f;

        public float theta = 0.0f;
        public float phi = 0.0f;

        public Vector3 Position
        {
            get
            {
                return center + Vector3.Transform(direction, rotation) * distanceFromCenter;
            }
        }

        public Matrix4 ViewMatrix
        {
            get
            {
                return Matrix4.LookAt(Position, center, up);
            }
        }

        public void Rotate(float dtheta, float dphi, float dt)
        {
            theta -= dtheta * dt * rotationSpeed;
            phi += dphi * dt * rotationSpeed;
            phi = (float)Math.Max(Math.Min(phi, Math.PI / 2.2f), -Math.PI / 2.2f);

            var side = Vector3.Cross(direction, up);
            var rotX = Quaternion.FromAxisAngle(up, theta);
            var rotY = Quaternion.FromAxisAngle(side, phi);

            rotation = rotX * rotY;
        }

        public void Dolly(float dr, float dt)
        {
            distanceFromCenter += moveSpeed * dr * dt;
            distanceFromCenter = Math.Max(distanceFromCenter, 0.0f);
        }

        public void Pan(float dx, float dy, float dt)
        {
            Vector3 v1 = -Vector3.Transform(direction, rotation);
            Vector3 v2 = Vector3.Cross(v1, up);
            v2.Normalize();
            Vector3 v3 = Vector3.Cross(v2, v1);
            v3.Normalize();

            dt = dt / 3.0f;

            center += -dx * dt * moveSpeed * v2 + dy * dt * moveSpeed * v3;
        }
    }

    enum CameraMoveMode
    {
        None,
        Pan,
        Dolly,
        Rotate
    }

    class CameraController
    {
        public CameraMoveMode moveMode = CameraMoveMode.None;
        public MouseState cameraRotateMouseState;
        public Camera camera;
        public GameWindow parent;

        public CameraController(Camera camera)
        {
            this.camera = camera;
        }

        public void UpdateControl(MouseButtonEventArgs e)
        {
            var state = e.Mouse;

            var bCursorCapture = false;
            if (state.LeftButton == ButtonState.Pressed && state.RightButton == ButtonState.Pressed)
            {
                moveMode = CameraMoveMode.Dolly;
                bCursorCapture = true;
            }
            else if (state.LeftButton == ButtonState.Pressed)
            {
                moveMode = CameraMoveMode.Rotate;
                bCursorCapture = true;
            }
            else if (state.RightButton == ButtonState.Pressed)
            {
                moveMode = CameraMoveMode.Pan;
                bCursorCapture = true;
            }
            else
            {
                moveMode = CameraMoveMode.None;
            }

            if (bCursorCapture)
            {
                parent.CursorVisible = false;
                cameraRotateMouseState = Mouse.GetCursorState();
            }
            else
            {
                parent.CursorVisible = true;
            }
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateControl(e);
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateControl(e);
        }

        public void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            switch (moveMode)
            {
                case CameraMoveMode.Rotate:
                    var state = Mouse.GetCursorState();
                    var dx = state.X - cameraRotateMouseState.X;
                    var dy = state.Y - cameraRotateMouseState.Y;
                    var dt = 1.0f;
                    camera.Rotate(dx, dy, dt);
                    Mouse.SetPosition(cameraRotateMouseState.X, cameraRotateMouseState.Y);
                    break;
                case CameraMoveMode.Dolly:
                    state = Mouse.GetCursorState();
                    dy = state.Y - cameraRotateMouseState.Y;
                    dt = 1.0f;
                    camera.Dolly(dy, dt);
                    Mouse.SetPosition(cameraRotateMouseState.X, cameraRotateMouseState.Y);
                    break;
                case CameraMoveMode.Pan:
                    state = Mouse.GetCursorState();
                    dx = state.X - cameraRotateMouseState.X;
                    dy = state.Y - cameraRotateMouseState.Y;
                    dt = 1.0f;
                    camera.Pan(dx, dy, dt);
                    Mouse.SetPosition(cameraRotateMouseState.X, cameraRotateMouseState.Y);
                    break;
            }
        }

        public void Attach(GameWindow window)
        {
            window.MouseUp += OnMouseUp;
            window.MouseDown += OnMouseDown;
            window.MouseMove += OnMouseMove;
            parent = window;
        }

        public void Unattach(GameWindow window)
        {
            window.MouseUp -= OnMouseUp;
            window.MouseDown -= OnMouseDown;
            window.MouseMove -= OnMouseMove;
        }
    }

}
