using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyDuchess.LotPopulation
{
    public static class Utils
    {
        public static bool IsSimPerforming(Sim sim)
        {
            if (sim.LotCurrent == null || sim.LotCurrent == LotManager.sWorldLot)
                return false;
            var showControllers = sim.LotCurrent.GetObjects<IShowController>();
            foreach(var controller in showControllers)
            {
                if (controller.PerformingSim == sim)
                    return true;
            }
            return false;
        }
        public static bool SimFestAtLot(Lot lot)
        {
            var controllers = lot.GetObjects<IShowController>();
            foreach(var controller in controllers)
            {
                if (controller.InSimFest)
                    return true;
            }
            return false;
        }
        public static bool IsCameraInTheSky()
        {
            if (CameraController.IsMapViewModeEnabled())
                return true;
            var cameraPosition = CameraController.GetPosition();
            var cameraFloorPosition = World.SnapToFloor(cameraPosition);
            var heightDifference = cameraPosition.y - cameraFloorPosition.y;
            if (heightDifference >= 400)
                return true;
            return false;
        }

        public static bool IsPointOnScreen(Vector3 point)
        {
            if (!IsPointInFrontOfCamera(point))
                return false;
            var pointOnScreen = GetNormalizedScreenPosition(point);
            if (pointOnScreen.x >= 0f && pointOnScreen.y >= 0f && pointOnScreen.x <= 1f && pointOnScreen.y <= 1f)
                return true;
            return false;
        }

        public static bool IsLotOnScreen(Lot lot)
        {
            var screenBbox = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 1f) };
            var bbox3D = GetLotBoundingBox(lot);
            var inFrontOfCamera = false;
            foreach (var point in bbox3D)
            {
                if (IsPointInFrontOfCamera(point))
                {
                    inFrontOfCamera = true;
                    break;
                }
            }
            if (!inFrontOfCamera)
                return false;
            var bbox2D = GetCornersToNormalizedScreenBox(bbox3D);
            return GetBoxIntersection(screenBbox, bbox2D);
        }

        static bool GetBoxIntersection(Vector2[] bbox1, Vector2[] bbox2)
        {
            if (bbox1[0].x >= bbox2[1].x && bbox1[1].x >= bbox2[1].x)
                return false;
            if (bbox1[0].x <= bbox2[0].x && bbox1[1].x <= bbox2[0].x)
                return false;
            if (bbox1[0].y >= bbox2[1].y && bbox1[1].y >= bbox2[1].y)
                return false;
            if (bbox1[0].y <= bbox2[0].y && bbox1[1].y <= bbox2[0].y)
                return false;
            return true;
        }

        static Vector2[] GetCornersForBox(Vector2[] bbox)
        {
            var topLeft = bbox[0];
            var bottomRight = bbox[1];
            var topRight = new Vector2(bottomRight.x, topLeft.y);
            var bottomLeft = new Vector2(topLeft.x, bottomRight.y);
            return new Vector2[] { topLeft, topRight, bottomLeft, bottomRight };
        }

        static bool IsPointInsideBox(Vector2[] bbox, Vector2 point)
        {
            //return new Vector2[] { topLeftPoint, bottomRightPoint };
            if (point.x >= bbox[0].x && point.y >= bbox[0].y && point.x <= bbox[1].x && point.y <= bbox[1].y)
                return true;
            return false;
        }

        public static Vector2 GetNormalizedScreenPosition(Vector3 position)
        {
            World.GetScreenSpacePos(position, out Vector2 pos);
            var deviceResolution = DeviceConfig.GetCurrentResolution(out bool _);
            pos.x /= deviceResolution.mWidth;
            pos.y /= deviceResolution.mHeight;
            return pos;
        }

        static bool IsPointInFrontOfCamera(Vector3 point)
        {
            var cameraForward = (CameraController.GetTarget() - CameraController.GetPosition()).Normalize();
            var pointDifference = (point - CameraController.GetPosition()).Normalize();
            if (cameraForward * pointDifference < 0)
                return false;
            return true;
        }

        static Vector2[] GetCornersToNormalizedScreenBox(Vector3[] corners)
        {
            var screenSpacePoints = new Vector2[corners.Length];
            for(var i=0;i<corners.Length;i++)
            {
                screenSpacePoints[i] = GetNormalizedScreenPosition(corners[i]);
            }
            var topLeftPoint = Vector2.Invalid;
            var bottomRightPoint = Vector2.Invalid;
            var topLeftSet = false;
            var bottomRightSet = false;
            foreach(var point in screenSpacePoints)
            {
                if (!topLeftSet)
                {
                    topLeftPoint = point;
                    topLeftSet = true;
                }
                else
                {
                    if (point.x <= topLeftPoint.x)
                        topLeftPoint.x = point.x;
                    if (point.y <= topLeftPoint.y)
                        topLeftPoint.y = point.y;
                }

                if (!bottomRightSet)
                {
                    bottomRightPoint = point;
                    bottomRightSet = true;
                }
                else
                {
                    if (point.x >= bottomRightPoint.x)
                        bottomRightPoint.x = point.x;
                    if (point.y >= bottomRightPoint.y)
                        bottomRightPoint.y = point.y;
                }
            }

            
            return new Vector2[] { topLeftPoint, bottomRightPoint };
        }

        public static Vector3[] GetLotBoundingBox(Lot lot)
        {
            var height = 100f;
            var box = new Vector3[8];

            //Bottom Corners
            box[0] = lot.Corners[0];
            box[1] = lot.Corners[1];
            box[2] = lot.Corners[2];
            box[3] = lot.Corners[3];

            //Top Corners
            box[4] = lot.Corners[0];
            box[4].y += height;
            box[5] = lot.Corners[1];
            box[5].y += height;
            box[6] = lot.Corners[2];
            box[6].y += height;
            box[7] = lot.Corners[3];
            box[7].y += height;

            return box;
        }
    }
}
