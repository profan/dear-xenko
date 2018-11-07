using System;
using Xenko.Core;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Input;

using ImGuiNET;

namespace DearXenko {
    /// <summary>
    /// A script that allows to move and rotate an entity through keyboard, mouse and touch input to provide basic camera navigation.
    /// </summary>
    /// <remarks>
    /// The entity can be moved using W, A, S, D, Q and E, arrow keys or dragging/scaling using multi-touch.
    /// Rotation is achieved using the Numpad, the mouse while holding the right mouse button, or dragging using single-touch.
    /// </remarks>
    public class BasicCameraController : SyncScript {
        private const float MaximumPitch = MathUtil.PiOverTwo * 0.99f;

        private Vector3 upVector;
        private Vector3 translation;
        private Vector3 planeTranslation;
        private float yaw;
        private float pitch;

        float mouseZoomSpeed = 10.0f;
        float mousePanSpeed  = 500.0f;

        Vector3 keyboardMovementSpeed = new Vector3(5.0f);
        Vector2 keyboardRotationSpeed  = new Vector2(3.0f);
        Vector2 mouseRotationSpeed = new Vector2(90.0f, 60.0f);

        bool invertPan = true;
        bool invertZoom = true;
        bool scaleZoom = true;
        float scaleZoomFactor = 10.0f;
        float camDamping = 0.82f;
        float camRotDamping = 0.75f;

        float cameraMinY = 0.0f;
        float cameraMaxY = 100.0f;

        Vector3 initialCameraPosition;

        public override void Start() {
            base.Start();

            // Default up-direction
            upVector = Vector3.UnitY;

            // Initial camera position
            initialCameraPosition = Entity.Transform.Position;

        }

        public override void Update() {
            DrawCameraDebug();
            ProcessInput();
            UpdateTransform();
        }

        private void DrawCameraDebug() {

            ImGui.Text("Camera Options");
            ImGui.Separator();
            ImGui.DragFloat("camera translation damping: ", ref camDamping, 0.01f, 0.1f, 1.0f);
            ImGui.DragFloat("camera rotation damping: ", ref camRotDamping, 0.01f, 0.1f, 1.0f);
            ImGui.DragFloat("scale zoom divisor", ref scaleZoomFactor);
            ImGui.DragFloat("zoom speed", ref mouseZoomSpeed);
            ImGui.DragFloat("pan speed", ref mousePanSpeed);
            ImGui.DragFloat("min height", ref cameraMinY);
            ImGui.DragFloat("max height", ref cameraMaxY);

            if (ImGui.Button((scaleZoom ? "scale pan by zoom: true" : "scale pan by zoom: false"))) {
                scaleZoom = !scaleZoom;
            }

            if (ImGui.Button((invertZoom ? "invert zoom: true" : "invert zoom: false"))) {
                invertZoom = !invertZoom;
            }

            if (ImGui.Button((invertPan ? "invert pan: true" : "invert pan: false"))) {
                invertPan = !invertPan;
            }

            if (ImGui.Button("reset camera position")) {
                Entity.Transform.Position = initialCameraPosition;
            }

        }

        private void ProcessInput() {
            var elapsedTime = Game.UpdateTime.Elapsed.TotalSeconds;
            planeTranslation *= (float)(camDamping);
            translation *= (float)(camDamping);
            pitch *= (float)(camRotDamping);
            yaw *= (float)(camRotDamping);

            // Move with keyboard
            if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up)) {
                planeTranslation.Z += -keyboardMovementSpeed.Z;
            } else if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down)) {
                planeTranslation.Z += keyboardMovementSpeed.Z;
            }

            if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left)) {
                planeTranslation.X += -keyboardMovementSpeed.X;
            } else if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right)) {
                planeTranslation.X += keyboardMovementSpeed.X;
            }

            // Rotate with keyboard
            if (Input.IsKeyDown(Keys.NumPad2)) {
                pitch += keyboardRotationSpeed.X;
            } else if (Input.IsKeyDown(Keys.NumPad8)) {
                pitch += -keyboardRotationSpeed.X;
            }

            if (Input.IsKeyDown(Keys.NumPad4)) {
                yaw += keyboardRotationSpeed.Y;
            } else if (Input.IsKeyDown(Keys.NumPad6)) {
                yaw += -keyboardRotationSpeed.Y;
            }

            // Rotate with mouse
            if (Input.IsMouseButtonDown(MouseButton.Right)) {
                Input.LockMousePosition();
                Game.IsMouseVisible = false;

                yaw += -Input.MouseDelta.X * mouseRotationSpeed.X;
                pitch += -Input.MouseDelta.Y * mouseRotationSpeed.Y;
            } else {
                Input.UnlockMousePosition();
                Game.IsMouseVisible = true;
            }

            // Pan with mouse
            if (Input.IsMouseButtonDown(MouseButton.Middle)) {
                Input.LockMousePosition();
                Game.IsMouseVisible = false;

                planeTranslation.Z += Input.MouseDelta.Y * ((invertPan ? -1 : 1) * mousePanSpeed);
                planeTranslation.X += Input.MouseDelta.X * ((invertPan ? -1 : 1) * mousePanSpeed);
            }

            // Zoom with scrollwheel
            foreach (var inputEvent in Input.Events) {
                switch (inputEvent) {
                    case MouseWheelEvent mouseWheelEvent:
                        translation.Z += mouseWheelEvent.WheelDelta * ((invertZoom ? -1 : 1) * mouseZoomSpeed);
                        break;
                }
            }

        }

        private void UpdateTransform() {
            var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            var finalPlaneTranslation = planeTranslation * (elapsedTime * (scaleZoom ? (Entity.Transform.Position.Y / scaleZoomFactor) : 1.0f));
            var finalTranslation = translation * elapsedTime;

            var finalYaw = yaw * elapsedTime;
            var finalPitch = pitch * elapsedTime;

            // Get the local coordinate system
            var rotation = Matrix.RotationQuaternion(Entity.Transform.Rotation);

            // Enforce the global up-vector by adjusting the local x-axis
            var right = Vector3.Cross(rotation.Forward, upVector);
            var up = Vector3.Cross(right, rotation.Forward);

            // Stabilize
            right.Normalize();
            up.Normalize();

            // Adjust pitch. Prevent it from exceeding up and down facing. Stabilize edge cases.
            var currentPitch = MathUtil.PiOverTwo - (float)Math.Acos(Vector3.Dot(rotation.Forward, upVector));
            finalPitch = MathUtil.Clamp(currentPitch + finalPitch, -MaximumPitch, MaximumPitch) - currentPitch;

            // Move in local coordinates
            Entity.Transform.Position += Vector3.TransformCoordinate(finalTranslation, rotation);

            // Yaw around global up-vector, pitch and roll in local space
            Entity.Transform.Rotation *= Quaternion.RotationAxis(right, finalPitch) * Quaternion.RotationAxis(upVector, finalYaw);

            // Move in global coords, transform translation only along plane, not towards it
            var rot = Entity.Transform.Rotation;
            var planeRot = Matrix.RotationQuaternion(new Quaternion(0.0f, rot.Y, 0.0f, rot.W));
            Entity.Transform.Position += Vector3.TransformCoordinate(finalPlaneTranslation, planeRot);

            // Clamp ourselves to the space
            var ourPos = Entity.Transform.Position;
            ourPos.Y = MathUtil.Clamp(ourPos.Y, cameraMinY, cameraMaxY);
            Entity.Transform.Position = ourPos;

        }
    }
}
